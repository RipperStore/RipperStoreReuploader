using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReuploaderMod.VRChatApi.Models {
    
    public class CustomAssetVersion {
        public int ApiVersion { get; set; }
        public string UnityVersion { get; set; }

        public CustomAssetVersion() {
            ApiVersion = 1;
            UnityVersion = "2018.4.20f1";
        }

        public CustomAssetVersion(int apiV) {
            ApiVersion = apiV;
            UnityVersion = "2018.4.20f1";
        }

        public CustomAssetVersion(string unityV) {
            ApiVersion = 1;
            UnityVersion = unityV;
        }

        public CustomAssetVersion(int apiV, string unityV) {
            ApiVersion = apiV;
            UnityVersion = unityV;
        }
    }
}