using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
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
        public partial class With
        {
            /// <summary>
            /// An implementation of <see cref="IConductor"/> that holds on to many items which are all activated.
            /// </summary>
            public class AllActive : ConductorBase<T>
            {
                //private readonly DynamicData.IObservableList<T> _items = new SourceList<T>();
                private readonly bool _openPublicItems;

                /// <summary>
                /// Initializes a new instance of the <see cref="Conductor&lt;T&gt;.With.AllActive"/> class.
                /// </summary>
                /// <param name="openPublicItems">if set to <c>true</c> opens public items that are properties of this class.</param>
                public AllActive(bool openPublicItems)
                    : this()
                {
                    this._openPublicItems = openPublicItems;
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="Conductor&lt;T&gt;.With.AllActive"/> class.
                /// </summary>
                public AllActive()
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

                    //this._items.CollectionChanged += (s, e) =>
                    //{
                    //    switch (e.Action)
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

                ///// <summary>
                ///// Gets the items that are currently being conducted.
                ///// </summary>
                //public IObservableCollection<T> Items => this._items;

                /// <summary>
                /// Called when activating.
                /// </summary>
                protected override Task OnActivateAsync(CancellationToken cancellationToken)
                {
                    return Task.WhenAll(this.Children.Items.OfType<IActivatable>().Select(x => x.ActivateAsync(cancellationToken)));
                }

                /// <summary>
                /// Called when deactivating.
                /// </summary>
                /// <param name="close">Indicates whether this instance will be closed.</param>
                /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
                {
                    foreach (var deactivate in this.Children.Items.OfType<IActivatable>())
                    {
                        await deactivate.DeactivateAsync(close, cancellationToken);
                    }

                    if (close)
                        this._children.Edit(editor => editor.Clear());
                }

                /// <summary>
                /// Called to check whether or not this instance can close.
                /// </summary>
                /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
                {
                    var closeResult = await this.CloseStrategy.ExecuteAsync(this.Children.Items, cancellationToken);

                    if (!closeResult.CloseCanOccur && closeResult.Children.Any())
                    {
                        foreach (var deactivate in closeResult.Children.OfType<IActivatable>())
                        {
                            await deactivate.DeactivateAsync(true, cancellationToken);
                        }

                        this._children.Edit(editor => editor.RemoveMany(closeResult.Children));
                    }

                    return closeResult.CloseCanOccur;
                }

                /// <summary>
                /// Called when initializing.
                /// </summary>
                protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
                {
                    if (this._openPublicItems)
                        await Task.WhenAll(this.GetType().GetRuntimeProperties()
                            .Where(x => x.Name != "Parent" && typeof(T).GetTypeInfo().IsAssignableFrom(x.PropertyType.GetTypeInfo()))
                            .Select(x => x.GetValue(this, null))
                            .Cast<T>()
                            .Select(i => this.ActivateItemAsync(i, cancellationToken)));
                }

                /// <summary>
                /// Activates the specified item.
                /// </summary>
                /// <param name="item">The item to activate.</param>
                /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
                /// <returns>A task that represents the asynchronous operation.</returns>
                public override async Task ActivateItemAsync(T item, CancellationToken cancellationToken = default)
                {
                    if (item == null)
                        return;

                    item = this.EnsureItem(item);

                    if (this.IsActive)
                        await ScreenExtensions.TryActivateAsync(item, cancellationToken);

                    this.OnActivationProcessed(item, true);
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

                    if (close)
                    {
                        var closeResult = await this.CloseStrategy.ExecuteAsync(new[] { item }, CancellationToken.None);

                        if (closeResult.CloseCanOccur)
                            await this.CloseItemCoreAsync(item, cancellationToken);
                    }
                    else
                        await ScreenExtensions.TryDeactivateAsync(item, false, cancellationToken);
                }

                private async Task CloseItemCoreAsync(T item, CancellationToken cancellationToken = default)
                {
                    await ScreenExtensions.TryDeactivateAsync(item, true, cancellationToken);
                    this._children.Edit(editor => editor.Remove(item));
                }

                /// <summary>
                /// Ensures that an item is ready to be activated.
                /// </summary>
                /// <param name="newItem">The item that is about to be activated.</param>
                /// <returns>The item to be activated.</returns>
                protected override T EnsureItem(T newItem)
                {
                    var index = this.Children.Items.IndexOf(newItem);

                    if (index == -1)
                        this._children.Edit(editor => editor.Add(newItem));
                    else
                        newItem = this.Children.Items.ElementAtOrDefault(index);

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
