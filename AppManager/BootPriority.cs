using System;
using System.Collections.Generic;
using System.Text;

namespace AppManager
{
    public enum BootPriority
    {
        Default = 0,
        Finalize = 2,
        Last = 1,
        Common = 0,
        HightPriority = -1,
        Initialize = -2
    }
}
