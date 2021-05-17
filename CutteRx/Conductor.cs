using DynamicData;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace SheaRx
{
    /// <summary>
    /// An implementation of <see cref="IConductor"/> that holds on to and activates only one item at a time.
    /// </summary>
    public partial class Conductor<T> : ConductorBaseWithActiveItem<T>, IDisposable
        where T : class, IChild, IEquatable<T>
    {
        public Conductor()
        {
            //this._children = new SourceList<T>().DisposeWith(this._disposables);
        }

        //private readonly ISourceList<T> _children;
        //public override IObservableList<T> Children => this._children.AsObservableList();

        /// <inheritdoc />
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

            var closeResult = await this.CloseStrategy.ExecuteAsync(new[] { this.ActiveItem }, cancellationToken);

            if (closeResult.CloseCanOccur)
            {
                await this.ChangeActiveItemAsync(item, true, cancellationToken);
            }
            else
            {
                this.OnActivationProcessed(item, false);
            }
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
            if (item == null || !item.Equals(this.ActiveItem))
            {
                return;
            }

            var closeResult = await this.CloseStrategy.ExecuteAsync(new[] { this.ActiveItem }, CancellationToken.None);

            if (closeResult.CloseCanOccur)
            {
                await this.ChangeActiveItemAsync(default, close);
            }
        }

        /// <summary>
        /// Called to check whether or not this instance can close.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            var closeResult = await this.CloseStrategy.ExecuteAsync(new[] { this.ActiveItem }, cancellationToken);

            return closeResult.CloseCanOccur;
        }

        /// <summary>
        /// Called when activating.
        /// </summary>
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
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return ScreenExtensions.TryDeactivateAsync(this.ActiveItem, close, cancellationToken);
        }

        #region IDisposable

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private bool _isDisposed = false;

        // use this in derived class
        protected override void Dispose(bool isDisposing)
        // use this in non-derived class
        //protected virtual void Dispose(bool isDisposing)
        {
            if (this._isDisposed)
            {
                return;
            }

            if (isDisposing)
            {
                // free managed resources here

                this._disposables.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below.
            // set large fields to null.

            this._isDisposed = true;
        }

        // remove if in derived class
        //public void Dispose()
        //{
        //    // Do not change this code. Put cleanup code in Dispose(bool isDisposing) above.
        //    this.Dispose(true);
        //}

        #endregion
    }
}
