using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;
using System.Text;

namespace DSSerializable
{
    public static class SerializeFunction
    {
        public static T DeserializeObject<T>(this string toDeserialize)
        {
            BinaryFormatter bft = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(toDeserialize)))
            {
                return (T)bft.Deserialize(ms);
            }
            ;
        }

        public static string SerializeObject<T>(this T toSerialize)
        {
            BinaryFormatter bft = new BinaryFormatter();
            using(MemoryStream ms = new MemoryStream())
            {
                bft.Serialize(ms, toSerialize);
                return Encoding.ASCII.GetString(ms.ToArray());
            }
        }
    }
}
