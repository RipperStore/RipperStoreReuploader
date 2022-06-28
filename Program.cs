using Reuploader.Misc;
using Reuploader.VRChatApi;
using System;
using System.Text;
using static System.Console;
using static RipperStoreReuploader.Misc.Functions;

namespace RipperStoreReuploader
{
    internal class Program
    {
        public static string vrcaPath;
        public static string imgPath;
        public static bool isWaiting = false;

        static void Main(string[] args)
        {
            OutputEncoding = Encoding.UTF8;
            InputEncoding = Encoding.UTF8;

            DownloadHelper.Setup(); apiClient = new VRChatApiClient(10, GenerateFakeMac());

            //Load Config
            InitializeConfig(false, false);

            SetUpVRChat();
            SetUpRipperStore();

            //Save Config
            InitializeConfig(false, true);

            DisplayMenu();

            Console.WriteLine(vrcaPath);
            Console.WriteLine(imgPath);

            Read();
        }

    }
}
