using DSObjectProtocol;

namespace DSObjectStructure.Items.Materials
{
    public class Sand : MineralMaterial
    {
        static ItemID _id;
        static string _name;
        static string _description;
        static MineralType _type;

        static Sand()
        {
            _id = ItemID.Sand;
            _name = "沙";
            _description = "";
            _type = MineralType.Stone;
        }
        protected Sand() : base() { }
        public Sand(int itemCount) : base(itemCount) { }

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

        public override MineralType type
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
            return new Sand(itemCount);
        }
    }
}
