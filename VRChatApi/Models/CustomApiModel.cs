using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ReuploaderMod.VRChatApi.Models {
    //
    public abstract class CustomApiModel {
        [JsonIgnore] public static AdminOrApiWritableOnlyExcluderContractResolver Aoawoecr = new AdminOrApiWritableOnlyExcluderContractResolver();
        [JsonIgnore] public static CustomContractResolver CustomContractResolver = new CustomContractResolver();
        [JsonIgnore] public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings() {ContractResolver = Aoawoecr, NullValueHandling = NullValueHandling.Ignore};

        [JsonIgnore] public VRChatApiClient ApiClient;

        [JsonIgnore] public string Endpoint { get; set; }

        [JsonProperty("id")] public string Id { get; set; }

        public CustomApiModel(VRChatApiClient apiClient) {
            Endpoint = null;
            ApiClient = apiClient;
        }

        public CustomApiModel(string endpoint) {
            Endpoint = endpoint;
            ApiClient = null;
        }

        public CustomApiModel(VRChatApiClient apiClient, string endpoint) {
            Endpoint = endpoint;
            ApiClient = apiClient;
        }

        public CustomApiModel(string endpoint, Dictionary<string, object> fields) : this(endpoint) {
            FillAllProperties(fields);
        }

        public CustomApiModel(VRChatApiClient apiClient, string endpoint, Dictionary<string, object> fields) :
            this(apiClient, endpoint) {
            FillAllProperties(fields);
        }

        private void FillAllProperties(Dictionary<string, object> fields) {
            foreach (var kvp in fields)
                if (!FillProperty(kvp))
                    throw new MissingMemberException(kvp.Key);
        }

        private bool FillProperty(KeyValuePair<string, object> kvp) {
            var property = typeof(CustomApiModel).GetProperties().First(p => p.Name.Equals(kvp.Key));
            var propertySetter = property.GetSetMethod();
            if (propertySetter == null)
                return false;
            propertySetter.Invoke(this, new[] {kvp.Value});
            return true;
        }

        public string MakeRequestEndpoint(bool includeId = true) {
            return Endpoint + (!string.IsNullOrEmpty(Id) && includeId ? $"/{Id}" : string.Empty);
        }

        public static JsonContent AvatarPostJsonContent(CustomApiAvatar caa) {
            var avatarDict = new Dictionary<string, object>();
            avatarDict["id"] = caa.Id;
            avatarDict["name"] = caa.Name;
            avatarDict["assetUrl"] = caa.AssetUrl;
            avatarDict["imageUrl"] = caa.ImageUrl;
            avatarDict["description"] = caa.Description;
            if (caa.UnityPackages is {Count: > 0}) {
                var unityPackage = caa.UnityPackages.FirstOrDefault(u => u.Platform == "standalonewindows");
                unityPackage ??= caa.UnityPackages.FirstOrDefault();
                avatarDict["platform"] = unityPackage == null ? "standalonewindows" : unityPackage.Platform;
                avatarDict["unityVersion"] = unityPackage == null ? "2018.4.20f1" : unityPackage.UnityVersion;
            } else if (caa.AssetVersion != null) {
                avatarDict["platform"] = string.IsNullOrEmpty(caa.Platform) ? "standalonewindows" : caa.Platform;
                avatarDict["unityVersion"] = string.IsNullOrEmpty(caa.AssetVersion.UnityVersion) ? "2018.4.20f1" : caa.AssetVersion.UnityVersion;
            }
            else {
                throw new NullReferenceException("Found no complete unity package or asset version");
                return null;
            }
            avatarDict["created_at"] = caa.Created;
            avatarDict["updated_at"] = caa.Updated;
            avatarDict["assetVersion"] = "1";
            avatarDict["releaseStatus"] = caa.ReleaseStatus;
            avatarDict["tags"] = caa.Tags;
            avatarDict["authorName"] = caa.AuthorName;
            avatarDict["authorId"] = caa.AuthorId;
            return new JsonContent(JsonConvert.SerializeObject(avatarDict, SerializerSettings));
        }


        public static JsonContent AvatarPutJsonContent(CustomApiAvatar caa) {
            var avatarDict = new Dictionary<string, object>();
            avatarDict["id"] = caa.Id;
            avatarDict["name"] = caa.Name;
            avatarDict["assetUrl"] = caa.AssetUrl;
            avatarDict["imageUrl"] = caa.ImageUrl;
            avatarDict["description"] = caa.Description;
            if (caa.UnityPackages is { Count: > 0 }) {
                var unityPackage = caa.UnityPackages.FirstOrDefault(u => u.Platform == "android");
                unityPackage ??= caa.UnityPackages.FirstOrDefault();
                avatarDict["platform"] = unityPackage == null ? "android" : unityPackage.Platform;
                avatarDict["unityVersion"] = unityPackage == null ? "2018.4.20f1" : unityPackage.UnityVersion;
            } else if (caa.AssetVersion != null) {
                avatarDict["platform"] = string.IsNullOrEmpty(caa.Platform) ? "android" : caa.Platform;
                avatarDict["unityVersion"] = string.IsNullOrEmpty(caa.AssetVersion.UnityVersion) ? "2018.4.20f1" : caa.AssetVersion.UnityVersion;
            } else {
                throw new NullReferenceException("Found no complete unity package or asset version");
                return null;
            }
            avatarDict["created_at"] = caa.Created;
            avatarDict["updated_at"] = caa.Updated;
            avatarDict["assetVersion"] = "1";
            avatarDict["releaseStatus"] = caa.ReleaseStatus;
            avatarDict["tags"] = caa.Tags;
            avatarDict["authorName"] = caa.AuthorName;
            avatarDict["authorId"] = caa.AuthorId;
            avatarDict["thumbnailImageUrl"] = caa.ThumbnailImageUrl;
            return new JsonContent(JsonConvert.SerializeObject(avatarDict, SerializerSettings));
        }

        public static JsonContent AvatarPutJsonContentNameDescriptionImage(CustomApiAvatar caa) {
            var avatarDict = new Dictionary<string, object>();
            avatarDict["id"] = caa.Id;
            avatarDict["name"] = caa.Name;
            avatarDict["imageUrl"] = caa.ImageUrl;
            avatarDict["description"] = caa.Description;
            return new JsonContent(JsonConvert.SerializeObject(avatarDict, SerializerSettings));
        }

        public static JsonContent WorldPostJsonContent(CustomApiWorld caw) {
            var worldDict = new Dictionary<string, object>();
            worldDict["id"] = caw.Id;
            worldDict["name"] = caw.Name;
            worldDict["assetUrl"] = caw.AssetUrl;
            worldDict["imageUrl"] = caw.ImageUrl;
            worldDict["description"] = caw.Description;
            var unityPackage = caw.UnityPackages.OrderByDescending(u => u.Created).FirstOrDefault(u => u.Platform == "standalonewindows");
            worldDict["platform"] = unityPackage!.Platform;
            worldDict["unityVersion"] = unityPackage!.UnityVersion;
            worldDict["created_at"] = caw.Created;
            worldDict["updated_at"] = caw.Updated;
            worldDict["assetVersion"] = "4";
            worldDict["releaseStatus"] = caw.ReleaseStatus;
            worldDict["tags"] = caw.Tags;
            worldDict["authorName"] = caw.AuthorName;
            worldDict["authorId"] = caw.AuthorId;
            worldDict["capacity"] = caw.Capacity;
            return new JsonContent(JsonConvert.SerializeObject(worldDict, SerializerSettings));
        }

        public static JsonContent WorldPutJsonContent(CustomApiWorld caw) {
            var worldDict = new Dictionary<string, object>();
            worldDict["id"] = caw.Id;
            worldDict["name"] = caw.Name;
            worldDict["assetUrl"] = caw.AssetUrl;
            worldDict["imageUrl"] = caw.ImageUrl;
            worldDict["description"] = caw.Description;
            var unityPackage = caw.UnityPackages.OrderByDescending(u => u.Created).FirstOrDefault(u => u.Platform == "android");
            worldDict["platform"] = unityPackage!.Platform;
            worldDict["unityVersion"] = unityPackage!.UnityVersion;
            worldDict["created_at"] = caw.Created;
            worldDict["updated_at"] = caw.Updated;
            worldDict["assetVersion"] = "4";
            worldDict["releaseStatus"] = caw.ReleaseStatus;
            worldDict["tags"] = caw.Tags;
            worldDict["authorName"] = caw.AuthorName;
            worldDict["authorId"] = caw.AuthorId;
            worldDict["capacity"] = caw.Capacity;
            worldDict["thumbnailImageUrl"] = caw.ThumbnailImageUrl;
            return new JsonContent(JsonConvert.SerializeObject(worldDict, SerializerSettings));
        }

        public static JsonContent WorldPutJsonContentNameDescriptionImage(CustomApiWorld caw) {
            var worldDict = new Dictionary<string, object>();
            worldDict["id"] = caw.Id;
            worldDict["name"] = caw.Name;
            worldDict["imageUrl"] = caw.ImageUrl;
            worldDict["description"] = caw.Description;
            return new JsonContent(JsonConvert.SerializeObject(worldDict, SerializerSettings));
        }

        public static StringContent ToJsonContent<T>(T serialize) where T : CustomApiModel {
            return new StringContent(JsonConvert.SerializeObject(serialize, SerializerSettings), Encoding.UTF8, "application/json");
        }

        public static StringContent ToJsonContent(string json) {
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }

    
    public class AdminOrApiWritableOnlyExcluderContractResolver : DefaultContractResolver {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var property = base.CreateProperty(member, memberSerialization);
            var propertyAttributes = property.AttributeProvider?.GetAttributes(false);
            if (propertyAttributes == null || propertyAttributes.Count == 0)
                return property;

            foreach (var propertyAttribute in propertyAttributes) {
                if (propertyAttribute is AdminOrApiWriteableOnly)
                    property.ShouldSerialize = _ => false;
            }

            return property;
        }
    }

    
    [AttributeUsage(AttributeTargets.Property)]
    public class AdminOrApiWriteableOnly : Attribute {

    }

    
    public class CustomContractResolver : DefaultContractResolver {

    }
}