using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reuploader.Models;
using Reuploader.VRChatApi;
using Reuploader.VRChatApi.Models;
using RipperStoreReuploader;

namespace Reuploader.Misc
{
    internal class AvatarObjectStore : FileObjectStore
    {
        private string _UnityVersion;
        private bool _quest;

        internal AvatarObjectStore(VRChatApiClient client, string unityversion, string path, bool quest = false, CancellationToken? ct = null) : base(client, path)
        {
            _UnityVersion = unityversion;
            _quest = quest;
        }

        internal override async Task Reupload()
        {
            try
            {

                var friendlyAssetBundleName = GetFriendlyAvatarName(_UnityVersion, _quest ? "android" : "standalonewindows");

                if (!await CustomApiFileHelper.UploadFile(_apiClient, _path, friendlyAssetBundleName, string.Empty, true, OnAvatarUploadSuccess, OnAvatarUploadFailure).ConfigureAwait(false))
                {
                    Program.isWaiting = false;
                    Console.WriteLine($"{RipperStoreReuploader.Misc.Functions.errorPrefix}Failed to upload avatar!");
                }
            }
            catch (Exception e)
            {
                Program.isWaiting = false;
                Console.WriteLine($"{RipperStoreReuploader.Misc.Functions.errorPrefix}{e}");
            }
        }

        private void OnAvatarUploadSuccess(CustomApiFile file)
        {
            FileUrl = file.GetFileUrl();
        }

        private void OnAvatarUploadFailure(string error)
        {
            Program.isWaiting = false;
            Console.WriteLine($"{RipperStoreReuploader.Misc.Functions.errorPrefix}Avatar error: {error}");
        }

        private string GetFriendlyAvatarName(string unityVersion, string platform) =>
            $"Avatar - {ReuploadHelper.FriendlyName} - Asset bundle - {unityVersion}_4_{platform}_Release";
    }
}
