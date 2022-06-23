using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReuploaderMod.VRChatApi;
using ReuploaderToolForFriends;

namespace ReuploaderMod.Misc {
    internal static class DownloadHelper {
        private static CancellationTokenSource _cancellationTokenSource;
        private static HttpClientHandler _httpClientHandler;
        //private static StandardSocketsHttpHandler _standardSocketsHttpHandler;
        private static HttpClient _httpClient;
        private static HttpFactory _httpFactory;

        internal static CancellationToken CancellationToken {
            get => _cancellationTokenSource?.Token ?? CancellationToken.None;
        }

        internal static HttpClient HttpClient {
            get => _httpClient;
        }

        public static void Setup() {
            _cancellationTokenSource = new CancellationTokenSource();
            _httpClientHandler = new HttpClientHandler() {
                //Proxy = new WebProxy("http://localhost:8866", false),
                UseProxy = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };
            //_standardSocketsHttpHandler = new StandardSocketsHttpHandler() {
            //    UseProxy = false,
            //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            //};
            _httpClient = new HttpClient(_httpClientHandler, true) { Timeout = TimeSpan.FromMinutes(90) };
            var requestHeaders = _httpClient.DefaultRequestHeaders;
            requestHeaders.Clear();
            requestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            requestHeaders.UserAgent.ParseAdd("UnityPlayer/2019.4.31f1 (UnityWebRequest/1.0, libcurl/7.75.0-DEV)");
            requestHeaders.Add("X-Unity-Version", ReuploadHelper.UnityVersion);
            requestHeaders.Add("X-Client-Version", ReuploadHelper.ClientVersion);
            _httpFactory = new HttpFactory(_httpClient);
        }

        public static byte[] Download(string uri, IProgress<double> progress = null) =>
            _httpFactory.DownloadAsync(uri, CancellationToken, progress).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<byte[]> DownloadAsync(string uri, IProgress<double> progress = null) =>
            await _httpFactory.DownloadAsync(uri, CancellationToken, progress).ConfigureAwait(false);

        public static string DownloadToRandomPath(string uri, IProgress<double> progress = null) =>
            _httpFactory.DownloadToRandomPathAsync(uri, CancellationToken, progress).ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task<string> DownloadToRandomPathAsync(string uri, IProgress<double> progress = null) =>
            await _httpFactory.DownloadToRandomPathAsync(uri, CancellationToken, progress).ConfigureAwait(false);

        public static void Cancel() {
            _cancellationTokenSource.Cancel();
            Thread.Sleep(TimeSpan.FromMilliseconds(250));
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public static void Cleanup() {
            _httpClient?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
