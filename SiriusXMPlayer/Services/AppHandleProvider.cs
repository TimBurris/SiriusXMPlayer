using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace Services
{
    public class AppHandleProvider : Abstractions.IAppHandleProvider
    {
        public IntPtr GetAppHandle()
        {
            return new WindowInteropHelper(Application.Current.MainWindow).Handle;
        }
    }
}
