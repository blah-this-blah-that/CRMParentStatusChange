using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace ParentStatusChangeGenericPlugin
{
    public static class Common
    {
        internal static void WriteToTrace(ITracingService traceService, string message)
        {
            if (traceService != null)
                traceService.Trace(message);
        }
    }
}
