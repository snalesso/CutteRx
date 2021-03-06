using System.Threading.Tasks;

namespace SheaRx
{
    /// <summary>
    /// Denotes an object that can be closed.
    /// </summary>
    public interface ICloseable
    {
        /// <summary>
        /// Tries to close this instance.
        /// Also provides an opportunity to pass a dialog result to it's corresponding view.
        /// </summary>
        /// <param name="dialogResult">The dialog result.</param>
        Task TryCloseAsync(bool? dialogResult = null);
    }
}
