using System;
using System.ComponentModel;

namespace Cutter.Reactive
{
    /// <summary>
    /// Extends <see cref = "INotifyPropertyChanged" /> such that the change event can be raised by external parties.
    /// </summary>
    public interface INotifyPropertyChangedEx : ReactiveUI.propertychanged
    {
        ///// <summary>
        ///// Enables/Disables property change notification.
        ///// </summary>
        //[Obsolete("User IDisposable")]
        //bool IsNotifying { get; set; }

        ///// <summary>
        ///// Notifies subscribers of the property change.
        ///// </summary>
        ///// <param name = "propertyName">Name of the property.</param>
        //void NotifyOfPropertyChange(string propertyName);

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        void Refresh();
    }
}
