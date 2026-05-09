using System;
using Newtonsoft.Json;

namespace VSMC
{
    public class FloatArrayJSONConverter : JsonConverter<float[]>
    {

        public override void WriteJson(JsonWriter writer, float[] value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Formatting originalFormatting = writer.Formatting;
            writer.Formatting = Formatting.None;

            writer.WriteStartArray();
            foreach (float v in value)
            {
                writer.WriteValue(v);
            }
            writer.WriteEndArray();

            writer.Formatting = originalFormatting;
        }

        public override float[] ReadJson(JsonReader reader, Type objectType, float[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<float[]>(reader);
        }

    }
}
