using DSObjectProtocol;
using Newtonsoft.Json;

namespace DSObjectStructure.Items.Materials
{
    public abstract class LiquidMaterial : Material
    {
        [JsonIgnore]
        public abstract LiquidType type { get; protected set; }
        protected LiquidMaterial() : base() { }
        protected LiquidMaterial(int itemCount) : base(itemCount)
        {

        }
    }
}
