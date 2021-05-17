﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace SheaRx
{
    /// <summary>
    /// Hosts extension methods for <see cref="IScreen"/> classes.
    /// </summary>
    public static class ScreenExtensions
    {
        /// <summary>
        /// Activates the item if it implements <see cref="IActivatable"/>, otherwise does nothing.
        /// </summary>
        /// <param name="potentialActivatable">The potential activatable.</param>
        public static Task TryActivateAsync(object potentialActivatable)
        {
            return potentialActivatable is IActivatable activator ? activator.ActivateAsync(CancellationToken.None) : Task.FromResult(true);
        }

        /// <summary>
        /// Activates the item if it implements <see cref="IActivatable"/>, otherwise does nothing.
        /// </summary>
        /// <param name="potentialActivatable">The potential activatable.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task TryActivateAsync(object potentialActivatable, CancellationToken cancellationToken)
        {
            return potentialActivatable is IActivatable activator ? activator.ActivateAsync(cancellationToken) : Task.FromResult(true);
        }

        /// <summary>
        /// Deactivates the item if it implements <see cref="IActivatable"/>, otherwise does nothing.
        /// </summary>
        /// <param name="potentialDeactivatable">The potential deactivatable.</param>
        /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task TryDeactivateAsync(object potentialDeactivatable, bool close)
        {
            return potentialDeactivatable is IActivatable deactivator ? deactivator.DeactivateAsync(close, CancellationToken.None) : Task.FromResult(true);
        }

        /// <summary>
        /// Deactivates the item if it implements <see cref="IActivatable"/>, otherwise does nothing.
        /// </summary>
        /// <param name="potentialDeactivatable">The potential deactivatable.</param>
        /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task TryDeactivateAsync(object potentialDeactivatable, bool close, CancellationToken cancellationToken)
        {
            return potentialDeactivatable is IActivatable deactivator ? deactivator.DeactivateAsync(close, cancellationToken) : Task.FromResult(true);
        }

        /// <summary>
        /// Closes the specified item.
        /// </summary>
        /// <param name="conductor">The conductor.</param>
        /// <param name="item">The item to close.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task CloseItemAsync(this IConductor conductor, object item)
        {
            return conductor.DeactivateItemAsync(item, true, CancellationToken.None);
        }

        /// <summary>
        /// Closes the specified item.
        /// </summary>
        /// <param name="conductor">The conductor.</param>
        /// <param name="item">The item to close.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task CloseItemAsync(this IConductor conductor, object item, CancellationToken cancellationToken)
        {
            return conductor.DeactivateItemAsync(item, true, cancellationToken);
        }

        /// <summary>
        /// Closes the specified item.
        /// </summary>
        /// <param name="conductor">The conductor.</param>
        /// <param name="item">The item to close.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task CloseItemAsync<T>(this ConductorBase<T> conductor, T item) where T : class, IChild
        {
            return conductor.DeactivateItemAsync(item, true, CancellationToken.None);
        }

        /// <summary>
        /// Closes the specified item.
        /// </summary>
        /// <param name="conductor">The conductor.</param>
        /// <param name="item">The item to close.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static Task CloseItemAsync<T>(this ConductorBase<T> conductor, T item, CancellationToken cancellationToken) where T : class, IChild
        {
            return conductor.DeactivateItemAsync(item, true, cancellationToken);
        }

        ///<summary>
        /// Activates a child whenever the specified parent is activated.
        ///</summary>
        ///<param name="child">The child to activate.</param>
        ///<param name="parent">The parent whose activation triggers the child's activation.</param>
        public static void ActivateWith(this IActivatable child, IActivatable parent)
        {
            var childReference = new WeakReference(child);

            void OnParentActivated(object s, ActivationEventArgs e)
            {
                var activatable = (IActivatable)childReference.Target;
                if (activatable == null)
                    ((IActivatable)s).Activated -= OnParentActivated;
                else
                    activatable.ActivateAsync(CancellationToken.None);
            }

            parent.Activated += OnParentActivated;
        }

        ///<summary>
        /// Deactivates a child whenever the specified parent is deactivated.
        ///</summary>
        ///<param name="child">The child to deactivate.</param>
        ///<param name="parent">The parent whose deactivation triggers the child's deactivation.</param>
        public static void DeactivateWith(this IActivatable child, IActivatable parent)
        {
            var childReference = new WeakReference(child);
            AsyncEventHandler<DeactivationEventArgs> handler = null;
            handler = async (s, e) =>
            {
                var deactivatable = (IActivatable)childReference.Target;
                if (deactivatable == null)
                    ((IActivatable)s).Deactivated -= handler;
                else
                    await deactivatable.DeactivateAsync(e.WasClosed, CancellationToken.None);
            };
            parent.Deactivated += handler;
        }

        ///<summary>
        /// Activates and Deactivates a child whenever the specified parent is Activated or Deactivated.
        ///</summary>
        ///<param name="child">The child to activate/deactivate.</param>
        ///<param name="parent">The parent whose activation/deactivation triggers the child's activation/deactivation.</param>
        public static void ConductWith<TChild, TParent>(this TChild child, TParent parent)
            where TChild : IActivatable
            where TParent : IActivatable
        {
            child.ActivateWith(parent);
            child.DeactivateWith(parent);
        }
    }
}