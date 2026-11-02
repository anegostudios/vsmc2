using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VSMC
{
    public class Vector3SerJsonConverter : JsonConverter<Vector3Ser>
    {
        public override void WriteJson(JsonWriter writer, Vector3Ser value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Formatting originalFormatting = writer.Formatting;
            writer.Formatting = Formatting.None;
            string s = JsonConvert.SerializeObject(value, ShapeAccessor.BasicSerializerSettings);
            s = s.Replace(",", ", ").Replace(":", ": ");
            writer.WriteRawValue(s);
            writer.Formatting = originalFormatting;
        }

        public override Vector3Ser ReadJson(JsonReader reader, Type objectType, Vector3Ser existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<Vector3Ser>(reader);
        }

    }
}
