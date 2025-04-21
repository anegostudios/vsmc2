using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;
using System.Runtime.Serialization;

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
    /// An array holding the faces of this shape element in BlockFacing order: North, East, South, West, Up, Down.  May be null if not present or not enabled.
    /// </summary>
    [DoNotSerialize()]
    public ShapeElementFaceJSON[] FacesResolved = new ShapeElementFaceJSON[6];

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

    public Matrix4x4 cachedMatrix;
    public int storedMeshID;

    /// <summary>
    /// Sets up a local transform matrix for the shape element. Mostly copied from game code.
    /// Not actually used lol
    /// </summary>
    public Matrix4x4 GetLocalTransformMatrix(int animVersion = 1, Matrix4x4? output = null)
    {
        if (output == null) output = Matrix4x4.identity;

        ShapeElementJSON elem = this;
        Vector3 origin = new Vector3();

        //Setup rotation origin.
        if (elem.RotationOrigin != null)
        {
            origin.x = (float)elem.RotationOrigin[0] / 16;
            origin.y = (float)elem.RotationOrigin[1] / 16;
            origin.z = (float)elem.RotationOrigin[2] / 16;
        }

        // R = rotate, S = scale, T = translate
        // Version 0: R * S * T
        // Version 1: T * S * R

        if (animVersion == 1)
        {
            output *= Matrix4x4.Translate(origin);
            output *= Matrix4x4.Scale(new Vector3((float)elem.ScaleX, (float)elem.ScaleY, (float)elem.ScaleZ));

            //Rotation. May be source of problems, as base game splits this to X, then Y, then Z. Should be fine though.
            output *= Matrix4x4.Rotate(Quaternion.Euler((float)elem.RotationX, (float)elem.RotationY, (float)elem.RotationZ));
            output *= Matrix4x4.Translate(-origin);
            
            //Going to ignore animation for now.
        }
        return output.Value;
    }

    /// <summary>
    /// Resolves the face indices and the texture codes for the element and its children.
    /// </summary>
    public void ResolveFacesAndTextures(Dictionary<string, string> textures)
    {
        if (Faces != null)
        {
            foreach (var val in Faces)
            {
                ShapeElementFaceJSON f = val.Value;
                f.ResolveTexture(textures);
                if (!f.Enabled) continue;
                BlockFacing facing = BlockFacing.FromFirstLetter(val.Key);
                FacesResolved[facing.index] = f;
            }
            if (Children != null)
            {
                foreach (ShapeElementJSON child in Children)
                {
                    child.ResolveFacesAndTextures(textures);
                }
            }
        }
    }

}
