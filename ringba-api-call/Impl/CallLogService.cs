using ringba_api_call.Helper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using static ringba_api_call.Helper.JsonElementHelper;

namespace ringba_api_call
{
    public class CallLogService : ICallLogsService
    {
        private readonly IRingbaApiRequester _requester;

        public CallLogService(IRingbaApiRequester requester)
        {
            _requester = requester;
        }

        public async Task<IEnumerable<CallLogRow>> GetCallLogsAsync(DateTime date, int size)
        {
            if (size > 10000)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            string jsonRequest = _GetRequestBodyForCallLogs(date.ToUniversalTime(), 0, size);

            using (HttpResponseMessage result = await _requester.PostAsync("CallLogs/Date", new StringContent(jsonRequest, Encoding.UTF8, "application/json")))
            {
                using (JsonDocument doc = await JsonDocument.ParseAsync(await result.Content.ReadAsStreamAsync()))
                {
                    (bool found, JsonElement callLogElement) = doc.RootElement.TryGetJsonElementInPath("result", "callLog", "data");

                    if (found && callLogElement.ValueKind == JsonValueKind.Array)
                    {
                        return callLogElement
                            .EnumerateArray()
                            .Select(_PullRowFromRecord)
                            .Where(r => r.Id != null)
                            .ToList();
                    }
                    else
                    {
                        return Array.Empty<CallLogRow>();
                    }
                }
            }
        }

        private CallLogRow _PullRowFromRecord(JsonElement row)
        {
            try
            {
                IDictionary<string, JsonElement> columns = row.ConvertColumnsToDictionary();
                IDictionary<string, JsonElement> events = row.ConvertEventsToDictionary();
                IDictionary<string, JsonElement> tags = row.ConvertTagsToDictionary();

                return new CallLogRow
                {
                    Id = columns.GetValue("inboundCallId"),
                    CallerId = columns.GetValue("inboundPhoneNumber"),
                    DialedNumber = columns.GetValue("number"),
                    TargetNumber = events.GetTargetNumberFromEvents(),
                    CallLength = (int)columns.GetDecimalValue("callLengthInSeconds"),
                    ConnectedCallLength = (int)columns.GetDecimalValue("callConnectionLength"),
                    State = tags.GetValue("InboundNumber:Region"),
                    CallTime = DateTimeOffset.FromUnixTimeMilliseconds(columns.GetLongValue("dtStamp")),
                    IsLive = events.IsCallLive()
                };
            }
            catch (Exception ex)
            {
                return new CallLogRow();
            }
        }

        private string _GetRequestBodyForCallLogs(DateTime startTime, int page, int pageSize)
        {
            long past = (int)DateTime.UtcNow.Subtract(startTime).TotalDays;

            var request = new
            {
                dateRange = new { past = past, days = 1 },
                callLog = new { page = page, pageSize = pageSize, sort = "dtStamp", sortDirection = "desc" }
            };

            return JsonSerializer.Serialize(request);
        }
    }
}