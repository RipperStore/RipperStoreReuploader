using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReuploaderMod.Models;
using ReuploaderMod.VRChatApi;
using ReuploaderMod.VRChatApi.Models;
using ReuploaderToolForFriends;

namespace ReuploaderMod.Misc {
    internal class AvatarObjectStore : FileObjectStore {
        private string _UnityVersion;
        private bool _quest;

        internal AvatarObjectStore(VRChatApiClient client, string unityversion, string path, bool quest = false, CancellationToken? ct = null) : base(client, path) {
            _UnityVersion = unityversion;
            _quest = quest;
        }

        internal override async Task Reupload() {
            try {

                var friendlyAssetBundleName = GetFriendlyAvatarName(_UnityVersion , _quest ? "android" : "standalonewindows");

                if (!await CustomApiFileHelper.UploadFile(_apiClient, _path, friendlyAssetBundleName, string.Empty, true, OnAvatarUploadSuccess, OnAvatarUploadFailure).ConfigureAwait(false)) {
                    Console.WriteLine("Failed to upload avatar!");
                }
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private void OnAvatarUploadSuccess(CustomApiFile file) {
            FileUrl = file.GetFileUrl();
            Console.WriteLine($"Avatar uri: {FileUrl}");
        }

        private void OnAvatarUploadFailure(string error) {
            Console.WriteLine($"Avatar error: {error}");
        }

        private string GetFriendlyAvatarName(string unityVersion, string platform) =>
            $"Аvаtаr - {ReuploadHelper.FriendlyName} - Asset bundle - {unityVersion}_4_{platform}_Release";
    }
}
