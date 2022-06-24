using ReuploaderMod.Misc;

namespace ReuploaderToolForFriends
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
