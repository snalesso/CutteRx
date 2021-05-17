using System;
using System.Threading.Tasks;

namespace SheaRx
{
    public delegate Task AsyncEventHandler<TEventArgs>(
        object sender,
        TEventArgs e)
        where TEventArgs : EventArgs;
}
