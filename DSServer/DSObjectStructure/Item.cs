using System;
using DSObjectProtocol;
using Newtonsoft.Json;

namespace DSObjectStructure
{
    public abstract class Item : IComparable<Item>, IScalable, ICloneable
    {
        [JsonIgnore]
        public abstract ItemID id { get; protected set; }
        [JsonIgnore]
        public abstract string name { get; protected set; }
        [JsonIgnore]
        public abstract string description { get; protected set; }
        [JsonProperty("ietmCount")]
        public int itemCount { get; protected set; }

        protected Item() { }
        protected Item(int itemCount)
        {
            this.itemCount = itemCount;
        }
        public int CompareTo(Item other)
        {
            return id.CompareTo(other.id);
        }

        public void Increase(int value = 1)
        {
            if (value > 0)
                itemCount += value;
        }

        public void Decrease(int value = 1)
        {
            if (value > 0)
                itemCount -= value;
        }

        public void Reset()
        {
            itemCount = 0;
        }

        public abstract object Clone();
    }
}
