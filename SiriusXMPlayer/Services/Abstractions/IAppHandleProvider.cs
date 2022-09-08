using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Abstractions
{
    public interface IAppHandleProvider
    {
        IntPtr GetAppHandle();
    }
}
