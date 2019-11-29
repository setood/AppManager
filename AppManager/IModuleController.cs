using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppManager
{
    public interface IModuleController
    {
        BootPriority BootPriority { get; }
        int BootOrder { get; }
        Task InitializeAsync();
        Task RunAsync();
        Task ShutdownAsync();
    }
}
