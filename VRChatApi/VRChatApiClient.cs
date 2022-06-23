using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReuploaderMod.Misc;
using ReuploaderMod.VRChatApi.Models;

namespace ReuploaderMod.VRChatApi {
    
    public class VRChatApiClient {
        public ObjectStore ObjectStore { get; set; }

        public CustomRemoteConfig CustomRemoteConfig { get; set; }
        public CustomApiUser CustomApiUser { get; set; }
        public CustomApiAvatar CustomApiAvatar { get; set; }
        public CustomApiFile CustomApiFile { get; set; }
        public CustomApiWorld CustomApiWorld { get; set; }

        public HttpClient HttpClient;

        public HttpFactory HttpFactory;

        public bool DebugHttp = false;

        public VRChatApiClient(int objectStoreSize = 15, string hmac = "") {
            ObjectStore = new ObjectStore(objectStoreSize);
            Initialize(hmac);

            CustomRemoteConfig = new CustomRemoteConfig(this);
            CustomApiUser = new CustomApiUser(this);
            CustomApiAvatar = new CustomApiAvatar(this);
            CustomApiWorld = new CustomApiWorld(this);
            CustomApiFile = new CustomApiFile(this);
        }

        private void Initialize(string hmac) {
            ObjectStore["ApiUri"] = new Uri("https://api.vrchat.cloud/api/1/", UriKind.Absolute);
            ObjectStore["CookieContainer"] = new CookieContainer();
            ObjectStore["HttpClientHandler"] = new HttpClientHandler() {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = (CookieContainer) ObjectStore["CookieContainer"],
                //Proxy = new WebProxy("http://localhost:8866", false),
                UseProxy = false
                //WindowsProxyUsePolicy = WindowsProxyUsePolicy.DoNotUseProxy,
                //CookieUsePolicy = CookieUsePolicy.UseSpecifiedCookieContainer,
                //EnableMultipleHttp2Connections = true
            };
            ObjectStore["HttpClient"] = new HttpClient((HttpClientHandler) ObjectStore["HttpClientHandler"], true) {
                BaseAddress = (Uri) ObjectStore["ApiUri"],
                Timeout = TimeSpan.FromMinutes(90)
            };

            HttpClient = (HttpClient) ObjectStore["HttpClient"];

            ObjectStore["HttpFactory"] = new HttpFactory(HttpClient);
            HttpFactory = (HttpFactory) ObjectStore["HttpFactory"];

            //if (string.IsNullOrEmpty(hmac))
            //    hmac = GetDeviceUniqueIdentifier();
            if (string.IsNullOrEmpty(hmac))
                hmac = EasyHash.GetSHA1String(new byte[] {0, 1, 2, 3, 4});
            var requestHeaders = HttpClient.DefaultRequestHeaders;
            requestHeaders.Clear();
            requestHeaders.UserAgent.ParseAdd("VRC.Core.BestHTTP");
            requestHeaders.Host = "api.vrchat.cloud";
            requestHeaders.Add("Origin", "vrchat.com");
            requestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            requestHeaders.Add("X-SDK-Version", "2021.09.30.16.29");
            requestHeaders.Add("X-Platform", "standalonewindows");
            requestHeaders.Add("X-MacAddress", hmac);
        }

        //private string GetDeviceUniqueIdentifier() {
        //    var ret = string.Empty;

        //    var concatStr = string.Empty;
        //    try {
        //        using var searcherBb = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
        //        foreach (var obj in searcherBb.Get())
        //            concatStr += (string) obj.Properties["SerialNumber"].Value ?? string.Empty;

        //        using var searcherBios = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
        //        foreach (var obj in searcherBios.Get())
        //            concatStr += (string) obj.Properties["SerialNumber"].Value ?? string.Empty;

        //        using var searcherOs = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
        //        foreach (var obj in searcherOs.Get())
        //            concatStr += (string) obj.Properties["SerialNumber"].Value ?? string.Empty;

        //        using var sha1 = SHA1.Create();
        //        ret =
        //            string.Join("", sha1.ComputeHash(Encoding.UTF8.GetBytes(concatStr)).Select(b => b.ToString("x2")));
        //    }
        //    catch (Exception e) {
        //        Console.WriteLine(e.ToString());
        //    }

        //    return ret;
        //}

        public string GetApiKey() {
            return (string) ObjectStore["ApiKey"] ?? "";
        }

        public string GetApiKeyAsQuery() {
            if (ObjectStore.ContainsKey("ApiKey"))
                return $"?apiKey={(string) ObjectStore["ApiKey"]}";
            return "";
        }

        public string GetApiKeyAsAdditionalQuery() {
            if (ObjectStore.ContainsKey("ApiKey"))
                return $"&apiKey={(string) ObjectStore["ApiKey"]}";
            return "";
        }

        public string GetOrganizationAsQuery() {
            return "?organization=vrchat";
        }

        public string GetOrganizationAsAdditionalQuery() {
            return "&organization=vrchat";
        }

        public async Task<string> Get(string uri) {
            try {
                HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                var response = await HttpClient.GetAsync(uri).ConfigureAwait(false);
                HttpClient.DefaultRequestHeaders.Remove("Content-Type");
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return responseAsString;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                HttpClient.DefaultRequestHeaders.Remove("Content-Type");
            }

            return null;
        }

        public async Task<T> Get<T>(string uri) where T : class {
            try {
                HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                var response = await HttpClient.GetAsync(uri).ConfigureAwait(false);
                HttpClient.DefaultRequestHeaders.Remove("Content-Type");
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(responseAsString);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                HttpClient.DefaultRequestHeaders.Remove("Content-Type");
            }

            return null;
        }

        public async Task<string> Get(string uri, string parameters) {
            try {
                HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                var response = await HttpClient.GetAsync(uri + parameters).ConfigureAwait(false);
                HttpClient.DefaultRequestHeaders.Remove("Content-Type");
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return responseAsString;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                HttpClient.DefaultRequestHeaders.Remove("Content-Type");
            }

            return null;
        }

        public async Task<T> Get<T>(string uri, string parameters) where T : class {
            try {
                HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                var response = await HttpClient.GetAsync(uri + parameters).ConfigureAwait(false);
                HttpClient.DefaultRequestHeaders.Remove("Content-Type");
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(responseAsString);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                HttpClient.DefaultRequestHeaders.Remove("Content-Type");
            }

            return null;
        }

        public async Task<string> Put(string uri, HttpContent content) {
            try {
                var response = await HttpClient.PutAsync(uri, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return responseAsString;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<string> Put(HttpClient awsHttpClient, string uri, HttpContent content) {
            try {
                var response = await awsHttpClient.PutAsync(uri, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return responseAsString;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<string> Put(HttpClient awsHttpClient, string uri, HttpContent content, List<string> etags) {
            try {
                var response = await awsHttpClient.PutAsync(uri, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    if (response.Headers.TryGetValues("ETag", out var etagsNew)) {
                        etags.AddRange(etagsNew);
                        return responseAsString;
                    }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<HttpResponseMessage> PutAsResponse(string uri, HttpContent content) {
            try {
                var response = await HttpClient.PutAsync(uri, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return response;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<T> Put<T>(string uri, HttpContent content) where T : class {
            try {
                var response = await HttpClient.PutAsync(uri, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(responseAsString);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<string> Put(string uri, string parameters, HttpContent content) {
            try {
                var response = await HttpClient.PutAsync(uri + parameters, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return responseAsString;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<string> Put(string uri, string parameters, HttpContent content, IEnumerable<string> etags) {
            try {
                var response = await HttpClient.PutAsync(uri + parameters, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    if (response.Headers.TryGetValues("ETag", out etags))
                        return responseAsString;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<HttpResponseMessage> PutAsResponse(string uri, string parameters, HttpContent content) {
            try {
                var response = await HttpClient.PutAsync(uri + parameters, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return response;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<T> Put<T>(string uri, string parameters, HttpContent content) where T : class {
            try {
                var response = await HttpClient.PutAsync(uri + parameters, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(responseAsString);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<string> Post(string uri, HttpContent content) {
            try {
                var response = await HttpClient.PostAsync(uri, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return responseAsString;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<T> Post<T>(string uri, HttpContent content) where T : class {
            try {
                var response = await HttpClient.PostAsync(uri, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(responseAsString);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<string> Post(string uri, string parameters, HttpContent content) {
            try {
                var response = await HttpClient.PostAsync(uri + parameters, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return responseAsString;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<T> Post<T>(string uri, string parameters, HttpContent content) where T : class {
            try {
                var response = await HttpClient.PostAsync(uri + parameters, content).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(responseAsString);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<string> Delete(string uri) {
            try {
                var response = await HttpClient.DeleteAsync(uri).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return responseAsString;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<T> Delete<T>(string uri) where T : class {
            try {
                var response = await HttpClient.DeleteAsync(uri).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(responseAsString);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<string> Delete(string uri, string parameters) {
            try {
                var response = await HttpClient.DeleteAsync(uri + parameters).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return responseAsString;
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public async Task<T> Delete<T>(string uri, string parameters) where T : class {
            try {
                var response = await HttpClient.DeleteAsync(uri + parameters).ConfigureAwait(false);
                var responseAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (DebugHttp)
                    Console.WriteLine(responseAsString);
                if (response.StatusCode == HttpStatusCode.Forbidden && IsBanned(responseAsString))
                    throw new Exception("Banned!");

                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<T>(responseAsString);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        private bool IsBanned(string responseString) {
            return responseString.Contains("temporary ban") || responseString.Contains("permanently banned");
        }
    }
}