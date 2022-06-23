using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReuploaderMod.VRChatApi;

namespace ReuploaderMod.Misc {
    internal abstract class FileObjectStore {
        private protected VRChatApiClient _apiClient;
        private protected string _path;

        #nullable enable
        internal string? FileUrl { get; private protected set; }
        #nullable restore

        internal FileObjectStore(VRChatApiClient client, string path) {
            _apiClient = client;
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            _path = path;
        }

        internal virtual Task Reupload() {
            return Task.CompletedTask;
        }
    }
}
