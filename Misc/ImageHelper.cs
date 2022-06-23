using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReuploaderMod.Misc {
    internal static class ImageHelper {
        internal static string CreateImage() {
            var path = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), "png"));
            try {
                using var image = new Bitmap(1200, 900);
                using var graphics = Graphics.FromImage(image);
                graphics.Clear(Color.White);
                image.Save(path, ImageFormat.Png);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            return path;
        }

        internal static string ConvertImage(string path) {
            var newPath = Path.ChangeExtension(path, "png");
            try {
                using var image = Image.FromFile(path);
                image.Save(newPath, ImageFormat.Png);
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            try {
                File.Delete(path);
            } catch (Exception) {

            }
            return newPath;
        }
    }
}
