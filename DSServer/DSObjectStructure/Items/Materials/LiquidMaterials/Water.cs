using DSObjectProtocol;

namespace DSObjectStructure.Items.Materials
{
    public class Water : LiquidMaterial
    {
        static ItemID _id;
        static string _name;
        static string _description;
        static LiquidType _type;

        static Water()
        {
            _id = ItemID.Water;
            _name = "淡水";
            _description = "淡水敘述";
            _type = LiquidType.Water;
        }
        protected Water() : base() { }
        public Water(int itemCount) : base(itemCount) { }

        public override ItemID id
        {
            get
            {
                return _id;
            }

            protected set
            {
                _id = value;
            }
        }

        public override string name
        {
            get
            {
                return _name;
            }

            protected set
            {
                _name = value;
            }
        }

        public override string description
        {
            get
            {
                return _description;
            }

            protected set
            {
                _description = value;
            }
        }

        public override LiquidType type
        {
            get
            {
                return _type;
            }

            protected set
            {
                _type = value;
            }
        }

        public override object Clone()
        {
            return new Water(itemCount);
        }
    }
}
