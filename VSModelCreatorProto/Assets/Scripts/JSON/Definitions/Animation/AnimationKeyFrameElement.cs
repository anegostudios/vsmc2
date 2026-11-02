using Newtonsoft.Json;
using UnityEngine;


namespace VSMC
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AnimationKeyFrameElement
    {
        [JsonProperty]
        public double? OffsetX = null;
        [JsonProperty]
        public double? OffsetY = null;
        [JsonProperty]
        public double? OffsetZ = null;

        [JsonProperty]
        public double? StretchX = null;
        [JsonProperty]
        public double? StretchY = null;
        [JsonProperty]
        public double? StretchZ = null;
        [JsonProperty]
        public double? RotationX = null;
        [JsonProperty]
        public double? RotationY = null;
        [JsonProperty]
        public double? RotationZ = null;
        [JsonProperty]
        public double? OriginX = null;
        [JsonProperty]
        public double? OriginY = null;
        [JsonProperty]
        public double? OriginZ = null;
        [JsonProperty]
        public bool RotShortestDistanceX = false;
        [JsonProperty]
        public bool RotShortestDistanceY = false;
        [JsonProperty]
        public bool RotShortestDistanceZ = false;


        internal int Frame;
        public ShapeElement ForElement;


        public bool AnySet
        {
            get { return PositionSet || StretchSet || RotationSet || OriginSet; }
        }

        public bool PositionSet
        {
            get { return OffsetX != null || OffsetY != null || OffsetZ != null; }
        }
        public bool StretchSet
        {
            get { return StretchX != null || StretchY != null || StretchZ != null; }
        }
        public bool RotationSet
        {
            get { return RotationX != null || RotationY != null || RotationZ != null; }
        }
        public bool OriginSet
        {
            get { return OriginX != null || OriginY != null || OriginZ != null; }
        }

        internal bool IsSet(int flag)
        {
            switch (flag)
            {
                case 0: return PositionSet;
                case 1: return RotationSet;
                case 2: return StretchSet;
                case 3: return OriginSet;
            }

            return false;
        }

        public void ScaleAll(float size)
        {
            OffsetX *= size;
            OffsetY *= size;
            OffsetZ *= size;
            StretchX *= size;
            StretchY *= size;
            StretchZ *= size;
            OriginX *= size;
            OriginY *= size;
            OriginZ *= size;
        }

        public void RoundValuesAndReduceDecimals()
        {
            double p = ProgramPreferences.UseHighPrecisionMode.GetValue() ? 1000000 : 1000;
            if (OffsetX.HasValue) OffsetX = Mathf.RoundToInt((float)(OffsetX.Value * p)) / p;
            if (OffsetY.HasValue) OffsetY = Mathf.RoundToInt((float)(OffsetY.Value * p)) / p;
            if (OffsetZ.HasValue) OffsetZ = Mathf.RoundToInt((float)(OffsetZ.Value * p)) / p;

            if (StretchX.HasValue) StretchX = Mathf.RoundToInt((float)(StretchX.Value * p)) / p;
            if (StretchY.HasValue) StretchY = Mathf.RoundToInt((float)(StretchY.Value * p)) / p;
            if (StretchZ.HasValue) StretchZ = Mathf.RoundToInt((float)(StretchZ.Value * p)) / p;

            if (RotationX.HasValue) RotationX = Mathf.RoundToInt((float)(RotationX.Value * p)) / p;
            if (RotationY.HasValue) RotationY = Mathf.RoundToInt((float)(RotationY.Value * p)) / p;
            if (RotationZ.HasValue) RotationZ = Mathf.RoundToInt((float)(RotationZ.Value * p)) / p;

            if (OriginX.HasValue) OriginX = Mathf.RoundToInt((float)(OriginX.Value * p)) / p;
            if (OriginY.HasValue) OriginY = Mathf.RoundToInt((float)(OriginY.Value * p)) / p;
            if (OriginZ.HasValue) OriginZ = Mathf.RoundToInt((float)(OriginZ.Value * p)) / p;
        }

    }
}