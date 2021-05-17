using ReactiveUI;
using System;
using System.Collections.Generic;

namespace Cutter.Reactive
{
    /// <summary>
    /// A base implementation of <see cref = "IViewAware" /> which is capable of caching views by context.
    /// </summary>
    public class ViewAware : ReactiveObject
    {
        /// <summary>
        /// Creates an instance of <see cref="ViewAware"/>.
        /// </summary>
        public ViewAware()
        {
        }
    }
}
