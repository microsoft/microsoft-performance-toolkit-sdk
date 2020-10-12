using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SampleExtensions.Helpers
{
    public static class JsonBinary
    {
        public static byte[] ToBinary(object value)
        {
            byte[] result = null;

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.Default, true))
                {
                    writer.Write(JsonConvert.SerializeObject(value));
                    result = stream.ToArray();
                }
            }

            return result;
        }

        public static T ToObject<T>(byte[] value)
        {
            string jsonValue = string.Empty;

            using (var stream = new MemoryStream(value))
            {
                using (var reader = new BinaryReader(stream, Encoding.Default, true))
                {
                    jsonValue = reader.ReadString();
                }
            }

            return JsonConvert.DeserializeObject<T>(jsonValue);
        }
    }

}