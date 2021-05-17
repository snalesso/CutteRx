using System.Threading.Tasks;

namespace SheaRx
{
    /// <summary>
    /// Extension methods for the <see cref="IActivatable"/> instance.
    /// </summary>
    public static class IActivatableExtensions
    {
        /// <summary>
        /// Activates this instance.
        /// </summary>
        /// <param name="activate">The instance to activate</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task ActivateAsync(this IActivatable activate) => activate.ActivateAsync(default);
        /// <summary>
        /// Deactivates this instance.
        /// </summary>
        /// <param name="deactivate">The instance to deactivate</param>
        /// <param name="close">Indicates whether or not this instance is being closed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task DeactivateAsync(this IActivatable deactivate, bool close) => deactivate.DeactivateAsync(close, default);
    }
}
