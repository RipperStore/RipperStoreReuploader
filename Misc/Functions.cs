using Newtonsoft.Json;
using Reuploader.Misc;
using Reuploader.VRChatApi;
using Reuploader.VRChatApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;
namespace RipperStoreReuploader.Misc
{
    internal class Functions
    {
        internal static string avatarName;
        internal static string queueID;
        internal static string prefix = "\n> ";
        internal static string errorPrefix = "!! ";
        internal static string ident;
        internal static string vrcaPath;
        internal static string imgPath;

        internal static Config Config { get; set; }
        internal static HttpClient _http = new HttpClient();
        internal static Queue _queue = new Queue();

        internal static void DisplayMenu()
        {
            string[] options = { "Upload Avatar", "Upload Avatar (with Custom Image)", "Reset Configuration" };
            Menu mainMenu = new Menu(options);
            int selectedAction = mainMenu.Run();

            string avatarID = $"avtr_{Guid.NewGuid()}";

            switch (selectedAction)
            {
                case 0:
                    Clear();

                    SetUpAvatarName();
                    SetUpReuploaderQueue(avatarID);
                    WaitForReuploaderQueue();
                    ImageDownload(false);
                    ReuploadHelper.ReUploadAvatarAsync(avatarName, vrcaPath, imgPath, avatarID).Wait();
                    break;
                case 1:
                    Clear();

                    SetUpAvatarName();
                    SetUpReuploaderQueue(avatarID);
                    WaitForReuploaderQueue();
                    ImageDownload(true);
                    ReuploadHelper.ReUploadAvatarAsync(avatarName, vrcaPath, imgPath, avatarID).Wait();
                    break;
                case 2:
                    Clear();

                    InitializeConfig(true, false);
                    SetUpVRChat();
                    SetUpRipperStore();
                    InitializeConfig(false, true);
                    DisplayMenu();
                    break;
                default:
                    break;
            }
        }
        internal static void InitializeConfig(bool resetConfig, bool saveConfig)
        {
            if (resetConfig)
            {
                File.Delete("Config.json");
                ReuploadHelper.customApiUser = null;
                ReuploadHelper.apiClient = null;

            };
            if (saveConfig) { File.WriteAllText("Config.json", JsonConvert.SerializeObject(Config)); Console.WriteLine(Config.userID); Console.WriteLine(Config.authCookie); }

            if (File.Exists("Config.json")) { Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json")); }
            else { Config = new Config() { username = null, password = null, authCookie = null, userID = null, apiKey = null }; }

        }
        internal static void Close()
        {
            Console.WriteLine($"\n{prefix}press any key to exit");
            Console.ReadKey();
            Environment.Exit(0);
        }
        internal static void SetUpReuploaderQueue(string avatarid)
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
                        Program.isWaiting = true;
                        new Task(() => { LoadingBar(71); }).Start();
                        break;
                    case 400:
                        Console.WriteLine($"{errorPrefix}Invalid API Key / ID provided");
                        break;
                    case 401:
                        Console.WriteLine($"{errorPrefix}Invalid API Key Provided, please check https://ripper.store/clientarea > Profile-Settings");
                        break;
                    case 402:
                        Console.WriteLine($"{errorPrefix}You do not own the requested avatar, please purchase before hotswapping");
                        Console.WriteLine($"(if you are 100% sure that you purchased the avatar, restart and reset your config)");
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
        internal static void WaitForReuploaderQueue()
        {
            string download_url = "";
            string status = "";
            do
            {

                var request = _http.GetAsync($"https://worker.ripper.store/api/v1/status?ident={queueID}").Result;

                if ((int)request.StatusCode != 200)
                {
                    Program.isWaiting = false;
                    Console.WriteLine($"{errorPrefix}There was an Error (VRCA), please try again later");
                    Close();
                };

                _queue = JsonConvert.DeserializeObject<List<Queue>>(request.Content.ReadAsStringAsync().Result)[0];

                if (_queue.status == "done")
                {
                    Program.isWaiting = false;
                    download_url = _queue.download;
                    status = _queue.status;
                }
                else
                {
                    Thread.Sleep(2000);
                }

                if (_queue.status == "failed")
                {
                    Program.isWaiting = false;
                    Console.WriteLine($"{errorPrefix}There was an Error (VRCA), please try again later");
                    Close();
                }

            } while (status != "done");


            Console.Clear();
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

            vrcaPath = _vrcaPath;
            Console.WriteLine($"{prefix}VRCA downloaded");

        }
        internal static void ImageDownload(bool useCustomImage)
        {
            bool success = false;
            do
            {
                string imageURI = null;
                if (useCustomImage)
                {
                    Console.WriteLine($"{prefix}Please enter the URL of a custom Image (png/jpg only)");
                    imageURI = Console.ReadLine();
                }
                else
                {
                    Console.WriteLine($"{prefix}Requesting Avatar Image");
                    imageURI = $"https://worker.ripper.store/api/v1/image?apiKey={Config.apiKey}&ident={ident}";
                }

                var _imgRequest = _http.GetAsync(imageURI).Result;

                if ((int)_imgRequest.StatusCode != 200)
                {
                    Console.WriteLine($"{errorPrefix}There was an Error (Image DL), please try again later");
                    if (!useCustomImage) Close();
                };

                byte[] _img = _imgRequest.Content.ReadAsByteArrayAsync().Result;
                string _imgPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".png");
                File.WriteAllBytes(_imgPath, _img);

                if (!IsImage(File.OpenRead(_imgPath)))
                {
                    Console.WriteLine($"{errorPrefix}Invalid Image provided, try something else");
                }
                else
                {

                    Console.WriteLine($"{prefix}Image downloaded");

                    imgPath = _imgPath;
                    success = true;
                }

            } while (!success);
        }
        internal static void SetUpVRChat()
        {
            bool loggedIn = false;
            string userID, authCookie, username, password;

            do
            {
                if (Config.userID == null || Config.authCookie == null)
                {
                    Console.WriteLine($"{prefix}Please enter your VRChat Username / Email:");
                    username = ReadLine();

                    Console.WriteLine($"{prefix}Please enter your VRChat Password:");
                    password = ReadLine();

                    ReuploadHelper.apiClient = new VRChatApiClient(10, GenerateFakeMac());
                    ReuploadHelper.customApiUser ??= ReuploadHelper.apiClient.CustomApiUser.Login(username, password, CustomApiUser.VerifyTwoFactorAuthCode).Result;

                    if (ReuploadHelper.customApiUser == null || ReuploadHelper.customApiUser.Id == null)
                    {
                        Console.WriteLine($"{errorPrefix}Error, Unable to login: invalid credentials");
                    }
                    else
                    {
                        Config.username = username; Config.password = password;
                        Console.WriteLine($"{prefix}Successfully logged in (VRChat)");

                        loggedIn = true;
                    }
                }
                else
                {
                    Console.WriteLine($"{prefix}logging in with existing session (VRChat)");

                    ReuploadHelper.apiClient = new VRChatApiClient(10, GenerateFakeMac());
                    ReuploadHelper.customApiUser ??= ReuploadHelper.apiClient.CustomApiUser.LoginWithExistingSession(Config.userID, Config.authCookie, Config.twoFactor).Result;

                    if (ReuploadHelper.customApiUser == null || ReuploadHelper.customApiUser.Id == null)
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
        internal static void SetUpRipperStore()
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
        internal static void SetUpAvatarName()
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
                    ReuploadHelper.FriendlyName = avatarName;
                }
            } while (!success);
        }
        internal static string GenerateFakeMac()
        {
            Random rand = new Random();
            byte[] data = new byte[5];
            rand.NextBytes(data);
            string hmac = EasyHash.GetSHA1String(data);
            return hmac;
        }
        internal static void LoadingBar(int width)
        {
            int curserPos = Console.CursorTop;
            var _i = 0;
            while (Program.isWaiting)
            {
                char[] array = new char[width];
                _i = _i % (width * 2 - 2);


                for (int i = 0; i < width; i++)
                {
                    array[i] = '-';
                }


                char[] _array = array;
                if (_i < width)
                {
                    _array[_i] = '■';
                }
                else
                {

                    _array[width - (_i - width + 2)] = '■';
                }

                Console.Write(_array);
                Console.SetCursorPosition(0, curserPos);
                _i++;
                Thread.Sleep(20);
            }
        }
        internal static bool IsImage(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            List<string> jpg = new List<string> { "FF", "D8" };
            List<string> png = new List<string> { "89", "50", "4E", "47", "0D", "0A", "1A", "0A" };
            List<List<string>> imgTypes = new List<List<string>> { jpg, png };

            List<string> bytesIterated = new List<string>();

            for (int i = 0; i < 8; i++)
            {
                string bit = stream.ReadByte().ToString("X2");
                bytesIterated.Add(bit);

                bool isImage = imgTypes.Any(img => !img.Except(bytesIterated).Any());
                if (isImage)
                {
                    return true;
                }
            }

            return false;
        }
        internal class Queue
        {
            public string status { get; set; }
            public string download { get; set; }
        }


    }
}
