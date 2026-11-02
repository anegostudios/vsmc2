using System.ComponentModel;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class OriginPointData
{

    [DefaultValue(null)]
    public Vector3Ser itemTranslation = null;

    [DefaultValue(null)]
    public Vector3Ser itemRotation = null;
    
    [DefaultValue(null)]
    public Vector3Ser itemOrigin = null;

    [DefaultValue(null)]
    public Vector3Ser itemScaleXYZ = null;

    public ElementCodeInShape rootElement = null;
    public string apCode = null;

    public bool useMountOffset = false;

    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
        if (itemTranslation == null) itemTranslation = new Vector3Ser(0, 0, 0);
        if (itemRotation == null) itemRotation = new Vector3Ser(0, 0, 0);
        if (itemOrigin == null) itemOrigin = new Vector3Ser(0.5f, 0.5f, 0.5f);
        if (itemScaleXYZ == null) itemScaleXYZ = new Vector3Ser(1, 1, 1);
    }

    public IOriginPoint ConvertToOriginPoint()
    {
        if (apCode != null && apCode != "")
        {
            AttachmentPointOriginPoint apOrig = new AttachmentPointOriginPoint();
            apOrig.LoadFromOriginData(this);
            return apOrig;
        }
        if (rootElement != null && rootElement.element != "")
        {
            StepparentOriginPoint spOrig = new StepparentOriginPoint();
            spOrig.LoadFromOriginData(this);
            return spOrig;
        }
        TransformOnlyOriginPoint orig = new TransformOnlyOriginPoint();
        orig.LoadFromOriginData(this);
        return orig;
    }

}
