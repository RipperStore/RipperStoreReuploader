using Reuploader.Misc;
using Reuploader.VRChatApi;
using System;
using System.Text;
using static System.Console;
using static RipperStoreReuploader.Misc.Functions;
using System.IO;
using System.Security.Cryptography;
using System.Net.Http;

namespace RipperStoreReuploader
{
    internal class Program
    {
        public static bool isWaiting = false;

        static void Main(string[] args)
        {
            OutputEncoding = Encoding.UTF8;
            InputEncoding = Encoding.UTF8;

            DownloadHelper.Setup();

            //Load Config
            InitializeConfig(false, false);

            SetUpVRChat();
            SetUpRipperStore();

            //Save Config
            InitializeConfig(false, true);

            DisplayMenu();

            Read();
        }
    }
}
