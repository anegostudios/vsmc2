using UnityEngine;
using VSMC;

public class LoadedAP
{

    public int shapeElementParentUID;

    private ShapeElement cachedShapeElement;

    public string code;

    public Vector3 position;
    public Vector3 rotation;
    public APUIEntry uiEntry;

    public LoadedAP(ShapeElement parent)
    {
        cachedShapeElement = parent;
        shapeElementParentUID = parent.elementUID;
        code = "newAP";
        position = new Vector3();
        rotation = new Vector3();
    }

    public LoadedAP(ShapeElement parent, AttachmentPoint ap)
    {
        cachedShapeElement = parent;
        shapeElementParentUID = parent.elementUID;
        code = ap.Code;
        position = new Vector3((float)ap.PosX, (float)ap.PosY, (float)ap.PosZ);
        rotation = new Vector3((float)ap.RotationX, (float)ap.RotationY, (float)ap.RotationZ);
    }

    public ShapeElement GetElement()
    {
        if (cachedShapeElement == null || cachedShapeElement.parentShape == ShapeHolder.CurrentLoadedShape) //Only find the element if the AP was loaded into the editing shape.
        {
            cachedShapeElement = ShapeElementRegistry.main.GetShapeElementByUID(shapeElementParentUID);
        }
        return cachedShapeElement;
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
