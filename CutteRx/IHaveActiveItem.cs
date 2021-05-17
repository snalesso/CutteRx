namespace SheaRx
{
    /// <summary>
    /// Denotes an instance which maintains an active item.
    /// </summary>
    public interface IHaveActiveItem
    {
        /// <summary>
        /// The currently active item.
        /// </summary>
        object ActiveItem { get; set; }
    }

    /// <summary>
    /// Denotes an instance which maintains an active item.
    /// </summary>
    public interface IHaveActiveItem<T> : IHaveActiveItem
    {
        /// <summary>
        /// The currently active item.
        /// </summary>
        new T ActiveItem { get; set; }
    }
}
