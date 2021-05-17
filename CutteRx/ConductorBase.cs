using DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace SheaRx
{
    /// <summary>
    /// A base class for various implementations of <see cref="IConductor"/>.
    /// </summary>
    /// <typeparam name="T">The type that is being conducted.</typeparam>
    public abstract class ConductorBase<T> : Screen, IConductor<T>, IParent<T>, IDisposable
        where T : IChild
    {
        protected ConductorBase()
        {
            this._children = new SourceList<T>().DisposeWith(this._disposables);
        }

        private ICloseStrategy<T> _closeStrategy;
        public ICloseStrategy<T> CloseStrategy
        {
            get => this._closeStrategy ??= new DefaultCloseStrategy<T>();
            set => this._closeStrategy = value;
        }

        protected readonly ISourceList<T> _children;
        public virtual IObservableList<T> Children => this._children;

        IObservableList<object> IParent.Children => this.Children as IObservableList<object>;

        /// <summary>
        /// Occurs when an activation request is processed.
        /// </summary>
        public virtual event EventHandler<ActivationProcessedEventArgs> ActivationProcessed = delegate { };

        /// <summary>
        /// Activates the specified item.
        /// </summary>
        /// <param name="item">The item to activate.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task ActivateItemAsync(T item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deactivates the specified item.
        /// </summary>
        /// <param name="item">The item to close.</param>
        /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public abstract Task DeactivateItemAsync(T item, bool close, CancellationToken cancellationToken = default);

        public Task ActivateItemAsync(object item, CancellationToken cancellationToken)
        {
            return this.ActivateItemAsync((T)item, cancellationToken);
        }

        public Task DeactivateItemAsync(object item, bool close, CancellationToken cancellationToken)
        {
            return this.DeactivateItemAsync((T)item, close, cancellationToken);
        }

        /// <summary>
        /// Called by a subclass when an activation needs processing.
        /// </summary>
        /// <param name="item">The item on which activation was attempted.</param>
        /// <param name="success">if set to <c>true</c> activation was successful.</param>
        protected virtual void OnActivationProcessed(T item, bool success)
        {
            if (item == null)
                return;

            ActivationProcessed?.Invoke(this, new ActivationProcessedEventArgs
            {
                Item = item,
                Success = success
            });
        }

        /// <summary>
        /// Ensures that an item is ready to be activated.
        /// </summary>
        /// <param name="newItem">The item that is about to be activated.</param>
        /// <returns>The item to be activated.</returns>
        protected virtual T EnsureItem(T newItem)
        {
            if (newItem is not IChild node)
                // TODO: define a better exception
                throw new NotSupportedException(nameof(newItem));

            if (node.Parent != this)
                node.Parent = this;

            return newItem;
        }

        #region IDisposable

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private bool _isDisposed = false;

        protected virtual void Dispose(bool isDisposing)
        {
            if (this._isDisposed)
                return;

            if (isDisposing)
            {
                this._disposables.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below.
            // set large fields to null.

            this._isDisposed = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        #endregion
    }
}
