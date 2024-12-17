using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleEngine
{
    [DataContract]
    public class BijectiveDictionary<TKey, TValue>
    {
        public ICollection<TKey> Keys { get => forwardDictionary.Keys; }
        public ICollection<TValue> Values { get => forwardDictionary.Values; }
        public int Count { get => forwardDictionary.Count; }
        [DataMember] private Dictionary<TKey, TValue> forwardDictionary = new Dictionary<TKey, TValue>();
        [DataMember] private Dictionary<TValue, TKey> reverseDictionary = new Dictionary<TValue, TKey>();

        public void Add(TKey key, TValue value)
        {
            forwardDictionary.Add(key, value);
            reverseDictionary.Add(value, key);
        }

        public TValue GetValue(TKey key) => forwardDictionary[key];
        public TKey GetKey(TValue value) => reverseDictionary[value];
        public bool ContainsKey(TKey key) => forwardDictionary.ContainsKey(key);
        public bool ContainsValue(TValue value) => reverseDictionary.ContainsKey(value);
        public void RemoveByKey(TKey key)
        {
            reverseDictionary.Remove(GetValue(key));
            forwardDictionary.Remove(key);
        }
        public void RemoveByValue(TValue value)
        {
            forwardDictionary.Remove(GetKey(value));
            reverseDictionary.Remove(value);
        }
        public void Clear()
        {
            forwardDictionary.Clear(); 
            reverseDictionary.Clear();
        }
    }
}
