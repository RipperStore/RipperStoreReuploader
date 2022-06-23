using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReuploaderMod.VRChatApi.Models {
    
    public class CustomApiAvatar : CustomApiModel {
        [JsonProperty(PropertyName = "apiVersion")]
        public int ApiVersion { get; set; }

        [JsonProperty(PropertyName = "assetUrl")]
        public string AssetUrl { get; set; }

        [JsonProperty(PropertyName = "assetVersion")]
        public AssetVersion AssetVersion { get; set; }

        [JsonProperty(PropertyName = "authorId")]
        public string AuthorId { get; set; }

        [JsonProperty(PropertyName = "authorName")]
        public string AuthorName { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime Created { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [AdminOrApiWriteableOnly]
        [JsonProperty(PropertyName = "featured")]
        public bool Featured { get; set; }

        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get; set; }

        [JsonProperty(PropertyName = "releaseStatus")]
        public string ReleaseStatus { get; set; }

        [JsonProperty(PropertyName = "tags")] public List<string> Tags { get; set; }

        [JsonProperty(PropertyName = "thumbnailImageUrl")]
        public string ThumbnailImageUrl { get; set; }

        [JsonProperty(PropertyName = "unityPackageUrl")]
        public string UnityPackageUrl { get; set; }

        [JsonProperty(PropertyName = "updated_at")]
        public DateTime Updated { get; set; }

        [AdminOrApiWriteableOnly]
        [JsonProperty(PropertyName = "version")]
        public int Version { get; set; }

        [AdminOrApiWriteableOnly]
        [JsonProperty("unityPackages")]
        public List<AvatarUnityPackage> UnityPackages { get; set; }

        public CustomApiAvatar(VRChatApiClient apiClient) : base(apiClient, "avatars") {
            
        }

        public async Task<CustomApiAvatar> Get(string id) {
            var ret = await ApiClient.HttpFactory.GetAsync<CustomApiAvatar>(MakeRequestEndpoint() + $"/{id}" + ApiClient.GetApiKeyAsQuery()).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiAvatar> Save() {
            CustomApiAvatar ret = null;
            if (string.IsNullOrEmpty(Id))
                ret = await ApiClient.HttpFactory.PostAsync<CustomApiAvatar>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), AvatarPostJsonContent(this)).ConfigureAwait(false);
            else
                ret = await ApiClient.HttpFactory.PutAsync<CustomApiAvatar>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), AvatarPutJsonContent(this)).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiAvatar> Post() {
            var ret = await ApiClient.HttpFactory.PostAsync<CustomApiAvatar>(MakeRequestEndpoint(false) + ApiClient.GetApiKeyAsQuery(), AvatarPostJsonContent(this)).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiAvatar> Put() {
            var ret = await ApiClient.HttpFactory.PutAsync<CustomApiAvatar>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), AvatarPutJsonContent(this)).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiAvatar> PutNameDescriptionImage() {
            var ret = await ApiClient.HttpFactory.PutAsync<CustomApiAvatar>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), AvatarPutJsonContentNameDescriptionImage(this)).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiAvatar> Delete() {
            var ret = await ApiClient.HttpFactory.DeleteAsync<CustomApiAvatar>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery()).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }
    }

    
    public class AvatarAssetUrlObject { }

    
    public class AvatarUnityPackage {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("assetUrl")] public string AssetUrl { get; set; }

        [JsonProperty("unityVersion")] public string UnityVersion { get; set; }

        [JsonProperty("unitySortNumber")] public long UnitySortNumber { get; set; }

        [JsonProperty("assetVersion")] public long AssetVersion { get; set; }

        [JsonProperty("platform")] public string Platform { get; set; }

        [JsonProperty("created_at")] public DateTime Created { get; set; }
    }

    
    public class AvatarUnityPackageUrlObject { }

    public class AssetVersion {
        public int ApiVersion { get; set; }
        public string UnityVersion { get; set; }

        public AssetVersion() { }
    }
}