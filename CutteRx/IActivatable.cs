using System;
using System.Threading;
using System.Threading.Tasks;

namespace SheaRx
{
    /// <summary>
    /// Denotes an instance which requires activation.
    /// </summary>
    public interface IActivatable
    {
        ///<summary>
        /// Indicates whether or not this instance is active.
        ///</summary>
        bool IsActive { get; }

        /// <summary>
        /// Activates this instance.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ActivateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deactivates this instance.
        /// </summary>
        /// <param name="close">Indicates whether or not this instance is being closed.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeactivateAsync(bool close, CancellationToken cancellationToken = default);

        /// <summary>
        /// Raised after activation occurs.
        /// </summary>
        event EventHandler<ActivationEventArgs> Activated;

        /// <summary>
        /// Raised before deactivation.
        /// </summary>
        event EventHandler<DeactivationEventArgs> AttemptingDeactivation;

        /// <summary>
        /// Raised after deactivation.
        /// </summary>
        event AsyncEventHandler<DeactivationEventArgs> Deactivated;
    }
}
