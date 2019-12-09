using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ringba_api_call.Helper
{
    public static class CallLogServiceHelper
    {
        public static bool IsCallLive(this IDictionary<string, JsonElement> events)
        {
            return events.HasValue("ConnectedCall") 
                && !events.HasValue("CompletedCall") 
                && !events.HasValue("EndCallSource");
        }
    }
}
