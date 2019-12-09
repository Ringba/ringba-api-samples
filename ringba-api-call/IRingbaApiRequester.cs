using System.Net.Http;
using System.Threading.Tasks;

namespace ringba_api_call
{
    public interface IRingbaApiRequester
    {
        /// <summary>
        /// used to send autheticated requests
        /// </summary>
        /// <param name="path">the path to the api</param>
        /// <param name="content">the body to post</param>
        /// <returns></returns>
        Task<HttpResponseMessage> PostAsync(string path, HttpContent content);
    }
}