using UnityEngine;
using VSMC;

/// <summary>
/// Origin points replace the step parent element system. 
/// Stepparent names are still allowed, but this will allow additional objects to attach to 
///     stepparents, APs, and also have origin transformations.
/// </summary>
public abstract class IOriginPoint
{

    public Vector3 itemTranslation;
    public Vector3 itemRotation;
    public Vector3 itemOrigin = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 itemScaleXYZ = Vector3.one;

    /// <summary>
    /// Copy the transform values from this origin into another.
    /// </summary>
    public void CopyTransformValuesTo(IOriginPoint other)
    {
        other.itemTranslation = itemTranslation;
        other.itemRotation = itemRotation;
        other.itemOrigin = itemOrigin;
        other.itemScaleXYZ = itemScaleXYZ;
    }

    /// <summary>
    /// Gets the matrix to use when this element doesn't need to move.
    /// </summary>
    public abstract Matrix4x4 GetDefaultOriginMatrix();

    /// <summary>
    /// Gets the matrix to use per frame when this element is in animation mode.
    /// </summary>
    public abstract Matrix4x4 GetAnimationOriginMatrix();

    /// <summary>
    /// Load from the data saved into the JSON file.
    /// </summary>
    public abstract void LoadFromOriginData(OriginPointData data);

    /// <summary>
    /// Save into an object that will be saved in JSON.
    /// </summary>
    public abstract OriginPointData SaveToOriginData();

    /// <summary>
    /// Does this origin point depend on another element?
    /// </summary>
    public abstract bool DependsOnOtherShapeElement();

    /// <summary>
    /// Finds the shape element that this origin point uses.
    /// </summary>
    public abstract ShapeElement GetShapeElementThisDependsOn();

    /// <summary>
    /// If this origin point needs any setting up, this is where it happens.
    /// </summary>
    public abstract bool AttemptToResolveOriginPoint();

}
