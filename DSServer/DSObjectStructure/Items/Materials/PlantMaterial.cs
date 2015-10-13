using System;
using DSObjectProtocol;
using Newtonsoft.Json;

namespace DSObjectStructure.Items.Materials
{
    public abstract class PlantMaterial : Material
    {
        [JsonIgnore]
        public abstract PlantType type { get; protected set; }
        protected PlantMaterial() : base() { }
        protected PlantMaterial(int itemCount) : base(itemCount)
        {

        }
    }
}
