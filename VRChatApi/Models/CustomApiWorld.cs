using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReuploaderMod.VRChatApi.Models {
    
    public class CustomApiWorld : CustomApiModel {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("featured")] public bool Featured { get; set; }

        [JsonProperty("authorId")] public string AuthorId { get; set; }

        [JsonProperty("authorName")] public string AuthorName { get; set; }

        [JsonProperty("capacity")] public int Capacity { get; set; }

        [JsonProperty("tags")] public List<string> Tags { get; set; }

        [JsonProperty("releaseStatus")] public string ReleaseStatus { get; set; }

        [JsonProperty("imageUrl")] public string ImageUrl { get; set; }

        [JsonProperty("thumbnailImageUrl")] public string ThumbnailImageUrl { get; set; }

        [JsonProperty("assetUrl")] public string AssetUrl { get; set; }

        [JsonProperty("assetUrlObject")] public WorldAssetUrlObject WorldAssetUrlObject { get; set; }

        [JsonProperty("pluginUrl")] public string PluginUrl { get; set; }

        [JsonProperty("pluginUrlObject")] public WorldPluginUrlObject WorldPluginUrlObject { get; set; }

        [JsonProperty("unityPackageUrl")] public string UnityPackageUrl { get; set; }

        [JsonProperty("unityPackageUrlObject")]
        public WorldUnityPackageUrlObject WorldUnityPackageUrlObject { get; set; }

        [JsonProperty("namespace")] public string Namespace { get; set; }

        [JsonProperty("unityPackages")] public List<WorldUnityPackage> UnityPackages { get; set; }

        [AdminOrApiWriteableOnly][JsonProperty("version")] public int Version { get; set; }

        [JsonProperty("organization")] public string Organization { get; set; }

        [JsonProperty("previewYoutubeId")] public object PreviewYoutubeId { get; set; }

        [JsonProperty("favorites")] public int Favorites { get; set; }

        [JsonProperty("created_at")] public DateTime Created { get; set; }

        [JsonProperty("updated_at")] public DateTime Updated { get; set; }

        //[JsonProperty("publicationDate")] public DateTime PublicationDate { get; set; }

        [JsonProperty("labsPublicationDate")] public string LabsPublicationDate { get; set; }

        [JsonProperty("visits")] public int Visits { get; set; }

        [JsonProperty("popularity")] public int Popularity { get; set; }

        [JsonProperty("heat")] public int Heat { get; set; }

        [JsonProperty("publicOccupants")] public int PublicOccupants { get; set; }

        [JsonProperty("privateOccupants")] public int PrivateOccupants { get; set; }

        [JsonProperty("occupants")] public int Occupants { get; set; }

        [JsonProperty("instances")] public List<List<object>> Instances { get; set; }

        public CustomApiWorld(VRChatApiClient apiClient) : base(apiClient, "worlds") { }

        public async Task<CustomApiWorld> Get(string id) {
            var ret = await ApiClient.HttpFactory.GetAsync<CustomApiWorld>(MakeRequestEndpoint() + $"/{id}" + ApiClient.GetApiKeyAsQuery()).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiWorld> Save() {
            CustomApiWorld ret = null;
            if (string.IsNullOrEmpty(Id))
                ret = await ApiClient.HttpFactory.PostAsync<CustomApiWorld>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), ToJsonContent(this)).ConfigureAwait(false);
            else
                ret = await ApiClient.HttpFactory.PutAsync<CustomApiWorld>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), ToJsonContent(this)).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiWorld> Post() {
            var ret = await ApiClient.HttpFactory.PostAsync<CustomApiWorld>(MakeRequestEndpoint(false) + ApiClient.GetApiKeyAsQuery(), WorldPostJsonContent(this)).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiWorld> Put() {
            var ret = await ApiClient.HttpFactory.PutAsync<CustomApiWorld>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), WorldPutJsonContent(this)).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiWorld> PutNameDescriptionImage() {
            var ret = await ApiClient.HttpFactory.PutAsync<CustomApiWorld>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery(), WorldPutJsonContentNameDescriptionImage(this)).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }

        public async Task<CustomApiWorld> Delete() {
            var ret = await ApiClient.HttpFactory.DeleteAsync<CustomApiWorld>(MakeRequestEndpoint() + ApiClient.GetApiKeyAsQuery()).ConfigureAwait(false);
            ret.ApiClient = ApiClient;
            return ret;
        }
    }

    
    public class WorldAssetUrlObject { }

    
    public class WorldPluginUrlObject { }

    
    public class WorldUnityPackageUrlObject { }

    
    public class ApiWorldInstance {
        public string InstanceId { get; set; }

        public int InstanceOccupants { get; set; }
    }

    
    public class WorldUnityPackage {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("assetUrl")] public string AssetUrl { get; set; }

        [JsonProperty("pluginUrl")] public string PluginUrl { get; set; }

        [JsonProperty("unityVersion")] public string UnityVersion { get; set; }

        [JsonProperty("unitySortNumber")] public object UnitySortNumber { get; set; }

        [JsonProperty("assetVersion")] public int AssetVersion { get; set; }

        [JsonProperty("platform")] public string Platform { get; set; }

        [JsonProperty("created_at")] public DateTime Created { get; set; }

        [JsonProperty("assetUrlObject")] public WorldAssetUrlObject WorldAssetUrlObject { get; set; }

        [JsonProperty("pluginUrlObject")] public WorldPluginUrlObject WorldPluginUrlObject { get; set; }
    }
}