using Reuploader.Misc;

namespace RipperStoreReuploader
{
    internal class Program
    {
        static void Main(string[] args)
        {

            DownloadHelper.Setup();

            new ReuploadHelper();
        }
    }
}
