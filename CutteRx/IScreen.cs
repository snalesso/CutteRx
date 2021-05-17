namespace SheaRx
{
    /// <summary>
    /// Denotes an instance which implements <see cref="IHaveDisplayName"/>, <see cref="IActivatable"/>, 
    /// <see cref="IDeactivate"/>, <see cref="IGuardClose"/> and <see cref="INotifyPropertyChangedEx"/>
    /// </summary>
    public interface IScreen : IHaveDisplayName, IActivatable, IGuardClose
    {
    }
}
