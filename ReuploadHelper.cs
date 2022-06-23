using ReuploaderMod.Misc;
using ReuploaderMod.Models;
using ReuploaderMod.VRChatApi;
using ReuploaderMod.VRChatApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReuploaderToolForFriends
{
    internal class ReuploadHelper
    {
        internal static object FriendlyName = "KeafyIsHere";
        internal static string UnityVersion = "2019.4.31f1";
        internal static string ClientVersion = "2022.1.1p1-1170--Release";
        private string fakemac;
        private VRChatApiClient apiClient;
        private CustomApiUser customApiUser;

        public ReuploadHelper(string login)
        {
            if (!File.Exists("FakeMac.txt"))
            {
                fakemac = GenerateFakeMac();
                File.WriteAllText("FakeMac.txt", fakemac);
            }
            apiClient = new VRChatApiClient(10, fakemac);
            if (File.Exists("auth.txt"))
            {
                string[] auth = File.ReadAllLines("auth.txt");
                customApiUser = apiClient.CustomApiUser.LoginWithExistingSession(auth[0], auth[1]).GetAwaiter().GetResult();
            }
            customApiUser ??= apiClient.CustomApiUser
                .Login(login.Split(':')[0], login.Split(':')[1], CustomApiUser.VerifyTwoFactorAuthCode).Result;
            Random random = new Random();
            string[] _Words = File.ReadAllLines("Words.txt");
            FriendlyName = _Words.ElementAt(random.Next(_Words.Length));
        }
        private static string GenerateFakeMac()
        {
            Random rand = new();
            byte[] data = new byte[5];
            rand.NextBytes(data);
            string fakemac = EasyHash.GetSHA1String(data);
            return fakemac;
        }

        internal async Task ReUploadAvatarAsync(string Name, string AssetPath, string ImagePath)
        {
            var avatarId = $"avtr_{Guid.NewGuid()}";
            using AssetsToolsObjectStore? assetsToolsObject = new AssetsToolsObjectStore(AssetPath, "", avatarId, AssetsToolsObjectStore.AssetsToolsObjectType.Avatar, true, CancellationToken.None);
            assetsToolsObject.LoadAndUnpack();
            await GCHelper.BlockingCollectAsync().ConfigureAwait(false);
            assetsToolsObject.ReplaceAvatarOrWorldId();
            assetsToolsObject.PackAndSave();
            if (assetsToolsObject.Error)
            {
                Console.WriteLine(assetsToolsObject.LastError);
                return;
            }
            var reuploadedAvatarPath = assetsToolsObject.AssetsFilePath;
            Console.WriteLine($"Replaced asset bundle: {reuploadedAvatarPath}");
            ApiAvatar avatar = new ApiAvatar();
            var avatarFile = new AvatarObjectStore(apiClient, UnityVersion, reuploadedAvatarPath);
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
                UnityPackages = new List<ReuploaderMod.VRChatApi.Models.AvatarUnityPackage>() {
                        new ReuploaderMod.VRChatApi.Models.AvatarUnityPackage() {
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
                AssetVersion = new ReuploaderMod.VRChatApi.Models.AssetVersion()
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
            Console.WriteLine($"Avatar upload {newAvatar.Name} successful! (Id: {newAvatar.Id}, Platform: {newAvatar.UnityPackages.First().Platform})");
            Console.WriteLine("Job Done!");
        }
    }
}