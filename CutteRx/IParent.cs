using DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SheaRx
{
    /// <summary>
    ///   Interface used to define an object associated to a collection of children.
    /// </summary>
    public interface IParent
    {
        /// <summary>
        ///   Gets the children.
        /// </summary>
        /// <returns>
        ///   The collection of children.
        /// </returns>
        IObservableList<object> Children { get; }
    }

    /// <summary>
    /// Interface used to define a specialized parent.
    /// </summary>
    /// <typeparam name="T">The type of children.</typeparam>
    public interface IParent<T> : IParent
        //where T : class//, IChild
    {
        /// <summary>
        ///   Gets the children.
        /// </summary>
        /// <returns>
        ///   The collection of children.
        /// </returns>
        new IObservableList<T> Children { get; }
    }
}
