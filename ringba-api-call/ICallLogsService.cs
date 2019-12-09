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
        /// <param name="date">The day of the records to download</param>
        /// <param name="size">the number of records to return</param>
        /// <returns>The call logs</returns>
        Task<IEnumerable<CallLogRow>> GetCallLogsAsync(DateTime date, int size);
    }

    public struct CallLogRow
    {
        public string Id { get; set; }

        public string CallerId { get; set; }

        public string DialedNumber { get; set; }

        public string TargetNumber { get; set; }

        public int CallLength { get; set; }

        public int ConnectedCallLength { get; set; }

        public string State { get; set; }

        public DateTimeOffset CallTime { get; set; }

        public bool IsLive { get; set; }
    }
}