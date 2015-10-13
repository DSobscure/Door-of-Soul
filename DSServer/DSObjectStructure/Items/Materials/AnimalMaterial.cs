using DSObjectProtocol;
using Newtonsoft.Json;

namespace DSObjectStructure.Items.Materials
{
    public abstract class AnimalMaterial : Material
    {
        [JsonIgnore]
        public abstract AnimalType type { get; protected set; }
        protected AnimalMaterial() : base() { }
        protected AnimalMaterial(int itemCount) : base(itemCount)
        {

        }
    }
}
