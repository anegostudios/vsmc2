using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class ShapeElementJSON
{

    /// <summary>
    /// The name of the ShapeElement
    /// </summary>
    public string Name;

    public double[] From;
    public double[] To;

    /// <summary>
    /// Whether or not the shape element is shaded.
    /// </summary>
    public bool Shade = true;

    public bool GradientShade = false;

    /// <summary>
    /// The faces of the shape element by name (will normally be null except during object deserialization: use FacesResolved instead!)
    /// </summary>
    public Dictionary<string, ShapeElementFaceJSON> Faces;

    /// <summary>
    /// The origin point for rotation.
    /// </summary>
    public double[] RotationOrigin;

    /// <summary>
    /// The forward vertical rotation of the shape element.
    /// </summary>
    public double RotationX;

    /// <summary>
    /// The forward vertical rotation of the shape element.
    /// </summary>
    public double RotationY;

    /// <summary>
    /// The left/right tilt of the shape element
    /// </summary>
    public double RotationZ;

    /// <summary>
    /// How far away are the left/right sides of the shape from the center
    /// </summary>
    public double ScaleX = 1;

    /// <summary>
    /// How far away are the top/bottom sides of the shape from the center
    /// </summary>
    public double ScaleY = 1;

    /// <summary>
    /// How far away are the front/back sides of the shape from the center.
    /// </summary>
    public double ScaleZ = 1;

    public string ClimateColorMap = null;
    public string SeasonColorMap = null;
    public short RenderPass = -1;
    public short ZOffset = 0;
    /// <summary>
    /// Set this to true to disable randomDrawOffset and randomRotations on this specific element (e.g. used for the ice element of Coopers Reeds in Ice)
    /// </summary>
    public bool DisableRandomDrawOffset;

    /// <summary>
    /// The child shapes of this shape element
    /// </summary>
    public ShapeElementJSON[] Children;

    /// <summary>
    /// The attachment points for this shape.
    /// </summary>
    public AttachmentPointJSON[] AttachmentPoints;

    /// <summary>
    /// The "remote" parent for this element
    /// </summary>
    public string StepParentName;

}
