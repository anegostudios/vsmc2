using System;
using Newtonsoft.Json;

namespace VSMC
{
    public class DoubleArrayJSONConverter : JsonConverter<double[]>
    {

        public override void WriteJson(JsonWriter writer, double[] value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Formatting originalFormatting = writer.Formatting;
            writer.Formatting = Formatting.None;

            writer.WriteStartArray();
            foreach (double v in value)
            {
                writer.WriteValue(v);
            }
            writer.WriteEndArray();

            writer.Formatting = originalFormatting;
        }

        public override double[] ReadJson(JsonReader reader, Type objectType, double[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<double[]>(reader);
        }

    }
}
