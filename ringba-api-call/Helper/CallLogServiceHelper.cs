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
            return !events.HasValue("CompletedCall") && !events.HasValue("EndCallSource");
        }

        public static string GetTargetNumberFromEvents(this IDictionary<string, JsonElement> events)
        {
            if (events.TryGetValue("ConnectedCall", out JsonElement connectedCallEvent))
            {
                IDictionary<string, JsonElement> columns = connectedCallEvent.ConvertColumnsToDictionary();

                return columns.GetValue("targetNumber");
            }

            return string.Empty;
        }

    }
}


