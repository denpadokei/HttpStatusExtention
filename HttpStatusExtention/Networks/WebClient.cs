using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStatusExtention
{
    public class WebResponse
    {
        public readonly HttpStatusCode StatusCode;
        public readonly string ReasonPhrase;
        public readonly bool IsSuccessStatusCode;

        private readonly byte[] _content;

        internal WebResponse(HttpResponseMessage resp, byte[] body)
        {
            this.StatusCode = resp.StatusCode;
            this.ReasonPhrase = resp.ReasonPhrase;
            this.IsSuccessStatusCode = resp.IsSuccessStatusCode;
            this._content = body ?? Array.Empty<byte>();
        }

        public byte[] ContentToBytes()
        {
            return this._content;
        }

        public string ContentToString()
        {
            return Encoding.UTF8.GetString(this._content);
        }

        public JToken ConvertToJToken()
        {
            return JToken.Parse(this.ContentToString());
        }
    }

    internal static class WebClient
    {
        private static HttpClient _client;
        private static HttpClient Client
        {
            get
            {
                if (_client == null) {
                    Connect();
                }

                return _client;
            }
        }

        private static readonly int RETRY_COUNT = 2;
        private static readonly TimeSpan REQUEST_TIMEOUT = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan RETRY_DELAY = TimeSpan.FromSeconds(1);

        private static void Connect()
        {
            try {
                _client?.Dispose();
            }
            catch (Exception e) {
                Plugin.Log.Error(e);
            }

            _client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 15)
            };
            _ = _client.DefaultRequestHeaders.UserAgent.TryParseAdd($"{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Version}");
        }

        internal static async Task<WebResponse> GetAsync(string url, CancellationToken token)
        {
            try {
                return await SendAsync(HttpMethod.Get, url, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) {
                throw;
            }
            catch (Exception e) {
                Plugin.Log.Error(e);
                return null;
            }
        }

        internal static async Task<byte[]> DownloadImage(string url, CancellationToken token)
        {
            try {
                var response = await SendAsync(HttpMethod.Get, url, token);
                return response?.IsSuccessStatusCode == true ? response.ContentToBytes() : null;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) {
                throw;
            }
            catch (Exception e) {
                Plugin.Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// たぶんもう使わない。
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="token"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        internal static async Task<byte[]> DownloadSong(string hash, CancellationToken token, IProgress<double> progress = null)
        {
            // check if beatsaver url needs to be pre-pended
            try {
                var response = await SendAsync(HttpMethod.Get, hash, token, progress: progress);

                return response?.IsSuccessStatusCode == true ? response.ContentToBytes() : null;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) {
                throw;
            }
            catch (Exception e) {
                Plugin.Log.Error(e);
                return null;
            }
        }

        internal static async Task<WebResponse> SendAsync(HttpMethod methodType, string url, CancellationToken token, IProgress<double> progress = null)
        {
            HttpResponseMessage resp = null;
            byte[] content = null;
            Exception lastException = null;
            try {
                for (var attempt = 0; attempt <= RETRY_COUNT; attempt++) {
                    token.ThrowIfCancellationRequested();
                    if (attempt != 0) {
                        await Task.Delay(RETRY_DELAY, token).ConfigureAwait(false);
                    }

                    resp?.Dispose();
                    resp = null;

                    try {
                        // HttpClient.Timeout はヘッダー受信までしかカバーしないため、
                        // ボディの読み込みも含めたタイムアウトを別に設定する。
                        using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token)) {
                            timeoutCts.CancelAfter(REQUEST_TIMEOUT);

                            using (var req = new HttpRequestMessage(methodType, url)) {
                                resp = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token).ConfigureAwait(false);
                            }

                            Plugin.Log.Debug($"resp code : {resp.StatusCode}");

                            if (resp.IsSuccessStatusCode || resp.StatusCode == HttpStatusCode.NotFound || !ShouldRetry(resp.StatusCode)) {
                                if (resp.IsSuccessStatusCode) {
                                    content = await ReadContentAsync(resp, progress, timeoutCts.Token).ConfigureAwait(false);
                                }
                                break;
                            }
                        }
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested) {
                        throw;
                    }
                    catch (Exception e) {
                        lastException = e;
                        Plugin.Log.Warn($"Request failed (attempt {attempt + 1}/{RETRY_COUNT + 1}): {url} ({e.GetType().Name}: {e.Message})");
                    }
                }

                token.ThrowIfCancellationRequested();

                if (resp == null) {
                    Plugin.Log.Warn($"Request failed: {url}{(lastException == null ? string.Empty : $" ({lastException.Message})")}");
                    return null;
                }

                var result = new WebResponse(resp, content);
                resp.Dispose();
                resp = null;
                return result;
            }
            finally {
                resp?.Dispose();
            }
        }

        private static bool ShouldRetry(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.RequestTimeout) {
                return true;
            }
            if ((int)statusCode == 429) {
                return true;
            }
            return (int)statusCode >= 500;
        }

        private static async Task<byte[]> ReadContentAsync(HttpResponseMessage response, IProgress<double> progress, CancellationToken token)
        {
            using (var memoryStream = new MemoryStream())
            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false)) {
                var buffer = new byte[8192];
                var bytesRead = 0;

                var contentLength = response.Content.Headers.ContentLength;
                long totalRead = 0;

                // send report
                progress?.Report(0);

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0) {
                    token.ThrowIfCancellationRequested();

                    if (contentLength != null) {
                        progress?.Report(totalRead / (double)contentLength);
                    }

                    await memoryStream.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
                    totalRead += bytesRead;
                }

                progress?.Report(1);
                return memoryStream.ToArray();
            }
        }
    }
}
