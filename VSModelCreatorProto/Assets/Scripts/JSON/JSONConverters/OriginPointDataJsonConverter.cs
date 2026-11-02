using System;
using Newtonsoft.Json;
using UnityEngine;

namespace VSMC
{
    public class OriginPointDataJSONConverter : JsonConverter<OriginPointData>
    {

        public override void WriteJson(JsonWriter writer, OriginPointData value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Formatting originalFormatting = writer.Formatting;
            writer.Formatting = Formatting.None;

            OriginPointData copy = new OriginPointData();
            if (value.itemOrigin.ToVec3() != new Vector3(0.5f, 0.5f, 0.5f)) copy.itemOrigin = value.itemOrigin;
            if (value.itemRotation.ToVec3() != Vector3.zero) copy.itemRotation = value.itemRotation;
            if (value.itemTranslation.ToVec3() != Vector3.zero) copy.itemTranslation = value.itemTranslation;
            if (value.itemScaleXYZ.ToVec3() != Vector3.one) copy.itemScaleXYZ = value.itemScaleXYZ;
            copy.rootElement = value.rootElement;
            copy.apCode = value.apCode;
            string s = JsonConvert.SerializeObject(copy, ShapeAccessor.BasicSerializerSettings); 
            writer.WriteRawValue(s);
            writer.Formatting = originalFormatting;
        }

        public override OriginPointData ReadJson(JsonReader reader, Type objectType, OriginPointData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<OriginPointData>(reader);
        }

    }
}
