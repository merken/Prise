using System;
using Avalonia.Controls;

namespace Contract
{
    public interface IAppComponent
    {
        string GetName();
        UserControl Load();
    }
}
