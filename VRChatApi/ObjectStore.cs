using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReuploaderMod.VRChatApi {
    
    public class ObjectStore {
        private readonly Dictionary<string, object> _store;

        public ObjectStore() {
            _store = new Dictionary<string, object>();
        }

        public ObjectStore(int size) {
            _store = new Dictionary<string, object>(size);
        }

        public object this[string key] {
            get => _store[key];
            set => _store[key] = value;
        }

        public T ElementAt<T>(string key) {
            return (T) _store[key];
        }

        public int Count => _store.Count;

        public void Add(string key, object value) {
            _store.Add(key, value);
        }

        public bool Remove(string key) {
            return _store.Remove(key);
        }

        public bool ContainsKey(string key) {
            return _store.ContainsKey(key);
        }

        public bool ContainsValue(object value) {
            return _store.ContainsValue(value);
        }
    }
}