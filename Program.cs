using Newtonsoft.Json;
using Reuploader.Misc;
using Reuploader.VRChatApi;
using Reuploader.VRChatApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using static System.Console;

namespace RipperStoreReuploader
{
    internal class Program
    {
        private static string avatarName;
        private static string queueID;
        private static string prefix = "> ";
        private static string errorPrefix = "!! ";
        private string vrcaPath;
        private string imgPath;
        private static string ident;

        public static Config Config { get; set; }
        public static VRChatApiClient apiClient;
        public static CustomApiUser customApiUser;
        public static HttpClient _http = new HttpClient();
        public static Queue _queue = new Queue();

        static void Main(string[] args)
        {

            DownloadHelper.Setup();
            apiClient = new VRChatApiClient(10, GenerateFakeMac());

            //Load Config
            InitializeConfig(false, false);

            //SetUpVRChat();
            //SetUpRipperStore();

            //Save Config
            InitializeConfig(false, true);

            SetUpAvatarName();

            string avatarID = $"avtr_{Guid.NewGuid()}";

            SetUpReuploaderQueue(avatarID);
            WaitForReuploaderQueue();
            ImageDownload();

            //new ReuploadHelper();

            Read();
        }

        public static void InitializeConfig(bool resetConfig, bool saveConfig)
        {
            if (resetConfig) File.Delete("Config.json");
            if (saveConfig) File.WriteAllText("Config.json", JsonConvert.SerializeObject(Config)); ;

            if (File.Exists("Config.json")) { Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json")); }
            else { Config = new Config() { username = null, password = null, authCookie = null, userID = null, apiKey = null }; }
        }
        private static void SetUpReuploaderQueue(string avatarid)
        {
            bool success = false;
            do
            {
                Console.WriteLine($"{prefix}Enter the RipperStore ID (ident) of the Avatar you want to Reupload: ");
                string _ident = Console.ReadLine();
                Console.WriteLine("");

                var request = _http.PostAsync($"https://worker.ripper.store/api/v1/hotswap-url?apiKey={Config.apiKey}&ident={_ident}&avatarid={avatarid}&unityVersion={ReuploadHelper.UnityVersion}", null).Result;

                switch ((int)request.StatusCode)
                {
                    case 201:
                        ident = _ident;
                        queueID = request.Content.ReadAsStringAsync().Result.Split('"')[1];
                        success = true;
                        Console.WriteLine($"{prefix}Successfully placed in queue, waiting.. (this may take a few minutes)");
                        break;
                    case 400:
                        Console.WriteLine($"{errorPrefix}Invalid API Key / ID provided");
                        break;
                    case 401:
                        Console.WriteLine($"{errorPrefix}Invalid API Key Provided, please check https://ripper.store/clientarea > Profile-Settings");
                        break;
                    case 402:
                        Console.WriteLine($"{errorPrefix}You do not own the requested avatar, please purchase before hotswapping");
                        break;
                    case 404:
                        Console.WriteLine($"{errorPrefix}Invalid ID (ident) Provided, ID must be last part of URL");
                        break;
                    case 429:
                        Console.WriteLine($"{errorPrefix}You are being Rate Limited, please try again later");
                        break;
                    default:
                        Console.WriteLine($"{errorPrefix}There was an Error (Queue), please try again later");
                        break;
                }


            } while (!success);
        }
        private static void WaitForReuploaderQueue()
        {
            string download_url = "";
            string status = "";
            do
            {
                var request = _http.GetAsync($"https://worker.ripper.store/api/v1/status?ident={queueID}").Result;

                if ((int)request.StatusCode != 200)
                {
                    Console.WriteLine($"{errorPrefix}There was an Error (VRCA), please try again later");
                    throw new Exception();
                };

                _queue = JsonConvert.DeserializeObject<List<Queue>>(request.Content.ReadAsStringAsync().Result)[0];

                Console.WriteLine(_queue.status);

                if (_queue.status == "done")
                {
                    download_url = _queue.download;
                    status = _queue.status;
                }
                else
                {
                    Thread.Sleep(2000);
                }

                if (_queue.status == "failed")
                {
                    Console.WriteLine($"{errorPrefix}There was an Error (VRCA), please try again later");
                    throw new Exception();
                }

            } while (status != "done");


            Console.WriteLine($"{prefix}Requesting VRCA download");

            var _vrcaRequest = _http.GetAsync(download_url).Result;

            if ((int)_vrcaRequest.StatusCode != 200)
            {
                Console.WriteLine($"{errorPrefix}There was an Error (VRCA DL), please try again later");
                throw new Exception();
            };

            byte[] _vrca = _vrcaRequest.Content.ReadAsByteArrayAsync().Result;
            string _vrcaPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".vrca");
            File.WriteAllBytes(_vrcaPath, _vrca);

            Console.WriteLine($"{prefix}VRCA downloaded");

        }
        private static void ImageDownload()
        {
            Console.WriteLine("> Requesting Image download");
            var _imgRequest = _http.GetAsync($"https://worker.ripper.store/api/v1/image?apiKey={Config.apiKey}&ident={ident}").Result;

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

            _imgPath = _imgPath;
        }
        public static void SetUpVRChat()
        {
            bool loggedIn = false;
            do
            {
                if (Config.userID == null || Config.authCookie == null)
                {
                    Console.WriteLine($"{prefix}Please enter your VRChat Username / Email:");
                    Config.username = ReadLine();

                    Console.WriteLine($"{prefix}Please enter your VRChat Password:");
                    Config.password = ReadLine();

                    customApiUser = apiClient.CustomApiUser.Login(Config.username, Config.password, CustomApiUser.VerifyTwoFactorAuthCode).Result;

                    if (customApiUser == null || customApiUser.Id == null)
                    {
                        Console.WriteLine($"{errorPrefix}Error, Unable to login: invalid credentials");
                    }
                    else
                    {
                        Console.WriteLine($"{prefix}Successfully logged in (VRChat)");
                        loggedIn = true;
                    }
                }
                else
                {
                    Console.WriteLine($"{prefix}logging in with existing session (VRChat)");

                    customApiUser = apiClient.CustomApiUser.LoginWithExistingSession(Config.userID, Config.authCookie, null).Result;

                    if (customApiUser == null || customApiUser.Id == null)
                    {
                        Config.userID = null; Config.authCookie = null;
                        Console.WriteLine($"{errorPrefix}Error, Unable to login with existing session (VRChat)");
                    }
                    else
                    {
                        Console.WriteLine($"{prefix}Successfully logged in with existion session (VRChat)");
                        loggedIn = true;
                    }

                }
            } while (!loggedIn);
        }
        public static void SetUpRipperStore()
        {
            bool success = false;
            do
            {
                if (Config.apiKey == null)
                {
                    Console.WriteLine($"{prefix}Please enter your RipperStore APIKey:");
                    Config.apiKey = ReadLine();
                }

                var res = _http.GetAsync($"https://api.ripper.store/api/v1/clientarea/credits/validate?apiKey={Config.apiKey}").Result;
                if ((int)res.StatusCode == 200)
                {
                    Console.WriteLine($"{prefix}Successfully verified apiKey (RipperStore)");
                    success = true;
                }
                else
                {
                    Config.apiKey = null;
                    Console.WriteLine($"{errorPrefix}Error, invalid APIKey provided (RipperStore)");
                }


            } while (!success);
        }
        public static void SetUpAvatarName()
        {
            bool success = false;
            do
            {
                Console.WriteLine($"{prefix} Name for your new Avatar:");
                avatarName = ReadLine();

                if (avatarName.Length < 3 || avatarName.Length > 40)
                {
                    Console.WriteLine($"{errorPrefix}Error, invalid Avatar Name provided");
                }
                else
                {
                    success = true;
                }
            } while (!success);
        }
        public static string GenerateFakeMac()
        {
            Random rand = new Random();
            byte[] data = new byte[5];
            rand.NextBytes(data);
            string hmac = EasyHash.GetSHA1String(data);
            return hmac;
        }

        public class Queue
        {
            public string status { get; set; }
            public string download { get; set; }
        }

    }
}
