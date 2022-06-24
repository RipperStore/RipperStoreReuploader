using Newtonsoft.Json;
using Reuploader.Misc;
using Reuploader.Models;
using Reuploader.VRChatApi;
using Reuploader.VRChatApi.Models;
using RipperStoreReuploader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace RipperStoreReuploader
{
    internal class ReuploadHelper
    {
        internal static object FriendlyName = "";
        internal static string UnityVersion = "2019.4.31f1";
        internal static string ClientVersion = "2022.1.1p1-1170--Release";
        internal static string AvatarID = $"avtr_{Guid.NewGuid()}";
        internal static string Name;
        internal static HttpClient _http = new HttpClient();
        internal static string queue_id;

        public static Config Config { get; set; }

        private VRChatApiClient apiClient;
        private CustomApiUser customApiUser;

        public ReuploadHelper()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            apiClient = new VRChatApiClient(10, GenerateFakeMac());

            if (File.Exists("Config.json")) { Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json")); }
            else { Config = new Config() { apiKey = null, authCookie = null, password = null, username = null }; }

            while (!SetUpVRChat()) { }
            while (!SetUpApiKey()) { }

            File.WriteAllText("Config.json", JsonConvert.SerializeObject(Config));

            Console.WriteLine("> Enter the RipperStore ID (ident) of the Avatar you want to Reupload: ");
            string ident = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("> Enter an Avatar Name: ");
            Name = Console.ReadLine();
            Console.WriteLine("");

            if (Name.Length < 2 || Name.Length > 32)
            {
                Console.WriteLine("| Error, invalid Avatar Name provided");
                Console.ReadLine();
                return;
            }

            FriendlyName = Name;

            SetUpQueue(Config.apiKey, ident, AvatarID);

            var vrcaPath = ReuploadQueue(queue_id);
            var imgPath = ImageDownload(Config.apiKey, ident);

            Console.WriteLine("> Starting Reupload\n");

            ReUploadAvatarAsync(Name, vrcaPath, imgPath, AvatarID).Wait();

            Console.WriteLine($"> Done Reuploading ({Name})");
            Console.Read();
        }

        private void SetUpQueue(string apiKey, string ident, string avatarid)
        {
            var request = _http.PostAsync($"https://worker.ripper.store/api/v1/hotswap-url?apiKey={apiKey}&ident={ident}&avatarid={avatarid}", null).Result;

            switch ((int)request.StatusCode)
            {
                case 201:
                    Console.WriteLine("> Successfully placed in queue, waiting.. (this may take a few minutes)\n");
                    break;
                case 400:
                    Console.WriteLine("| Invalid API Key / ID provided");
                    Console.Read();
                    break;
                case 401:
                    Console.WriteLine("| Invalid API Key Provided, please check https://ripper.store/clientarea > Profile-Settings");
                    Console.Read();
                    break;
                case 402:
                    Console.WriteLine("| You do now own the requested avatar, please purchase before hotswapping");
                    Console.Read();
                    break;
                case 404:
                    Console.WriteLine("| Invalid ID (ident) Provided, ID must be last part of URL");
                    Console.Read();
                    break;
                case 500:
                    Console.WriteLine("| There was an Error (Queue), please try again later");
                    Console.Read();
                    break;
                default:
                    break;
            }

            var _ = request.Content.ReadAsStringAsync().Result.Split('"');
            queue_id = string.Join("", _);
        }
        private bool SetUpApiKey()
        {
            string apiKey;

            if (Config.apiKey == null)
            {
                Console.WriteLine("> RipperStore apiKey: ");
                apiKey = Console.ReadLine();
            }
            else
            {
                apiKey = Config.apiKey;
            }

            var res = _http.GetAsync($"https://api.ripper.store/clientarea/credits/validate?apiKey={apiKey}").Result;
            if ((int)res.StatusCode == 200)
            {
                Config.apiKey = apiKey;
                Console.WriteLine("> Successfully verified apiKey (RipperStore)\n");
                return true;
            }

            Config.apiKey = null;
            Console.WriteLine("| Error, invalid apiKey provided (RipperStore)\n");
            return false;

        }

        private bool SetUpVRChat()
        {
            string username, password;

            if (Config.userID != null && Config.authCookie != null)
            {
                Console.WriteLine("> Trying to login with existing session (VRChat)");
                customApiUser ??= apiClient.CustomApiUser.LoginWithExistingSession(Config.userID, Config.authCookie).Result;

                if (customApiUser == null)
                {
                    Config.userID = null; Config.authCookie = null;
                    Console.WriteLine("| Error, Unable to login with existing session (VRChat) \n");
                    return false;

                }

                Console.WriteLine("> Successfully logged in (VRChat)\n");
                return true;
            }
            else
            {
                if (Config.username == null)
                {
                    Console.WriteLine("> VRChat Username / E-Mail: ");
                    username = Console.ReadLine();
                    Console.WriteLine("");
                }
                else
                {
                    username = Config.username;
                }

                if (Config.password == null)
                {
                    Console.WriteLine("> VRChat Password: ");
                    password = Console.ReadLine();
                    Console.WriteLine("");
                }
                else
                {
                    password = Config.password;
                }

                customApiUser ??= apiClient.CustomApiUser.Login(username, password, CustomApiUser.VerifyTwoFactorAuthCode).Result;

                if (customApiUser.Id == null)
                {
                    Config.username = null; Config.password = null;
                    Console.WriteLine("| Error, Unable to login, invalid credentials\n");
                    return false;

                }

                Config.username = username; Config.password = password;
                Console.WriteLine("> Successfully logged in (VRChat)\n");
                return true;
            }

        }
        private string ReuploadQueue(string queue_id)
        {
            string status = "";
            string download_url = "";

            while (status != "done")
            {
                var request = _http.GetAsync($"https://worker.ripper.store/api/v1/status?ident={queue_id}").Result;

                if ((int)request.StatusCode != 200)
                {
                    Console.WriteLine("| There was an Error (VRCA), please try again later");
                    Console.Read();
                    break;
                };

                var json = JsonConvert.DeserializeObject<List<Queue>>(request.Content.ReadAsStringAsync().Result);
                status = json[0].status;

                if (status == "done")
                {
                    download_url = json[0].download;
                }

                if (status == "failed")
                {
                    Console.WriteLine("| There was an Error (VRCA), please try again later");
                    Console.Read();
                    break;
                }

                Thread.Sleep(2000);
            }

            Console.WriteLine("> Requesting VRCA download");

            var _vrcaRequest = _http.GetAsync(download_url).Result;

            if ((int)_vrcaRequest.StatusCode != 200)
            {
                Console.WriteLine("| There was an Error (VRCA DL), please try again later");
                Console.Read();
                throw new Exception();
            };

            byte[] _vrca = _vrcaRequest.Content.ReadAsByteArrayAsync().Result;
            string _vrcaPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".vrca");
            File.WriteAllBytes(_vrcaPath, _vrca);

            Console.WriteLine("> VRCA downloaded\n");

            return _vrcaPath;
        }

        private string ImageDownload(string apiKey, string ident)
        {
            Console.WriteLine("> Requesting Image download");
            var _imgRequest = _http.GetAsync($"https://worker.ripper.store/api/v1/image?apiKey={apiKey}&ident={ident}").Result;

            if ((int)_imgRequest.StatusCode != 200)
            {
                Console.WriteLine("| There was an Error (Image DL), please try again later");
                Console.Read();
                throw new Exception();
            };

            byte[] _img = _imgRequest.Content.ReadAsByteArrayAsync().Result;
            string _imgPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".png");
            File.WriteAllBytes(_imgPath, _img);

            Console.WriteLine("> Image downloaded\n");

            return _imgPath;
        }
        private string GenerateFakeMac()
        {
            Random rand = new();
            byte[] data = new byte[5];
            rand.NextBytes(data);
            string hmac = EasyHash.GetSHA1String(data);
            return hmac;
        }

        internal async Task ReUploadAvatarAsync(string Name, string AssetPath, string ImagePath, string avatarId)
        {
            //$"avtr_{Guid.NewGuid()}";

            ApiAvatar avatar = new ApiAvatar();
            var avatarFile = new AvatarObjectStore(apiClient, UnityVersion, AssetPath);
            await avatarFile.Reupload().ConfigureAwait(false);

            var imageFile = new ImageObjectStore(apiClient, ImagePath, UnityVersion);
            await imageFile.Reupload().ConfigureAwait(false);
            var newAvatar = await new CustomApiAvatar(apiClient)
            {
                Id = avatarId,
                Name = Name,
                Description = Name,
                AssetUrl = avatarFile.FileUrl /*_assetFileUrl*/,
                ImageUrl = imageFile.FileUrl /*_imageFileUrl*/,
                UnityPackages = new List<Reuploader.VRChatApi.Models.AvatarUnityPackage>() {
                        new Reuploader.VRChatApi.Models.AvatarUnityPackage() {
                            Platform = "standalonewindows",
                            UnityVersion = UnityVersion
                        }
                    },
                Tags = new List<string>(),
                AuthorId = customApiUser.Id,
                AuthorName = customApiUser.Username,
                Created = new DateTime(0),
                Updated = new DateTime(0),
                ReleaseStatus = "private",
                AssetVersion = new Reuploader.VRChatApi.Models.AssetVersion()
                {
                    UnityVersion = UnityVersion,
                    ApiVersion = avatar.ApiVersion
                },
                Platform = "standalonewindows"
            }.Post().ConfigureAwait(false);
            if (newAvatar == null)
            {
                Console.WriteLine("Avatar upload failed");
                return;
            }
            var tempUnityPackage = newAvatar.UnityPackages.FirstOrDefault(u => u.Platform == "standalonewindows");
            if (tempUnityPackage == null)
            {
                Console.WriteLine("Unable to get unity package from response");
                return;
            }
            //Console.WriteLine($"Avatar upload {newAvatar.Name} successful! (Id: {newAvatar.Id}, Platform: {newAvatar.UnityPackages.First().Platform})");
            //Console.WriteLine("Job Done!");
        }

        public class Queue
        {
            public string status { get; set; }
            public string download { get; set; }
        }
    }
}