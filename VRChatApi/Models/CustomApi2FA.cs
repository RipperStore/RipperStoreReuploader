using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReuploaderMod.VRChatApi.Models {
    public class CustomApi2FA : CustomApiModel {
        public CustomApi2FA(VRChatApiClient client) : base(client) { }

        [JsonProperty("requiresTwoFactorAuth")]
        public List<string> Supported2FATypes { get; set; }

        public bool IsOTPSupported() {
            return Supported2FATypes != null && Supported2FATypes.Count > 0 && Supported2FATypes.Contains("otp");
        }

        public bool IsTOTPSupported() {
            return Supported2FATypes != null && Supported2FATypes.Count > 0 && Supported2FATypes.Contains("totp");
        }

        public bool IsSMSSupported() {
            return Supported2FATypes != null && Supported2FATypes.Count > 0 && Supported2FATypes.Contains("sms");
        }

        public string GetFirstSupported2FAType() {
            return Supported2FATypes != null && Supported2FATypes.Count > 0 ? Supported2FATypes.FirstOrDefault() : string.Empty;
        }

        public class CustomApi2FAContainer {
            [JsonProperty("code")]
            public string Code { get; set; }

            public CustomApi2FAContainer() {}

            public CustomApi2FAContainer(string code) {
                Code = code;
            }
        }
    }

    public class CustomApi2FAVerify : CustomApiModel {
        public CustomApi2FAVerify(VRChatApiClient client) : base(client) { }

        [JsonProperty("verified")]
        public bool Verified { get; set; }
    }
}
