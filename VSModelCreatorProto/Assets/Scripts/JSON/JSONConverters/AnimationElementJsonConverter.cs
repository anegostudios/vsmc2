using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VSMC
{
    public class AnimationElementJSONConverter : JsonConverter<AnimationKeyFrameElement>
    {
        public override void WriteJson(JsonWriter writer, AnimationKeyFrameElement value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Formatting originalFormatting = writer.Formatting;
            writer.Formatting = Formatting.None;
            string s = JsonConvert.SerializeObject(value, ShapeAccessor.BasicSerializerSettings);

            List<int> spaceInserts = new List<int>();
            bool insideString = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\"') insideString = !insideString;
                if (!insideString)
                {
                    if (s[i] == ',' || s[i] == ':')
                    {
                        spaceInserts.Add(i);
                    }
                }
            }
            for (int i = 0; i < spaceInserts.Count; i++)
            {
                s = s.Insert(spaceInserts[i] + i + 1, " ");   
            }

            writer.WriteRawValue(s);
            writer.Formatting = originalFormatting;
        }

        public override AnimationKeyFrameElement ReadJson(JsonReader reader, Type objectType, AnimationKeyFrameElement existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<AnimationKeyFrameElement>(reader);
        }

    }
}
