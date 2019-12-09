using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace ringba_api_call
{
    public sealed class RingbaApiRequester : IRingbaApiRequester, IDisposable
    {
        private static readonly HttpClient _loginClient = new HttpClient()
        {
            BaseAddress = new Uri("https://api.ringba.com/v2/")
        };

        private readonly HttpClient _client;

        private AuthToken _authToken;


        private RingbaApiRequester(AuthToken authToken, string accountId)
        {
            _authToken = authToken;

            _client = new HttpClient()
            {
                BaseAddress = new Uri("https://api.ringba.com/v2/" + accountId + "/")
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_authToken.TokenType, _authToken.AccessToken);
        }

        public async static Task<RingbaApiRequester> LoginAsync(string username, string password, string accountId)
        {
            return new RingbaApiRequester(await _GetToken(new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type","password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            })), accountId);
        }

        public async Task<HttpResponseMessage> PostAsync(string path, HttpContent content)
        {
            await _CheckLoginToken();

            return await _client.PostAsync(path, content);
        }

        private async Task _CheckLoginToken()
        {
            if (_authToken.Expires < DateTimeOffset.UtcNow)
            {
                _authToken = await _GetToken(new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type","refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", _authToken.RefreshToken),
                    new KeyValuePair<string, string>("user_name", _authToken.UserName)
                }));

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_authToken.TokenType, _authToken.AccessToken);
            }
        }

        private async static Task<AuthToken> _GetToken(HttpContent content)
        {
            content.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";

            using HttpResponseMessage result = await _loginClient.PostAsync("Token", content);

            if (result.IsSuccessStatusCode)
            {
                return await _DeserializeAuthToken(result.Content);
            }

            throw new ApplicationException($"Login Failed {result.StatusCode}");
        }

        private async static Task<AuthToken> _DeserializeAuthToken(HttpContent httpContent)
        {
            using Stream stream = await httpContent.ReadAsStreamAsync();

            using JsonDocument doc = await JsonDocument.ParseAsync(stream);

            var token = new AuthToken
            {
                AccessToken = doc.RootElement.GetProperty("access_token").GetString(),
                TokenType = doc.RootElement.GetProperty("token_type").GetString(),
                RefreshToken = doc.RootElement.GetProperty("refresh_token").GetString(),
                UserName = doc.RootElement.GetProperty("userName").GetString(),
            };

            if (DateTimeOffset.TryParse(doc.RootElement.GetProperty(".issued").GetString(), out DateTimeOffset issued))
            {
                token.Issued = issued;
            }

            if (DateTimeOffset.TryParse(doc.RootElement.GetProperty(".expires").GetString(), out DateTimeOffset expires))
            {
                token.Expires = expires;
            }

            return token;
        }

        public void Dispose()
        {
            _authToken = null;
            _client.DefaultRequestHeaders.Authorization = null;
            _client.Dispose();
        }
    }

    internal class AuthToken
    {
        public string AccessToken { get; set; }

        public string TokenType { get; set; }

        public string RefreshToken { get; set; }

        public string UserName { get; set; }

        public DateTimeOffset Issued { get; set; }

        public DateTimeOffset Expires { get; set; }
    }
}