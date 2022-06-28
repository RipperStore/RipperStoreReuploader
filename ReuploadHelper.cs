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


        /*
        public ReuploadHelper()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            apiClient = new VRChatApiClient(10, GenerateFakeMac());

            //if (File.Exists("Config.json")) { Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json")); }
            //else { Config = new Config() { apiKey = null, authCookie = null, password = null, username = null }; }

            //File.WriteAllText("Config.json", JsonConvert.SerializeObject(Config));

            //while (!SetUpName()) { }

            while (!SetUpQueue(Config.apiKey, AvatarID)) { }

            var vrcaPath = ReuploadQueue(queue_id);
            var imgPath = ImageDownload(Config.apiKey, ident);

            Console.WriteLine("> Starting Reupload\n");

            ReUploadAvatarAsync(Name, vrcaPath, imgPath, AvatarID).Wait();

            Console.WriteLine($"> Done Reuploading ({Name})");
            Console.Read();
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
                AssetUrl = avatarFile.FileUrl,
                ImageUrl = imageFile.FileUrl,
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

        */

    }
}