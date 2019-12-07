using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ringba_api_call
{
    public interface ICallLogsService
    {
        /// <summary>
        /// returns the call logs for the given time frame
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Task<IEnumerable<(string id, string callerId, string dialedNumber, string targetNumber, int callLength, int connectedCallLength, string state, DateTimeOffset callTime, bool isLive)>> GetCallLogsAsync(DateTime startTime, DateTime? endTime = null);
    }
}