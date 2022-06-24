using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using librsync.net;
using Reuploader.Misc;
using Reuploader.VRChatApi.Models;

namespace Reuploader.VRChatApi {
    public static class CustomApiFileHelper {
        private const int MultipartBufferSize = 10 * 1024 * 1024;

        private static byte[] _fileAsBytes;
        private static byte[] _sigFileAsBytes;
        private static CustomApiFile.FileMetadata _fileMetadata;

        private static readonly HttpClientHandler _handler;
        private static readonly HttpClient _awsHttpClient = DownloadHelper.HttpClient;

        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public static CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

        static CustomApiFileHelper() {
            //_handler = new HttpClientHandler() {
            //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            //    UseProxy = false
            //};
            //_awsHttpClient = new HttpClient(_handler, true) {
            //    Timeout = TimeSpan.FromMinutes(90)
            //};
        }

        public static async Task<bool> UploadFile(VRChatApiClient client, string fileName, string friendlyName, string existingId = "", bool cleanUp = false, Action<CustomApiFile> onSuccess = null, Action<string> onFailure = null) {
            try {
                Reset();

                _fileAsBytes = File.ReadAllBytes(fileName);

                var sigFileName = await CreateSignatureFile(fileName).ConfigureAwait(false);
                _fileMetadata = new CustomApiFile.FileMetadata() {
                    FileMD5 = GetFileMD5AsBase64(),
                    FileSizeInBytes = GetLength(_fileAsBytes),
                    SignatureMD5 = GetSignatureMD5AsBase64(),
                    SignatureSizeInBytes = GetLength(_sigFileAsBytes)
                };

                var ext = Path.GetExtension(fileName);

                CustomApiFile apiFile = null;
                if (string.IsNullOrEmpty(existingId))
                    apiFile = await client.CustomApiFile.Create(friendlyName, GetMimeTypeFromExtension(ext), ext).ConfigureAwait(false);
                else
                    apiFile = await client.CustomApiFile.Get(existingId).ConfigureAwait(false);

                var newApiFile = await apiFile.CreateNewVersion(CustomApiFile.FileType.Full, _fileMetadata).ConfigureAwait(false);

                var fdt = CustomApiFile.Version.FileDescriptor.Type.file;

                var fileDesc = newApiFile.GetFileDescriptor(newApiFile.GetLatestVersionNumber(), fdt);

                var successFile = false;
                switch (fileDesc.Category) {
                    case CustomApiFile.Category.Simple:
                        successFile = await SingleFileUpload(client, newApiFile, ext, Convert.FromBase64String(_fileMetadata.FileMD5), _fileAsBytes, fdt).ConfigureAwait(false);
                        break;
                    case CustomApiFile.Category.Multipart:
                        successFile = await MultipartFileUpload(client, newApiFile, _fileAsBytes, fdt).ConfigureAwait(false);
                        break;
                }

                if (!successFile)
                    return false;

                ext = Path.GetExtension(sigFileName);

                fdt = CustomApiFile.Version.FileDescriptor.Type.signature;

                fileDesc = newApiFile.GetFileDescriptor(newApiFile.GetLatestVersionNumber(), fdt);

                var successSig = false;
                switch (fileDesc.Category) {
                    case CustomApiFile.Category.Simple:
                        successSig = await SingleFileUpload(client, newApiFile, ext, Convert.FromBase64String(_fileMetadata.SignatureMD5), _sigFileAsBytes, fdt).ConfigureAwait(false);
                        break;
                    case CustomApiFile.Category.Multipart:
                        successSig = await MultipartFileUpload(client, newApiFile, _sigFileAsBytes, fdt).ConfigureAwait(false);
                        break;
                }

                if (!successSig)
                    return false;

                Reset();

                ForceGC();
                if (cleanUp)
                    Cleanup(fileName, sigFileName);

                onSuccess?.Invoke(newApiFile);

                return true;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                onFailure?.Invoke(ex.ToString());
            }

            return false;
        }

        public static async Task<(bool successful, CustomApiFile file)> UploadFile(VRChatApiClient client, string fileName, string friendlyName, string existingId = "", bool cleanUp = false) {
            try {
                Reset();

                _fileAsBytes = File.ReadAllBytes(fileName);

                var sigFileName = await CreateSignatureFile(fileName).ConfigureAwait(false);
                _fileMetadata = new CustomApiFile.FileMetadata() {
                    FileMD5 = GetFileMD5AsBase64(),
                    FileSizeInBytes = GetLength(_fileAsBytes),
                    SignatureMD5 = GetSignatureMD5AsBase64(),
                    SignatureSizeInBytes = GetLength(_sigFileAsBytes)
                };

                var ext = Path.GetExtension(fileName);

                CustomApiFile apiFile = null;
                if (string.IsNullOrEmpty(existingId))
                    apiFile = await client.CustomApiFile.Create(friendlyName, GetMimeTypeFromExtension(ext), ext).ConfigureAwait(false);
                else
                    apiFile = await client.CustomApiFile.Get(existingId).ConfigureAwait(false);

                var newApiFile = await apiFile.CreateNewVersion(CustomApiFile.FileType.Full, _fileMetadata).ConfigureAwait(false);

                var fdt = CustomApiFile.Version.FileDescriptor.Type.file;

                var fileDesc = newApiFile.GetFileDescriptor(newApiFile.GetLatestVersionNumber(), fdt);

                var successFile = false;
                switch (fileDesc.Category) {
                    case CustomApiFile.Category.Simple:
                        successFile = await SingleFileUpload(client, newApiFile, ext, Convert.FromBase64String(_fileMetadata.FileMD5), _fileAsBytes, fdt).ConfigureAwait(false);
                        break;
                    case CustomApiFile.Category.Multipart:
                        successFile = await MultipartFileUpload(client, newApiFile, _fileAsBytes, fdt).ConfigureAwait(false);
                        break;
                }

                if (!successFile)
                    return default;

                ext = Path.GetExtension(sigFileName);

                fdt = CustomApiFile.Version.FileDescriptor.Type.signature;

                fileDesc = newApiFile.GetFileDescriptor(newApiFile.GetLatestVersionNumber(), fdt);

                var successSig = false;
                switch (fileDesc.Category) {
                    case CustomApiFile.Category.Simple:
                        successSig = await SingleFileUpload(client, newApiFile, ext, Convert.FromBase64String(_fileMetadata.SignatureMD5), _sigFileAsBytes, fdt).ConfigureAwait(false);
                        break;
                    case CustomApiFile.Category.Multipart:
                        successSig = await MultipartFileUpload(client, newApiFile, _sigFileAsBytes, fdt).ConfigureAwait(false);
                        break;
                }

                if (!successSig)
                    return default;

                Reset();

                ForceGC();
                if (cleanUp)
                    Cleanup(fileName, sigFileName);

                return (true, newApiFile);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

            return default;
        }

        public static async Task<(CustomApiFile, CustomApiFile)> UploadAssetBundleAndImage(VRChatApiClient client, string assetBundlePath, string imagePath, string friendlyName, string assetBundleId = "", string imageId = "") {
            try {
                CustomApiFile assetBundleFile = null;
                if (!await UploadFile(client, assetBundlePath, string.Format(friendlyName, "Asset bundle"), assetBundleId, false,
                                      file => {
                                          assetBundleFile = file;
                                      }, Console.WriteLine).ConfigureAwait(false)) {
                    return default;
                }

                CustomApiFile imageFile = null;
                if (!await UploadFile(client, imagePath, string.Format(friendlyName, "Image"), imageId, false,
                                      file => {
                                          imageFile = file;
                                      }, Console.WriteLine).ConfigureAwait(false)) {
                    return default;
                }

                return (assetBundleFile, imageFile);
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }

            return default;
        }

        private static async Task<bool> MultipartFileUpload(VRChatApiClient client, CustomApiFile newApiFile,
                                                            byte[] data,
                                                            CustomApiFile.Version.FileDescriptor.Type type) {
            try {
                using var memoryStream = new MemoryStream(data);

                var etags = new List<string>();
                var partsLength = (int)Math.Ceiling((double) data.Length / (double) MultipartBufferSize) + 1;
                for (var i = 1; i < partsLength; i++) {
                    var buffer = new byte[i == partsLength - 1 ? memoryStream.Length - memoryStream.Position : MultipartBufferSize];
                    var bytesRead = await memoryStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                    var tempEtags = new List<string>();
                    var fileRecord = await newApiFile.StartMultipartUpload(type, i).ConfigureAwait(false);

                    var res = await client.CustomApiFile.UploadFilePart(_awsHttpClient, fileRecord.Url, buffer, tempEtags).ConfigureAwait(false);
                    etags.AddRange(tempEtags);
                    Console.WriteLine($"> (MP) Upload progress: {((double)i / (double)partsLength) * 100}%");
                }

                Console.WriteLine($"> (MP) Upload progress: {((double)partsLength / (double)partsLength) * 100}%");

                newApiFile = await newApiFile.FinishUpload(type, etags).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            return false;
        }


        private static async Task<bool> SingleFileUpload(VRChatApiClient client, CustomApiFile newApiFile, string ext,
                                                         string md5AsBase64, byte[] data,
                                                         CustomApiFile.Version.FileDescriptor.Type type) {
            try {
                var fileRecord = await newApiFile.StartSimpleUpload(type).ConfigureAwait(false);

                var res = await client.CustomApiFile.UploadFile(_awsHttpClient, fileRecord.Url, GetMimeTypeFromExtension(ext), md5AsBase64, data).ConfigureAwait(false);

                newApiFile = await newApiFile.FinishUpload(type).ConfigureAwait(false);

                return true;
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            return false;
        }

        private static async Task<bool> SingleFileUpload(VRChatApiClient client, CustomApiFile newApiFile, string ext,
                                                         byte[] md5AsBase64, byte[] data,
                                                         CustomApiFile.Version.FileDescriptor.Type type) {
            try {
                var fileRecord = await newApiFile.StartSimpleUpload(type).ConfigureAwait(false);

                var res = await client.CustomApiFile.UploadFile(_awsHttpClient, fileRecord.Url, GetMimeTypeFromExtension(ext), md5AsBase64, data).ConfigureAwait(false);

                newApiFile = await newApiFile.FinishUpload(type).ConfigureAwait(false);

                return true;
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }

            return false;
        }

        public static string GetMimeTypeFromExtension(string extension) {
            if (extension == ".vrcw")
                return "application/x-world";
            if (extension == ".vrca")
                return "application/x-avatar";
            if (extension == ".dll")
                return "application/x-msdownload";
            if (extension == ".unitypackage")
                return "application/gzip";
            if (extension == ".gz")
                return "application/gzip";
            if (extension == ".jpg")
                return "image/jpg";
            if (extension == ".png")
                return "image/png";
            if (extension == ".sig")
                return "application/x-rsync-signature";
            if (extension == ".delta")
                return "application/x-rsync-delta";

            Console.WriteLine("Unknown file extension for mime-type: " + extension);
            return "application/gzip";
        }

        private static string GetFileMD5AsBase64() {
            if (_fileAsBytes == null)
                return string.Empty;

            return GetMD5AsBase64FromBytes(_fileAsBytes);
        }

        private static string GetSignatureMD5AsBase64() {
            if (_sigFileAsBytes == null)
                return string.Empty;

            return GetMD5AsBase64FromBytes(_sigFileAsBytes);
        }

        private static string GetMD5AsBase64FromBytes(byte[] input) {
            using var md5 = MD5.Create();
            return Convert.ToBase64String(md5.ComputeHash(input));
        }

        private static int GetLength(byte[] input) {
            return input?.Length ?? -1;
        }

        private static async Task<string> CreateSignatureFile(string fileName) {
            try {
                //string newFileName = Path.ChangeExtension(fileName, ".sig");

                using var memStream = new MemoryStream(_fileAsBytes);
                using var sigStream = Librsync.ComputeSignature(memStream);
                using var sigMemStream = new MemoryStream();
                //using var fileStream = File.Open(newFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

                //await sigStream.CopyToAsync(fileStream);
                await sigStream.CopyToAsync(sigMemStream).ConfigureAwait(false);

                //using var sigMemStream = new MemoryStream();
                //fileStream.Position = 0;
                //await fileStream.CopyToAsync(sigMemStream);
                _sigFileAsBytes = sigMemStream.GetBuffer();

                //return newFileName;
                return Path.ChangeExtension(fileName, ".sig");
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            ForceGC();

            return string.Empty;
        }

        private static void Reset() {
            _fileAsBytes = null;
            _sigFileAsBytes = null;
            _fileMetadata = null;
        }

        private static void ForceGC() {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }

        public static void Cancel() {
            _cancellationTokenSource.Cancel();
            Thread.Sleep(TimeSpan.FromMilliseconds(250));
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private static void Cleanup(string fileName = "", string sigFileName = "") {
            try {
                if (!string.IsNullOrEmpty(fileName))
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }

            try {
                if (!string.IsNullOrEmpty(sigFileName))
                    if (File.Exists(sigFileName)) { }
                        File.Delete(sigFileName);
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}