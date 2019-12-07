using System.Net.Http;
using System.Threading.Tasks;

namespace ringba_api_call
{
    public interface IRingbaApiRequester
    {
        /// <summary>
        /// used to send autheticated requests
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> PostAsync(string path, HttpContent content);
    }
}