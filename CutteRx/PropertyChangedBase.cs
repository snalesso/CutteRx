using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using ReactiveUI;

namespace Cutter.Reactive
{
    /// <summary>
    /// A base class that implements the infrastructure for property change notification and automatically performs UI thread marshalling.
    /// </summary>
    [DataContract]
    public class PropertyChangedBase : ReactiveObject, INotifyPropertyChangedEx
    {
        /// <summary>
        /// Creates an instance of <see cref = "PropertyChangedBase" />.
        /// </summary>
        public PropertyChangedBase()
        {
        }

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        public virtual void Refresh()
        {
           this.RaisePropertyChanged(string.Empty);
        }
    }
}
