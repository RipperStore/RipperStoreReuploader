using Microsoft.Win32;
using Newtonsoft.Json;
using ReuploaderMod.Misc;
using RipperStoreImageFindRework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReuploaderToolForFriends
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (!File.Exists("Login.txt"))
            {
                Console.WriteLine("Enter user:password login");
                string userpass = Console.ReadLine();
                File.AppendAllText("Login.txt", userpass);
            }
            
            ReuploadHelper rh = new(File.ReadAllText("Login.txt"));
            DownloadHelper.Setup();


            Console.WriteLine("Pick an option");
            Console.WriteLine("1. Manual");
            Console.WriteLine("2. Use Ripper.store API");
            string input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    Console.WriteLine("Enter new avatar name");
                    string name = Console.ReadLine();
                    if (string.IsNullOrEmpty(name)) 
                    {
                        Console.WriteLine("Name can not be null or empty");
                        Thread.Sleep(-1);
                    }
                    Console.WriteLine("Enter Asset url or drag drop vrca");
                    string assetpath = Console.ReadLine();
                    if (string.IsNullOrEmpty(assetpath)) 
                    {
                        Console.WriteLine("asset path can not be null or empty");
                        Thread.Sleep(-1);
                    }
                    if (assetpath.StartsWith("http")) 
                    {
                        Console.WriteLine("Please wait downloading vrca...");
                        assetpath = DownloadHelper.DownloadToRandomPath(assetpath);
                    }
                    Console.WriteLine("Enter image url or drag drop image");
                    string imagepath = Console.ReadLine();
                    if (string.IsNullOrEmpty(imagepath))
                    {
                        Console.WriteLine("image path can not be null or empty");
                        Thread.Sleep(-1);
                    }
                    if (imagepath.StartsWith("http")) 
                    {
                        Console.WriteLine("Please wait downloading image...");
                         imagepath = DownloadHelper.DownloadToRandomPath(imagepath);
                    }
                    Console.WriteLine(assetpath);
                    Console.WriteLine(imagepath);
                    Console.ReadLine();
                    await rh.ReUploadAvatarAsync(name, assetpath, imagepath);
                    break;
                case "2":
                    string apikey = "";
                    if (!File.Exists("Apikey.txt")) 
                    {
                        Console.WriteLine("Enter ripper.store API key:");
                        apikey = Console.ReadLine();
                        File.AppendAllText("Apikey.txt", apikey);
                    }
                    Console.WriteLine("Enter ripper.store ident for avatar you want to upload:");
                    string ident = Console.ReadLine();
                    apikey = File.ReadAllText("Apikey.txt");
                    Console.WriteLine("Enter new avatar name:");
                    string avaname = Console.ReadLine();
                    if (string.IsNullOrEmpty(avaname)) 
                    {
                        Console.WriteLine("Name can not be null or empty");
                    }
                    if (string.IsNullOrEmpty(apikey)) 
                    {
                        Console.WriteLine("apikey can not be null or empty");
                        Thread.Sleep(-1);
                    }
                    HttpClient client = new();
                    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "https://api.ripper.store/avatars/reupload")
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(new { apiKey = apikey, ident = ident }), Encoding.UTF8, "application/json")
                    };
                    var resp = await client.SendAsync(req);
                    resp.EnsureSuccessStatusCode();
                    byte[] filecontent = await resp.Content.ReadAsByteArrayAsync();
                    string vrcapath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".vrca");
                    File.WriteAllBytes(vrcapath, filecontent);
                    string ripperimagepath = DownloadHelper.DownloadToRandomPath(resp.Headers.GetValues("X-Image-Url").First());
                    Console.WriteLine(ripperimagepath);
                    byte[] rawWebP = File.ReadAllBytes(ripperimagepath);
                    Bitmap decodeWebpBmp;
                    using (WebP webp = new WebP())
                    {
                        decodeWebpBmp = webp.Decode(rawWebP);
                    }
                    decodeWebpBmp.Save(ripperimagepath + ".png");
                    ripperimagepath = ripperimagepath + ".png";
                    Console.WriteLine("Reuploading now....");
                    await rh.ReUploadAvatarAsync(avaname, vrcapath, ripperimagepath);
                    break;
                default:
                    Console.WriteLine("Lol retard");
                    Thread.Sleep(4000);
                    Environment.Exit(0);
                    break;
            }
            
            

        }
    }
}
