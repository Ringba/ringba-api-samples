using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ringba_api_call.Helper
{
    public static class DictionaryHelper
    {
        public static bool HasValue(this IDictionary<string, JsonElement> dic, string key)
        {
            return dic.TryGetValue(key, out JsonElement value);
        }

        public static string GetValue(this IDictionary<string, JsonElement> dic, string key)
        {
            if (dic.TryGetValue(key, out JsonElement value))
            {
                return value.GetString();
            }

            return string.Empty;
        }

        public static decimal GetDecimalValue(this IDictionary<string, JsonElement> dic, string key)
        {
            try
            {
                if (dic.TryGetValue(key, out JsonElement value) && value.ValueKind == JsonValueKind.Number)
                {
                    return value.GetDecimal();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return 0;
        }

        public static long GetLongValue(this IDictionary<string, JsonElement> dic, string key)
        {
            if (dic.TryGetValue(key, out JsonElement value) && value.ValueKind == JsonValueKind.Number)
            {
                return value.GetInt64();
            }

            return 0;
        }
    }
}