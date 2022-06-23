using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Policy;
using Newtonsoft.Json;
using ReuploaderMod.Misc;

namespace ReuploaderMod.VRChatApi.Models {
    
    public class CustomApiUser : CustomApiModel {
        #region ApiFields

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("userIcon")]
        public string UserIcon { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("bioLinks")]
        public List<object> BioLinks { get; set; }

        [JsonProperty("pastDisplayNames")]
        public List<object> PastDisplayNames { get; set; }

        [JsonProperty("hasEmail")]
        public bool HasEmail { get; set; }

        [JsonProperty("hasPendingEmail")]
        public bool HasPendingEmail { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("obfuscatedEmail")]
        public string ObfuscatedEmail { get; set; }

        [JsonProperty("obfuscatedPendingEmail")]
        public string ObfuscatedPendingEmail { get; set; }

        [JsonProperty("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("hasBirthday")]
        public bool HasBirthday { get; set; }

        [JsonProperty("unsubscribe")]
        public bool Unsubscribe { get; set; }

        [JsonProperty("friends")]
        public List<string> Friends { get; set; }

        [JsonProperty("friendGroupNames")]
        public List<object> FriendGroupNames { get; set; }

        [JsonProperty("currentAvatarImageUrl")]
        public string CurrentAvatarImageUrl { get; set; }

        [JsonProperty("currentAvatarThumbnailImageUrl")]
        public string CurrentAvatarThumbnailImageUrl { get; set; }

        [JsonProperty("fallbackAvatar")]
        public string FallbackAvatar { get; set; }

        [JsonProperty("currentAvatar")]
        public string CurrentAvatar { get; set; }

        [JsonProperty("currentAvatarAssetUrl")]
        public string CurrentAvatarAssetUrl { get; set; }

        [JsonProperty("accountDeletionDate")]
        public object AccountDeletionDate { get; set; }

        [JsonProperty("acceptedTOSVersion")]
        public int AcceptedTOSVersion { get; set; }

        [JsonProperty("steamId")]
        public string SteamId { get; set; }

        [JsonProperty("steamDetails")]
        public SteamDetails SteamDetails { get; set; }

        [JsonProperty("oculusId")]
        public string OculusId { get; set; }

        [JsonProperty("hasLoggedInFromClient")]
        public bool HasLoggedInFromClient { get; set; }

        [JsonProperty("homeLocation")]
        public string HomeLocation { get; set; }

        [JsonProperty("twoFactorAuthEnabled")]
        public bool TwoFactorAuthEnabled { get; set; }

        [JsonProperty("feature")]
        public Feature Feature { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("developerType")]
        public string DeveloperType { get; set; }

        [JsonProperty("last_login")]
        public DateTime? LastLogin { get; set; }

        [JsonProperty("last_platform")]
        public string LastPlatform { get; set; }

        [JsonProperty("allowAvatarCopying")]
        public bool AllowAvatarCopying { get; set; }

        [JsonProperty("date_joined")]
        public string DateJoined { get; set; }

        [JsonProperty("isFriend")]
        public bool IsFriend { get; set; }

        [JsonProperty("friendKey")]
        public string FriendKey { get; set; }

        [JsonProperty("onlineFriends")]
        public List<object> OnlineFriends { get; set; }

        [JsonProperty("activeFriends")]
        public List<object> ActiveFriends { get; set; }

        [JsonProperty("offlineFriends")]
        public List<string> OfflineFriends { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("worldId")]
        public string WorldId { get; set; }

        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        #endregion

        public CustomApiUser(VRChatApiClient apiClient) : base(apiClient, "users") { }

        public async Task<CustomApiUser> Get(string id) {
            var ret = await ApiClient.HttpFactory.GetAsync<CustomApiUser>(MakeRequestEndpoint() + $"/{id}" + ApiClient.GetApiKeyAsQuery());
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiUser> Login(string usernameOrEmail, string password, Func<CustomApi2FA, CustomApiUser> twoFactorAuth = null) {
            var requestHeaders = ApiClient.HttpClient.DefaultRequestHeaders;
            requestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Uri.EscapeDataString(usernameOrEmail)}:{Uri.EscapeDataString(password)}")));

            var res = await ApiClient.HttpFactory.GetStringAsync("auth/user" + ApiClient.GetApiKeyAsQuery()).ConfigureAwait(false);
            
            if (res.Contains("requiresTwoFactorAuth")) {
                var twoFA = JsonConvert.DeserializeObject<CustomApi2FA>(res);
                twoFA.ApiClient = ApiClient;
                return twoFactorAuth?.Invoke(twoFA);
            }

            CustomApiUser apiUser = JsonConvert.DeserializeObject<CustomApiUser>(res);
            apiUser.ApiClient = ApiClient;
            foreach (Cookie cookie in ((CookieContainer)ApiClient.ObjectStore["CookieContainer"]).GetCookies((Uri)ApiClient.ObjectStore["ApiUri"])) {
                if (cookie.Name.Equals("auth", StringComparison.OrdinalIgnoreCase))
                    ApiClient.ObjectStore["AuthCookie"] = cookie.Value;
            }
            requestHeaders.Remove("Authorization");
            return apiUser;
        }

        public async Task<CustomApiUser> Login(string usernameOrEmail, string password,
            Func<CustomApi2FA, Task<CustomApiUser>> twoFactorAuth = null)
        {
            var httpClient = ApiClient.HttpClient;
            var requestHeaders = httpClient.DefaultRequestHeaders;
            requestHeaders.Add("Authorization",
                "Basic " + Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{Uri.EscapeDataString(usernameOrEmail)}:{Uri.EscapeDataString(password)}")));

            var res = await ApiClient.HttpFactory.GetStringAsync("auth/user" + ApiClient.GetApiKeyAsQuery())
                .ConfigureAwait(false);
            if (res.Contains("Invalid Username or Password") || res.Contains("Missing Credentials"))
            {
                Console.WriteLine("Invalid credentials!");
                return null;
            }

            if (res.Contains("requiresTwoFactorAuth"))
            {
                var twoFA = JsonConvert.DeserializeObject<CustomApi2FA>(res);
                twoFA!.ApiClient = ApiClient;
                if (twoFactorAuth != null)
                    return await twoFactorAuth(twoFA);
            }

            CustomApiUser apiUser = JsonConvert.DeserializeObject<CustomApiUser>(res);
            apiUser.ApiClient = ApiClient;
            foreach (Cookie cookie in ((CookieContainer)ApiClient.ObjectStore["CookieContainer"]).GetCookies(
                (Uri)ApiClient.ObjectStore["ApiUri"]))
            {
                if (cookie.Name.Equals("auth", StringComparison.OrdinalIgnoreCase))
                {
                    ApiClient.ObjectStore["AuthCookie"] = cookie.Value;
                    List<string> list = new List<string>
                    {
                        apiUser.Id,
                        cookie.Value
                    };
                    File.WriteAllLines("auth.txt", list.ToArray());
                }
            }

            requestHeaders.Remove("Authorization");
            return apiUser;
        }

        public async Task<CustomApiUser> LoginWithExistingSession(string id, string authcookie, string twoFactor = "") {
            var cookies = (CookieContainer)ApiClient.ObjectStore["CookieContainer"];
            cookies.Add((Uri)ApiClient.ObjectStore["ApiUri"], new Cookie("auth", authcookie));
            ApiClient.ObjectStore["AuthCookie"] = authcookie;
            if (!string.IsNullOrEmpty(twoFactor)) {
                cookies.Add((Uri)ApiClient.ObjectStore["ApiUri"], new Cookie("twoFactorAuth", twoFactor));
                ApiClient.ObjectStore["TwoFactorAuth"] = twoFactor;
            }

            var apiUserResponse = await ApiClient.HttpFactory.GetResponseAsync(MakeRequestEndpoint() + $"/{id}" + ApiClient.GetApiKeyAsQuery() + ApiClient.GetOrganizationAsAdditionalQuery(), HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            if (!apiUserResponse.IsSuccessStatusCode) {
                Console.WriteLine($"Status: {(int)apiUserResponse.StatusCode} {apiUserResponse.StatusCode}{Environment.NewLine}{await apiUserResponse.Content.ReadAsStringAsync()}");
                ApiClient.ObjectStore["AuthCookie"] = null;
                if (!string.IsNullOrEmpty(twoFactor))
                    ApiClient.ObjectStore["TwoFactorAuth"] = null;
                return null;
            }

            using var contentStream = await apiUserResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var streamReader = new StreamReader(contentStream);
            using var jsonReader = new JsonTextReader(streamReader);
            var apiUser = JsonSerializer.Create().Deserialize<CustomApiUser>(jsonReader);
            apiUser.ApiClient = ApiClient;
            return apiUser;
        }

        public static async Task<CustomApiUser> VerifyTwoFactorAuthCode(CustomApi2FA twoFactorAuth) {
            var apiClient = twoFactorAuth.ApiClient;
            var httpClient = twoFactorAuth.ApiClient.HttpClient;
            var requestHeaders = httpClient.DefaultRequestHeaders;

            var twoFAType = twoFactorAuth.GetFirstSupported2FAType();
            var twoFAContainer = new CustomApi2FA.CustomApi2FAContainer(AskInputAuthCode(twoFAType));

            var twoFARet = await twoFactorAuth.ApiClient.HttpFactory.PostAsync<CustomApi2FAVerify>($"auth/twofactorauth/{twoFAType}/verify" + twoFactorAuth.ApiClient.GetApiKeyAsQuery(), ToJsonContent(JsonConvert.SerializeObject(twoFAContainer))).ConfigureAwait(false);
            if (!twoFARet.Verified) {
                Console.WriteLine("Couldn't verify 2FA!");
                return null;
            }

            var ret = await apiClient.HttpFactory.GetAsync<CustomApiUser>("auth/user" + apiClient.GetApiKeyAsQuery()).ConfigureAwait(false);
            foreach (Cookie cookie in ((CookieContainer)apiClient.ObjectStore["CookieContainer"]).GetCookies((Uri)apiClient.ObjectStore["ApiUri"])) {
                if (cookie.Name.Equals("auth", StringComparison.OrdinalIgnoreCase))
                    apiClient.ObjectStore["AuthCookie"] = cookie.Value;
                if (cookie.Name.Equals("twoFactorAuth", StringComparison.OrdinalIgnoreCase))
                    apiClient.ObjectStore["TwoFactorAuth"] = cookie.Value;
            }
            requestHeaders.Remove("Authorization");
            return ret;
        }

        public async Task<string> Logout() {
            //var cookies = (CookieContainer)ApiClient.ObjectStore["CookieContainer"];
            //foreach (Cookie cookie in cookies.GetCookies((Uri)ApiClient.ObjectStore["ApiUri"])) {
            //    if (cookie.Name.Equals("auth", StringComparison.OrdinalIgnoreCase))
            //        cookie.Expired = true;
            //}
            ApiClient.ObjectStore["AuthCookie"] = null;
            ApiClient.ObjectStore["TwoFactorAuth"] = null;
            return await ApiClient.HttpFactory.PutStringAsync("logout" + ApiClient.GetApiKeyAsQuery(), new StringContent("{}", Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        private static string AskInputAuthCode(string twoFAType) {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Enter 2FA Code ({twoFAType}):");
            string authCode = Console.ReadLine();
            Console.WriteLine(new string('=', 50));
            Console.ResetColor();
            return authCode;
        }
    }

    
    public class SteamDetails { }

    
    public class Feature {
        [JsonProperty("twoFactorAuth")] public bool TwoFactorAuth { get; set; }
    }
}