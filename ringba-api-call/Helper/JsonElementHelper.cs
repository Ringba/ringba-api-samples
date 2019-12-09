using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ringba_api_call.Helper
{
    public static class JsonElementHelper
    {
        public enum ReportDataType
        {
            events,
            columns,
            tags
        }

        public static IDictionary<string, JsonElement> ConvertDataToDictionary(this JsonElement row, ReportDataType ReportDataType, Func<JsonElement, (string, JsonElement)> PopulateItem, bool replace = true)
        {
            var rv = new Dictionary<string, JsonElement>();

            if (row.ValueKind == JsonValueKind.Object && row.TryGetProperty(ReportDataType.ToString(), out JsonElement property) && property.ValueKind == JsonValueKind.Array)
            {
                var rows = property
                    .EnumerateArray()
                    .Select(PopulateItem)
                    .Where(kv => !string.IsNullOrEmpty(kv.Item1));

                foreach (var r in rows)
                {
                    if (!rv.ContainsKey(r.Item1) || replace)
                    {
                        rv[r.Item1] = r.Item2;
                    }
                }
            }

            return rv;
        }

        public static (bool, JsonElement) TryGetJsonElementInPath(this JsonElement element, params string[] paths)
        {
            JsonElement temp = element;

            for (int i = 0; i < paths.Length; i++)
            {
                if (!temp.TryGetProperty(paths[i], out temp))
                {
                    return (false, temp);
                }
            }

            return (true, temp);
        }

        public static IDictionary<string, JsonElement> ConvertColumnsToDictionary(this JsonElement row)
        {
            return row.ConvertDataToDictionary(ReportDataType.columns, (JsonElement r) =>
            {
                if (r.TryGetProperty("name", out var name) && (r.TryGetProperty("formattedValue", out var val) || r.TryGetProperty("original", out val)))
                {
                    return (name.GetString(), val);
                }

                return (string.Empty, new JsonElement());
            });
        }

        public static IDictionary<string, JsonElement> ConvertTagsToDictionary(this JsonElement row)
        {
            return row.ConvertDataToDictionary(ReportDataType.tags, (JsonElement r) =>
            {
                if (r.TryGetProperty("tagName", out var tagName) && r.TryGetProperty("tagType", out var tagType) && (r.TryGetProperty("tagValue", out var val)))
                {
                    return ($"{tagType.GetString()}:{tagName.GetString()}", val);
                }

                return (string.Empty, new JsonElement());
            });
        }

        public static IDictionary<string, JsonElement> ConvertEventsToDictionary(this JsonElement row)
        {
            return row.ConvertDataToDictionary(ReportDataType.events, (JsonElement r) =>
            {
                if (r.TryGetProperty("name", out var name))
                {
                    return (name.GetString(), r);
                }

                return (string.Empty, new JsonElement());
            });
        }
    }
}