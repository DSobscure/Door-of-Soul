using System.Collections;
using System.Collections.Generic;
using DSObjectProtocol;
using Newtonsoft.Json;

namespace DSObjectStructure
{
    public class Inventory : IDictionary<ItemID, Item>
    {
        [JsonProperty("items")]
        protected Dictionary<ItemID, Item> items { get; set; }
        [JsonProperty("maxCount")]
        public int maxCount { get; set; }

        public Inventory()
        {
            items = new Dictionary<ItemID, Item>();
            maxCount = 40;
        }
        //IDictionary
        public Item this[ItemID key]
        {
            get
            {
                return items[key];
            }

            set
            {
                items[key] = value;
            }
        }
        public int Count
        {
            get
            {
                return items.Count;
            }
        }
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }
        public ICollection<ItemID> Keys
        {
            get
            {
                return items.Keys;
            }
        }
        public ICollection<Item> Values
        {
            get
            {
                return items.Values;
            }
        }
        public void Add(KeyValuePair<ItemID, Item> item)
        {
            if (items.Count < maxCount)
                items.Add(item.Key, item.Value);
        }
        public void Add(ItemID key, Item value)
        {
            if (items.Count < maxCount)
                items.Add(key, value);
        }
        public void Clear()
        {
            items.Clear();
        }
        public bool Contains(KeyValuePair<ItemID, Item> item)
        {
            return items.ContainsKey(item.Key) && items[item.Key] == item.Value;
        }
        public bool ContainsKey(ItemID key)
        {
            return items.ContainsKey(key);
        }
        public void CopyTo(KeyValuePair<ItemID, Item>[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < array.Length; i++)
            {
                items.Add(array[i].Key, array[i].Value);
            }
        }
        public IEnumerator<KeyValuePair<ItemID, Item>> GetEnumerator()
        {
            return items.GetEnumerator();
        }
        public bool Remove(KeyValuePair<ItemID, Item> item)
        {
            return this.Contains(item) && items.Remove(item.Key);
        }
        public bool Remove(ItemID key)
        {
            return items.Remove(key);
        }
        public bool TryGetValue(ItemID key, out Item value)
        {
            return items.TryGetValue(key, out value);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
