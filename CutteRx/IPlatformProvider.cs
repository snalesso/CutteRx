using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SheaRx
{
    /// <summary>
    /// Interface for platform specific operations that need enlightenment.
    /// </summary>
    public interface IPlatformProvider
    {
        /// <summary>
        ///   Indicates whether or not the framework is in design-time mode.
        /// </summary>
        bool InDesignMode { get; }

        /// <summary>
        /// Whether or not classes should execute property change notications on the UI thread.
        /// </summary>
        bool PropertyChangeNotificationsOnUIThread { get; }

        /// <summary>
        ///   Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        void BeginOnUIThread(Action action);

        /// <summary>
        ///   Executes the action on the UI thread.
        /// </summary>
        /// <param name = "action">The action to execute.</param>
        void OnUIThread(Action action);

        /// <summary>
        ///   Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name = "action">The action to execute.</param>
        Task OnUIThreadAsync(Func<Task> action);

        /// <summary>
        /// Executes the handler the fist time the view is loaded.
        /// </summary>
        /// <param name="handler">The handler.</param>
        void ExecuteOnFirstLoad(Action handler);

        /// <summary>
        /// Get the close action for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model to close.</param>
        /// <param name="dialogResult">The dialog result.</param>
        /// <returns>An <see cref="Func{T, TResult}"/> to close the view model.</returns>
        Func<CancellationToken, Task> GetViewCloseAction(object viewModel, bool? dialogResult);
    }
}
