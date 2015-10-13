using DSObjectProtocol;
using Newtonsoft.Json;

namespace DSObjectStructure.Items.Materials
{
    public abstract class MineralMaterial : Material
    {
        [JsonIgnore]
        public abstract MineralType type { get; protected set; }
        protected MineralMaterial() : base() { }
        protected MineralMaterial(int itemCount) : base(itemCount)
        {

        }
    }
}
