using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SheaRx
{
    /// <summary>
    /// Default implementation for <see cref="IPlatformProvider"/> that does no platform enlightenment.
    /// </summary>
    public class DefaultPlatformProvider : IPlatformProvider
    {
        /// <summary>
        /// Indicates whether or not the framework is in design-time mode.
        /// </summary>
        public virtual bool InDesignMode
        {
            get { return true; }
        }

        /// <summary>
        /// Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public virtual void BeginOnUIThread(Action action)
        {
            action();
        }

        /// <summary>
        /// Executes the action on the UI thread asynchronously.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns></returns>
        public virtual Task OnUIThreadAsync(Func<Task> action)
        {
            return Task.Factory.StartNew(action);
        }

        /// <summary>
        /// Executes the action on the UI thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public virtual void OnUIThread(Action action)
        {
            action();
        }

        /// <summary>
        /// Whether or not classes should execute property change notications on the UI thread.
        /// </summary>
        public virtual bool PropertyChangeNotificationsOnUIThread => true;

        /// <summary>
        /// Executes the handler the fist time the view is loaded.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>true if the handler was executed immediately; false otherwise</returns>
        public virtual void ExecuteOnFirstLoad( Action handler)
        {
            handler();
        }

        /// <summary>
        /// Executes the handler the next time the view's LayoutUpdated event fires.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="handler">The handler.</param>
        public virtual void ExecuteOnLayoutUpdated( Action handler)
        {
            handler();
        }

        /// <summary>
        /// Get the close action for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model to close.</param>
        /// <param name="views">The associated views.</param>
        /// <param name="dialogResult">The dialog result.</param>
        /// <returns>
        /// An <see cref="Action" /> to close the view model.
        /// </returns>
        public virtual Func<CancellationToken, Task> GetViewCloseAction(object viewModel, bool? dialogResult)
        {
            return ct => Task.FromResult(true);
        }
    }
}
