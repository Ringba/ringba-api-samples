using ringba_api_call;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace call_logs
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string username = "";
            string password = "";
            string accountId = "";
            

            Task.Run(async () =>
            {
                RingbaApiRequester client = await RingbaApiRequester.LoginAsync(username, password, accountId);

                var service = new CallLogService(client);

                foreach (var record in await service.GetCallLogsAsync(DateTime.Now.AddDays(-1)))
                {
                    Console.WriteLine(record.callerId);
                }
            }).Wait();
        }
    }
}