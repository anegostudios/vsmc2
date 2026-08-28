using UnityEngine;
using VSMC;

public class LoadedAP
{

    public int shapeElementParentUID;

    public string code;

    public Vector3 position;
    public Vector3 rotation;
    public APUIEntry uiEntry;

    public LoadedAP(ShapeElement parent)
    {
        shapeElementParentUID = parent.elementUID;
        code = "newAP";
        position = new Vector3();
        rotation = new Vector3();
    }

    public LoadedAP(ShapeElement parent, AttachmentPoint ap)
    {
        shapeElementParentUID = parent.elementUID;
        code = ap.Code;
        position = new Vector3((float)ap.PosX, (float)ap.PosY, (float)ap.PosZ);
        rotation = new Vector3((float)ap.RotationX, (float)ap.RotationY, (float)ap.RotationZ);
    }
     
    public ShapeElement GetElement()
    {
        return ShapeElementRegistry.main.GetShapeElementByUID(shapeElementParentUID);
    }

    public AttachmentPoint ConvertToJSONAP()
    {
        return new AttachmentPoint()
        {
            Code = code,
            PosX = position.x,
            PosY = position.y,
            PosZ = position.z,
            RotationX = rotation.x,
            RotationY = rotation.y,
            RotationZ = rotation.z
        };
    }

}
