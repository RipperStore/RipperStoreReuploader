using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReuploaderMod.VRChatApi.Models {
    
    public class CustomApiFile : CustomApiModel {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("ownerId")] public string OwnerId { get; set; }

        [JsonProperty("mimeType")] public string MimeType { get; set; }

        [JsonProperty("extension")] public string Extension { get; set; }

        [JsonProperty("tags")] public List<string> Tags { get; set; }

        [JsonProperty("versions")] public List<Version> Versions { get; set; }

        public CustomApiFile() : base(null, "file") { }

        public CustomApiFile(VRChatApiClient apiClient) : base(apiClient, "file") { }

        public int GetLatestVersionNumber() {
            if (Versions != null)
                return Math.Max(Versions.Count - 1, 0);
            return -1;
        }

        public async Task<CustomApiFile> Get(string id) {
            var ret = await ApiClient.HttpFactory.GetAsync<CustomApiFile>(MakeRequestEndpoint() + $"/{id}" + ApiClient.GetApiKeyAsQuery()).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public Version GetVersion(int num) {
            if (Versions == null || Versions.Count == 0 || num >= Versions.Count)
                return null;
            return Versions[num];
        }

        public Version.FileDescriptor GetFileDescriptor(int num, Version.FileDescriptor.Type ft) {
            return GetVersion(num)?.GetFileDescriptor(ft);
        }

        public string GetFileApiUrl(int version, Version.FileDescriptor.Type type) {
            var uri = (Uri)ApiClient.ObjectStore["ApiUri"];
            return $"{uri}{MakeRequestEndpoint()}/{version}/{type}";
        }

        public string GetFileUrl(int versionNumber) {
            return GetFileApiUrl(versionNumber, Version.FileDescriptor.Type.file);
        }

        public string GetFileUrl() {
            return GetFileUrl(GetLatestVersionNumber());
        }

        public string GetFileApiUrlRaw(int version, Version.FileDescriptor.Type type) {
            switch (type) {
                case Version.FileDescriptor.Type.file:
                    return GetVersion(version)?.File?.Url;
                case Version.FileDescriptor.Type.delta:
                    return GetVersion(version)?.Delta?.Url;
                case Version.FileDescriptor.Type.signature:
                    return GetVersion(version)?.Signature?.Url;
            }

            return string.Empty;
        }

        public string GetFileUrlRaw(int version) {
            return GetFileApiUrlRaw(version, Version.FileDescriptor.Type.file);
        }

        public string GetFileUrlRaw() {
            return GetFileUrlRaw(GetLatestVersionNumber());
        }

        public async Task<CustomApiFile> Create(string name, string mimeType, string extension) {
            var apiFile = new CustomApiFile() {
                Name = name,
                MimeType = mimeType,
                Extension = extension,
                Versions = new List<Version>()
            };
            //var ret = await ApiClient.Post<CustomApiFile>(MakeRequestEndpoint(), ApiClient.GetApiKeyAsQuery(), new StringContent(JsonConvert.SerializeObject(apiFile), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            var ret = await ApiClient.HttpFactory.PostAsync<CustomApiFile>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), new StringContent(JsonConvert.SerializeObject(apiFile), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiFile> Update() {
            var ret = await ApiClient.HttpFactory.PutAsync<CustomApiFile>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), new StringContent(JsonConvert.SerializeObject(this), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiFile> CreateNewVersion(FileType ft, FileMetadata fm) {
            return await CreateNewVersion(ft, fm.FileMD5, fm.FileSizeInBytes, fm.SignatureMD5, fm.SignatureSizeInBytes).ConfigureAwait(false);
        }

        public async Task<CustomApiFile> CreateNewVersion(FileType ft, string fileOrDeltaMd5Base64,
                                                          long fileOrDeltaSizeInBytes, string signatureMd5Base64,
                                                          long signatureSizeInBytes) {
            var meta = new FileMetadata() {
                SignatureMD5 = signatureMd5Base64,
                SignatureSizeInBytes = signatureSizeInBytes,
                FileMD5 = fileOrDeltaMd5Base64,
                FileSizeInBytes = fileOrDeltaSizeInBytes
            };
            //var ret = await ApiClient.Post<CustomApiFile>(MakeRequestEndpoint(), ApiClient.GetApiKeyAsQuery(), new StringContent(JsonConvert.SerializeObject(meta), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            var ret = await ApiClient.HttpFactory.PostAsync<CustomApiFile>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), new StringContent(JsonConvert.SerializeObject(meta), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<Version.StrippedFileDescriptor> StartSimpleUpload(Version.FileDescriptor.Type type) {
            var uplStatus = new UploadStatus(ApiClient, Id, GetLatestVersionNumber(), type, "start");
            uplStatus.ApiClient = ApiClient;
            //return await ApiClient.Put<Version.StrippedFileDescriptor>(uplStatus.MakeRequestEndpoint(), ApiClient.GetApiKeyAsQuery(), new StringContent("{}", Encoding.UTF8, "application/json")).ConfigureAwait(false);
            return await ApiClient.HttpFactory.PutAsync<Version.StrippedFileDescriptor>(uplStatus.MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), new StringContent("{}", Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task<Version.StrippedFileDescriptor>
            StartMultipartUpload(Version.FileDescriptor.Type type, int partNumber) {
            var uplStatus = new UploadStatus(ApiClient, Id, GetLatestVersionNumber(), type, "start");
            uplStatus.ApiClient = ApiClient;
            //return await ApiClient.Put<Version.StrippedFileDescriptor>(uplStatus.MakeRequestEndpoint(), GetPartNumberAsQuery(partNumber) + ApiClient.GetApiKeyAsAdditionalQuery(), new StringContent("{}", Encoding.UTF8, "application/json")).ConfigureAwait(false);
            return await ApiClient.HttpFactory.PutAsync<Version.StrippedFileDescriptor>(uplStatus.MakeRequestEndpoint() + GetPartNumberAsQuery(partNumber) + ApiClient.GetApiKeyAsAdditionalQuery(), new StringContent("{}", Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        public async Task<string>
            UploadFilePart(HttpClient awsHttpClient, string url, byte[] data, List<string> etags, IProgress<double> progress = null) {
            //return await ApiClient.Put(awsHttpClient, url, new ByteArrayContent(data), etags);
            //return await ApiClient.Put(awsHttpClient, url, new ProgressableByteArrayContent(data, new Progress<double>(ProgressReport)), etags).ConfigureAwait(false);
            //return await ApiClient.Put(awsHttpClient, url, new ProgressableByteArrayContent(data, new Progress<double>()), etags).ConfigureAwait(false);
            var tuple = await ApiClient.HttpFactory.PutStringAndETagAsync(url, new ByteArrayContent(data), awsHttpClient).ConfigureAwait(false);
            etags.Add(tuple.Item2.Tag);
            return tuple.Item1;
        }

        public async Task<string> UploadFile(HttpClient awsHttpClient, string url, string contentType,
                                             string md5AsBase64, byte[] data) {
            var uploadProgressPercents = new HashSet<int>(10);
            void OnUploadProgress(double value) {
                int val = RoundOff(value);
                if (uploadProgressPercents.Add(val))
                    Console.WriteLine($"(SP) Upload progress: {val}%");
            }

            //var content = new ByteArrayContent(data);
            var content = new ProgressableByteArrayContent(data, new Progress<double>(OnUploadProgress));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            content.Headers.Add("Content-MD5", md5AsBase64);
            //return await ApiClient.Put(awsHttpClient, url, content).ConfigureAwait(false);
            return await ApiClient.HttpFactory.PutStringAsync(url, content, awsHttpClient).ConfigureAwait(false);
        }

        public async Task<string> UploadFile(HttpClient awsHttpClient, string url, string contentType,
                                             byte[] md5AsBase64, byte[] data) {
            var uploadProgressPercents = new HashSet<int>(10);
            void OnUploadProgress(double value) {
                int val = RoundOff(value);
                if (uploadProgressPercents.Add(val))
                    Console.WriteLine($"(SP) Upload progress: {val}%");
            }

            //var content = new ByteArrayContent(data);
            var content = new ProgressableByteArrayContent(data, new Progress<double>(OnUploadProgress));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            content.Headers.ContentMD5 = md5AsBase64;
            //return await ApiClient.Put(awsHttpClient, url, content).ConfigureAwait(false);
            return await ApiClient.HttpFactory.PutStringAsync(url, content, awsHttpClient).ConfigureAwait(false);
        }

        public async Task<CustomApiFile> FinishUpload(Version.FileDescriptor.Type type, List<string> etags = null) {
            var uplStatus = new UploadStatus(ApiClient, Id, GetLatestVersionNumber(), type, "finish") {
                ETags = etags
            };
            uplStatus.ApiClient = ApiClient;
            //var ret = await ApiClient.Put<CustomApiFile>(uplStatus.MakeRequestEndpoint(), ApiClient.GetApiKeyAsQuery(), new StringContent(JsonConvert.SerializeObject(uplStatus), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            var ret = await ApiClient.HttpFactory.PutAsync<CustomApiFile>(uplStatus.MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), new StringContent(JsonConvert.SerializeObject(uplStatus), Encoding.UTF8, "application/json")).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        private static string GetPartNumberAsQuery(int partNumber) {
            return $"?partNumber={partNumber}";
        }

        private static string GetPartNumberAsAdditionalQuery(int partNumber) {
            return $"&partNumber={partNumber}";
        }

        internal static int RoundOff(double i) => (int)Math.Round(i / 10.0) * 10;

        
        public class Version {
            [JsonProperty("version")] public int VersionNumber { get; set; }

            [JsonProperty("status")] public Status Status { get; set; }

            [JsonProperty("created_at")] public DateTime Created { get; set; }

            [JsonProperty("file")] public FileDescriptor File { get; set; }

            [JsonProperty("delta")] public FileDescriptor Delta { get; set; }

            [JsonProperty("signature")] public FileDescriptor Signature { get; set; }

            [JsonProperty("deleted")] public bool Deleted { get; set; }

            public FileDescriptor GetFileDescriptor(FileDescriptor.Type ft) {
                switch (ft) {
                    case FileDescriptor.Type.file:
                        return File;
                    case FileDescriptor.Type.delta:
                        return Delta;
                    case FileDescriptor.Type.signature:
                        return Signature;
                    default:
                        return null;
                }
            }

            
            public class FileDescriptor {
                [JsonProperty("fileName")] public string FileName { get; set; }

                [JsonProperty("url")] public string Url { get; set; }

                [JsonProperty("md5")] public string MD5 { get; set; }

                [JsonProperty("sizeInBytes")] public int SizeInBytes { get; set; }

                [JsonProperty("status")] public Status Status { get; set; }

                [JsonProperty("category")] public Category Category { get; set; }

                [JsonProperty("uploadId")] public string UploadId { get; set; }

                
                public enum Type {
                    file,
                    delta,
                    signature
                }
            }

            
            public class StrippedFileDescriptor {
                [JsonProperty("url")] public string Url { get; set; }
            }
        }

        
        public class UploadStatus : CustomApiModel {
            [JsonProperty("etags")] public List<string> ETags { get; set; }

            [JsonProperty("fileName")] public string FileName { get; set; }

            [JsonProperty("maxParts")] public double MaxParts { get; set; }

            [JsonProperty("nextPartNumber")] public double NextPartNumber { get; set; }

            [JsonProperty("parts")] public List<object> Parts { get; set; }

            [JsonProperty("uploadId")] public string UploadId { get; set; }

            public UploadStatus(VRChatApiClient apiClient, string id, int version,
                                Version.FileDescriptor.Type descriptor, string action) : base(apiClient,
                $"file/{id}/{version}/{descriptor}/{action}") { }
        }

        
        public class FileMetadata {
            [JsonProperty("signatureMd5")] public string SignatureMD5 { get; set; }

            [JsonProperty("signatureSizeInBytes")] public long SignatureSizeInBytes { get; set; }

            [JsonProperty("fileMd5")] public string FileMD5 { get; set; }

            [JsonProperty("fileSizeInBytes")] public long FileSizeInBytes { get; set; }
        }

        
        public enum Status {
            None,
            Waiting,
            Queued,
            Complete,
            Error
        }

        
        public enum Category {
            Simple,
            Multipart,
            Queued
        }

        
        public enum FileType {
            Full,
            Delta
        }
    }

    
    public enum FileType {
        Image,
        VRCA,
        VRCW
    }
}