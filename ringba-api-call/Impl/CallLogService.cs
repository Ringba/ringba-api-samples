using ringba_api_call.Helper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static ringba_api_call.ICallLogsService;

namespace ringba_api_call
{
    public class CallLogService : ICallLogsService
    {
        private IRingbaApiRequester _requester;

        public CallLogService(IRingbaApiRequester requester)
        {
            _requester = requester;
        }

        public async Task<IEnumerable<(string id, string callerId, string dialedNumber, string targetNumber, int callLength, int connectedCallLength, string state, DateTimeOffset callTime, bool isLive)>> GetCallLogsAsync(DateTime startTime, DateTime? endTime = null)
        {
            DateTime end = endTime ?? DateTime.Now;

            end = end.ToUniversalTime();

            startTime = startTime.ToUniversalTime();

            var body = new StringContent(_GetRequestBodyForCallLogs(startTime, end), Encoding.UTF8, "application/json");

            HttpResponseMessage result = await _requester.PostAsync("CallLogs/Date", body);

            using var doc = await JsonDocument.ParseAsync(await result.Content.ReadAsStreamAsync());

            JsonElement callLogElement = doc.RootElement.GetProperty("result").GetProperty("callLog");

            return callLogElement.GetProperty("data")
                .EnumerateArray()
                .Select(r =>
                {
                    try
                    {
                        IDictionary<string, JsonElement> columns = _ConvertColumnsToDictionary(r);
                        IDictionary<string, JsonElement> events = _ConvertEventsToDictionary(r);
                        IDictionary<string, JsonElement> tags = _ConvertTagsToDictionary(r);

                        return (
                        columns.GetValue("inboundCallId"),
                        columns.GetValue("inboundPhoneNumber"),
                        columns.GetValue("number"),
                        columns.GetValue("inboundCallId"),
                        (int)columns.GetDecimalValue("callLengthInSeconds"),
                        (int)columns.GetDecimalValue("callConnectionLength"),
                        tags.GetValue("InboundNumber:Region"),
                        DateTimeOffset.FromUnixTimeMilliseconds(columns.GetLongValue("dtStamp")),
                        events.IsCallLive()
                        );
                    }
                    catch (Exception)
                    {

                        return (null, null, null, null, 0, 0, null, DateTimeOffset.MinValue, false);
                    }
                })
                .Where(r => r.Item1 != null)
                .ToList();
        }

        private IDictionary<string, JsonElement> _ConvertColumnsToDictionary(JsonElement row)
        {
            return _ConvertDataToDictionary(ReportDataType.columns, row, (JsonElement r) =>
            {
                if (r.TryGetProperty("name", out var name) && (r.TryGetProperty("formattedValue", out var val) || r.TryGetProperty("original", out val)))
                {
                    return (name.GetString(), val);
                }

                return (string.Empty, new JsonElement());
            });
        }

        private IDictionary<string, JsonElement> _ConvertEventsToDictionary(JsonElement row)
        {
            return _ConvertDataToDictionary(ReportDataType.events, row, (JsonElement r) =>
            {
                if (r.TryGetProperty("name", out var name) && (r.TryGetProperty("weigth", out var val)))
                {
                    return (name.GetString(), val);
                }

                return (string.Empty, new JsonElement());
            });
        }

        private IDictionary<string, JsonElement> _ConvertTagsToDictionary(JsonElement row)
        {
            return _ConvertDataToDictionary(ReportDataType.tags, row, (JsonElement r) =>
            {
                if (r.TryGetProperty("tagName", out var tagName) && r.TryGetProperty("tagType", out var tagType) && (r.TryGetProperty("tagValue", out var val)))
                {
                    return ($"{tagType.GetString()}:{tagName.GetString()}", val);
                }

                return (string.Empty, new JsonElement());
            });
        }

        private IDictionary<string, JsonElement> _ConvertDataToDictionary(ReportDataType ReportDataType, JsonElement row, Func<JsonElement, (string, JsonElement)> PopulateItem)
        {
            return row.GetProperty(ReportDataType.ToString())
                .EnumerateArray()
                .Select(r =>
                {
                    return PopulateItem(r);
                })
                .Where(kv => !string.IsNullOrEmpty(kv.Item1))
                .ToDictionary(c => c.Item1, c => c.Item2);
        }

        private string _GetRequestBodyForCallLogs(DateTime startTime, DateTime end)
        {
            long days = ((int)end.Subtract(startTime).TotalDays + 1);

            long past = (int)DateTime.UtcNow.Subtract(startTime).TotalDays;

            var request = new
            {
                dateRange = new { past = past, days = days },
                callLog = new { page = 0, pageSize = 100, sort = "dtStamp", sortDirection = "desc" }
            };

            return JsonSerializer.Serialize(request);
        }
    }
}