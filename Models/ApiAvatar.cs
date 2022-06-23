using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReuploaderMod.Misc;
using ReuploaderMod.VRChatApi.Models;

namespace ReuploaderMod.Models {
    public class ApiAvatar {
        [JsonProperty(PropertyName = "id")] public string Id { get; set; }

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

        [JsonProperty(PropertyName = "version")]
        public int Version { get; set; }

        [JsonProperty("unityPackages")]
        public List<AvatarUnityPackage> UnityPackages { get; set; }

        public ApiAvatar() { }
    }

    public class AssetVersion {
        public int ApiVersion { get; set; }
        public string UnityVersion { get; set; }

        public AssetVersion() { }
    }

    public class AvatarUnityPackage {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("assetUrl")] public string AssetUrl { get; set; }

        [JsonProperty("unityVersion")] public string UnityVersion { get; set; }

        [JsonProperty("unitySortNumber")] public long UnitySortNumber { get; set; }

        [JsonProperty("assetVersion")] public long AssetVersion { get; set; }

        [JsonProperty("platform")] public string Platform { get; set; }

        [JsonProperty("created_at")] public DateTime Created { get; set; }

        public AvatarUnityPackage() { }
    }
}
