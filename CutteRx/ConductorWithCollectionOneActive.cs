using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SheaRx
{
    public partial class Conductor<T>
        where T : class, IChild, IEquatable<T>
    {
        /// <summary>
        /// An implementation of <see cref="IConductor"/> that holds on many items.
        /// </summary>
        public sealed partial class With
        {
            /// <summary>
            /// An implementation of <see cref="IConductor"/> that holds on many items but only activates one at a time.
            /// </summary>
            public class OneActive : ConductorBaseWithActiveItem<T>, IDisposable
            {
                //private readonly ISourceList<T> _children = new SourceList<T>();
                private readonly CompositeDisposable _itemsSubscriptions = new();

                /// <summary>
                /// Initializes a new instance of the <see cref="Conductor&lt;T&gt;.With.OneActive"/> class.
                /// </summary>
                public OneActive()
                {
                    var itemsConnection = this.Children.Connect();

                    itemsConnection
                        .OnItemAdded(item =>
                        {
                            IChild child = item as IChild;
                            if (child is not null)
                                child.Parent = this;
                        })
                        .OnItemRemoved(item =>
                        {
                            IChild child = item as IChild;
                            if (child is not null)
                                child.Parent = null;
                        })
                        //.WhereReasonsAre(new ListChangeReason[] {ListChangeReason.Replace } )
                        .SubscribeOn(RxApp.MainThreadScheduler)
                        .Subscribe()
                        .DisposeWith(this._disposables);

                    //.Subscribe((e) =>
                    //{
                    //    switch (e.)
                    //    {
                    //        case NotifyCollectionChangedAction.Add:
                    //            e.NewItems.OfType<IChild>().Apply(x => x.Parent = this);
                    //            break;
                    //        case NotifyCollectionChangedAction.Remove:
                    //            e.OldItems.OfType<IChild>().Apply(x => x.Parent = null);
                    //            break;
                    //        case NotifyCollectionChangedAction.Replace:
                    //            e.NewItems.OfType<IChild>().Apply(x => x.Parent = this);
                    //            e.OldItems.OfType<IChild>().Apply(x => x.Parent = null);
                    //            break;
                    //        case NotifyCollectionChangedAction.Reset:
                    //            this._items.OfType<IChild>().Apply(x => x.Parent = this);
                    //            break;
                    //    }
                    //};
                }

                //public override IObservableList<T> Children => throw new NotImplementedException();

                /// <summary>
                /// Activates the specified item.
                /// </summary>
                /// <param name="item">The item to activate.</param>
                /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                public override async Task ActivateItemAsync(T item, CancellationToken cancellationToken = default)
                {
                    if (item != null && item.Equals(this.ActiveItem))
                    {
                        if (this.IsActive)
                        {
                            await ScreenExtensions.TryActivateAsync(item, cancellationToken);
                            this.OnActivationProcessed(item, true);
                        }

                        return;
                    }

                    await this.ChangeActiveItemAsync(item, false, cancellationToken);
                }

                /// <summary>
                /// Deactivates the specified item.
                /// </summary>
                /// <param name="item">The item to close.</param>
                /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
                /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                public override async Task DeactivateItemAsync(T item, bool close, CancellationToken cancellationToken = default)
                {
                    if (item == null)
                        return;

                    if (!close)
                        await ScreenExtensions.TryDeactivateAsync(item, false, cancellationToken);
                    else
                    {
                        var closeResult = await this.CloseStrategy.ExecuteAsync(new[] { item }, CancellationToken.None);

                        if (closeResult.CloseCanOccur)
                            await this.CloseItemCoreAsync(item, cancellationToken);
                    }
                }

                private async Task CloseItemCoreAsync(T item, CancellationToken cancellationToken = default)
                {
                    if (item.Equals(this.ActiveItem))
                    {
                        var index = this.Children.Items.IndexOf(item);
                        var next = this.DetermineNextItemToActivate(this.Children.Items.ToArray(), index);

                        await this.ChangeActiveItemAsync(next, true);
                    }
                    else
                    {
                        await ScreenExtensions.TryDeactivateAsync(item, true, cancellationToken);
                    }

                    this._children.Remove(item);
                }

                /// <summary>
                /// Determines the next item to activate based on the last active index.
                /// </summary>
                /// <param name="list">The list of possible active items.</param>
                /// <param name="lastIndex">The index of the last active item.</param>
                /// <returns>The next item to activate.</returns>
                /// <remarks>Called after an active item is closed.</remarks>
                protected virtual T DetermineNextItemToActivate(IList<T> list, int lastIndex)
                {
                    var toRemoveAt = lastIndex - 1;

                    if (toRemoveAt == -1 && list.Count > 1)
                        return list[1];

                    if (toRemoveAt > -1 && toRemoveAt < list.Count - 1)
                        return list[toRemoveAt];

                    return default;
                }

                /// <summary>
                /// Called to check whether or not this instance can close.
                /// </summary>
                /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
                {
                    var closeResult = await this.CloseStrategy.ExecuteAsync(this._children.Items.ToArray(), cancellationToken);

                    if (!closeResult.CloseCanOccur && closeResult.Children.Any())
                    {
                        var closable = closeResult.Children;

                        if (closable.Contains(this.ActiveItem))
                        {
                            var list = this._children.Items.ToList();
                            var next = this.ActiveItem;
                            do
                            {
                                var previous = next;
                                next = this.DetermineNextItemToActivate(list, list.IndexOf(previous));
                                list.Remove(previous);
                            } while (closable.Contains(next));

                            var previousActive = this.ActiveItem;
                            await this.ChangeActiveItemAsync(next, true);
                            this._children.Edit(editor => editor.Remove(previousActive));

                            var stillToClose = closable.ToList();
                            stillToClose.Remove(previousActive);
                            closable = stillToClose;
                        }

                        foreach (var deactivate in closable.OfType<IActivatable>())
                        {
                            await deactivate.DeactivateAsync(true, cancellationToken);
                        }

                        this._children.Edit(editor => editor.RemoveMany(closable));
                    }

                    return closeResult.CloseCanOccur;
                }

                /// <summary>
                /// Called when activating.
                /// </summary>
                /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                protected override Task OnActivateAsync(CancellationToken cancellationToken)
                {
                    return ScreenExtensions.TryActivateAsync(this.ActiveItem, cancellationToken);
                }

                /// <summary>
                /// Called when deactivating.
                /// </summary>
                /// <param name="close">Indicates whether this instance will be closed.</param>
                /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
                {
                    if (close)
                    {
                        foreach (var deactivate in this.Children.Items.OfType<IActivatable>())
                        {
                            await deactivate.DeactivateAsync(true, cancellationToken);
                        }

                        this._children.Edit(editor => editor.Clear());
                    }
                    else
                    {
                        await ScreenExtensions.TryDeactivateAsync(this.ActiveItem, false, cancellationToken);
                    }
                }

                /// <summary>
                /// Ensures that an item is ready to be activated.
                /// </summary>
                /// <param name="newItem">The item that is about to be activated.</param>
                /// <returns>The item to be activated.</returns>
                protected override T EnsureItem(T newItem)
                {
                    if (newItem == null)
                    {
                        newItem = this.DetermineNextItemToActivate(
                            this.Children.Items.ToArray(),
                            this.ActiveItem != null
                            ? this.Children.Items.IndexOf(this.ActiveItem)
                            : 0);
                    }
                    else
                    {
                        var index = this.Children.Items.IndexOf(newItem);

                        if (index == -1)
                            this._children.Add(newItem);
                        else
                            newItem = this.Children.Items.ElementAtOrDefault(index);
                    }

                    return base.EnsureItem(newItem);
                }

                #region IDisposable

                private readonly CompositeDisposable _disposables = new CompositeDisposable();
                private bool _isDisposed = false;

                protected override void Dispose(bool isDisposing)
                {
                    if (this._isDisposed)
                        return;

                    if (isDisposing)
                    {
                        // free managed resources here
                        this._disposables.Dispose();
                    }

                    // free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // set large fields to null.

                    this._isDisposed = true;

                    // remove in non-derived class
                    base.Dispose(isDisposing);
                }

                #endregion
            }
        }
    }
}
