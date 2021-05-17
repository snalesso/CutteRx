using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;

namespace SheaRx
{
    /// <summary>
    /// A base class for various implementations of <see cref="IConductor"/> that maintain an active item.
    /// </summary>
    /// <typeparam name="T">The type that is being conducted.</typeparam>
    public abstract class ConductorBaseWithActiveItem<T> : ConductorBase<T>, IConductActiveItem<T>
        where T : class, IChild
    {
        //public ConductorBaseWithActiveItem()
        //{
        //    this._children = new SourceList<T>().DisposeWith(this._disposables);
        //}

        //protected readonly ISourceList<T> _children;
        //public override IObservableList<T> Children => this._children;

        private T _activeItem;
        /// <summary>
        /// The currently active item.
        /// </summary>
        public T ActiveItem
        {
            get => this._activeItem;
            set => this.ActivateItemAsync(value, CancellationToken.None);
        }

        /// <summary>
        /// The currently active item.
        /// </summary>
        /// <value></value>
        object IHaveActiveItem.ActiveItem
        {
            get => this.ActiveItem;
            set => this.ActiveItem = (T)value;
        }

        /// <summary>
        /// Changes the active item.
        /// </summary>
        /// <param name="newItem">The new item to activate.</param>
        /// <param name="closePrevious">Indicates whether or not to close the previous active item.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual async Task ChangeActiveItemAsync(T newItem, bool closePrevious, CancellationToken cancellationToken)
        {
            await ScreenExtensions.TryDeactivateAsync(this._activeItem, closePrevious, cancellationToken);
            var oldItem = this._activeItem;
            newItem = this.EnsureItem(newItem);

            this._activeItem = newItem;
            this.RaisePropertyChanged(nameof(this.ActiveItem));

            if (this.IsActive)
                await ScreenExtensions.TryActivateAsync(newItem, cancellationToken);

            await this.DeactivateItemAsync(oldItem, closePrevious, cancellationToken);

            this.OnActivationProcessed(this._activeItem, true);
        }

        /// <summary>
        /// Changes the active item.
        /// </summary>
        /// <param name="newItem">The new item to activate.</param>
        /// <param name="closePrevious">Indicates whether or not to close the previous active item.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected Task ChangeActiveItemAsync(T newItem, bool closePrevious) => this.ChangeActiveItemAsync(newItem, closePrevious, default);

        #region IDisposable

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private bool _isDisposed = false;

        // use this in derived class
        protected override void Dispose(bool isDisposing)
        // use this in non-derived class
        //protected virtual void Dispose(bool isDisposing)
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
            //base.Dispose(isDisposing);
        }

        //// remove if in derived class
        //public void Dispose()
        //{
        //    // Do not change this code. Put cleanup code in Dispose(bool isDisposing) above.
        //    this.Dispose(true);
        //}

        #endregion
    }
}
