using System;
using System.Threading;
using System.Threading.Tasks;

namespace SheaRx
{
    /// <summary>
    /// Denotes an instance which conducts other objects by managing an ActiveItem and maintaining a strict lifecycle.
    /// </summary>
    /// <remarks>Conducted instances can optin to the lifecycle by impelenting any of the follosing <see cref="IActivatable"/>, <see cref="IGuardClose"/>.</remarks>
    public interface IConductor : IParent
    {
        /// <summary>
        /// Activates the specified item.
        /// </summary>
        /// <param name="item">The item to activate.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        Task ActivateItemAsync(object item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deactivates the specified item.
        /// </summary>
        /// <param name="item">The item to close.</param>
        /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeactivateItemAsync(object item, bool close, CancellationToken cancellationToken = default);

        /// <summary>
        /// Occurs when an activation request is processed.
        /// </summary>
        event EventHandler<ActivationProcessedEventArgs> ActivationProcessed;
    }

    /// <summary>
    /// Denotes an instance which conducts other objects by managing an ActiveItem and maintaining a strict lifecycle.
    /// </summary>
    /// <remarks>Conducted instances can optin to the lifecycle by impelenting any of the follosing <see cref="IActivatable"/>, <see cref="IDeactivate"/>, <see cref="IGuardClose"/>.</remarks>
    public interface IConductor<T> : IConductor, IParent<T>
    {
        /// <summary>
        /// Activates the specified item.
        /// </summary>
        /// <param name="item">The item to activate.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        Task ActivateItemAsync(T item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deactivates the specified item.
        /// </summary>
        /// <param name="item">The item to close.</param>
        /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeactivateItemAsync(T item, bool close, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// An <see cref="IConductor"/> that also implements <see cref="IHaveActiveItem"/>.
    /// </summary>
    public interface IConductActiveItem : IConductor, IHaveActiveItem
    {
    }

    /// <summary>
    /// An <see cref="IConductor"/> that also implements <see cref="IHaveActiveItem"/>.
    /// </summary>
    public interface IConductActiveItem<T> : IConductor<T>, IHaveActiveItem<T>
    {
    }
}
