using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventHubBlazor
{
    public enum RunningStateEnum
    {
        Undefined,
        Stopped,
        Starting,
        Started,
        Stopping
    }
}
