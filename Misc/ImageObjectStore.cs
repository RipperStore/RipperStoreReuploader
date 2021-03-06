using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reuploader.VRChatApi;
using Reuploader.VRChatApi.Models;
using RipperStoreReuploader;
using ApiAvatar = Reuploader.Models.ApiAvatar;

namespace Reuploader.Misc
{
    internal class ImageObjectStore : FileObjectStore
    {
        private bool _isQuest;
        private string _UnityVersion;
        private bool _deleteFiles;
        private string _existingId;

        public ImageObjectStore(VRChatApiClient client, string path, string UnityVersion, bool quest = false, bool deleteFiles = true, string existingId = "") : base(client, path)
        {
            _isQuest = quest;
            _UnityVersion = UnityVersion;
            _deleteFiles = deleteFiles;
            _existingId = existingId;
        }

        internal override async Task Reupload()
        {
            try
            {
                string friendlyImageName = friendlyImageName = GetFriendlyImageName(_UnityVersion, _isQuest ? "android" : "standalonewindows");

                if (!await CustomApiFileHelper.UploadFile(_apiClient, _path, friendlyImageName, _existingId, _deleteFiles, OnImageUploadSuccess, OnImageUploadFailure).ConfigureAwait(false))
                {
                    Program.isWaiting = false;
                    Console.WriteLine("Failed to upload image!");
                }
            }
            catch (Exception e)
            {
                Program.isWaiting = false;
                Console.WriteLine(e);
            }
        }

        private void OnImageUploadSuccess(CustomApiFile file)
        {
            FileUrl = file.GetFileUrl();
        }

        private void OnImageUploadFailure(string error)
        {
            Program.isWaiting = false;
            Console.WriteLine($"Image error: {error}");
        }
        private string GetFriendlyImageName(string unityVersion, string platform)
        {

            return $"Avatar - {ReuploadHelper.FriendlyName} - Image - {unityVersion}_4_{platform}_Release";
        }
    }
}
