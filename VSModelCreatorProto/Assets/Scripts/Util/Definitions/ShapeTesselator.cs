using System;
using System.Collections.Generic;
using UnityEngine;
namespace VSMC
{
    public class ShapeTesselator
    {

        static StackMatrix4 stackMatrix = new StackMatrix4(64);
        static Vector2[] storedTextureSizes;

        #region Mesh Data Arrays
        /// <summary>
        /// XYZ Vertex positions for every vertex in a cube. Origin is the cube middle point.
        /// </summary>
        public static int[] CubeVertices = {
            // North face
            -1, -1, -1,
            -1,  1, -1,
            1,  1, -1,
            1, -1, -1,

            // East face
            1, -1, -1,     // bot left
            1,  1, -1,     // top left
            1,  1,  1,     // top right
            1, -1,  1,     // bot right

            // South face
            -1, -1,  1,
            1, -1,  1,
            1,  1,  1,
            -1,  1,  1,

            // West face
            -1, -1, -1,
            -1, -1,  1,
            -1,  1,  1,
            -1,  1, -1,
            
            // Top face
            -1,  1, -1,
            -1,  1,  1,
            1,  1,  1,
            1,  1, -1,
                          
            // Bottom face
            -1, -1, -1,
            1, -1, -1,
            1, -1,  1,
            -1, -1,  1
        };

        public static int[] CubeLineIndices = {
            
            //Bottom Face
            20, 21, 22, 23, 20,
            //Top Face
            16, 17, 18, 19, 16


        };

        /// <summary>
        /// UV Coords for every Vertex in a cube
        /// </summary>
        public static int[] CubeUvCoords = {
            // North
            1, 0,
            1, 1,
            0, 1,
            0, 0,

            // East 
            1, 0,
            1, 1,
            0, 1,
            0, 0,

            // South
            0, 0,
            1, 0,
            1, 1,
            0, 1,
            
            // West
            0, 0,
            1, 0,
            1, 1,
            0, 1,

            // Top face
            0, 1,
            0, 0,
            1, 0,
            1, 1,

            // Bottom face
            1, 1,
            0, 1,
            0, 0,
            1, 0,
        };


        /// <summary>
        /// Indices for every triangle in a cube
        /// </summary>
        public static int[] CubeVertexIndices = {
            0, 1, 2,      0, 2, 3,    // North face
            4, 5, 6,      4, 6, 7,    // East face
            8, 9, 10,     8, 10, 11,  // South face
            12, 13, 14,   12, 14, 15, // West face
            16, 17, 18,   16, 18, 19, // Top face
            20, 21, 22,   20, 22, 23  // Bottom face
        };

        /// <summary>
        /// Can be used for any face if offseted correctly
        /// </summary>
        public static int[] BaseCubeVertexIndices =
        {
            0, 1, 2,      0, 2, 3
        };
        #endregion

        /// <summary>
        /// This will fill in all shape elements mesh data fields.
        /// </summary>
        /// <param name="shape"></param>
        public static void TesselateShape(Shape shape)
        {
            /*
             * Generating each shape's box is slightly different to how the game does it.
             * The game follows the pattern of:
             *      - foreach (ShapeElement):
             *          - Generate 3D Matrix
             *          - Generate Mesh Data
             *          - Repeat with Children
             * My code does the following:
             *      - foreach (ShapeElement):
             *          - Generate 3D Matrix
             *          - Repeat with Children
             *      - foreach (ShapeElement):
             *          - Generate Mesh Data
             *          - Repeat With Children
             * It's been done this way to allow me to visualize how to seperate the models to move them much more efficiently.
             * No point in recalculating the entire mesh if only one elements position has moved, basically.
             */

            /*
             * All of this is timed as well.
             * In general, shape matrices are taking less than a single millisecond to calculate for a complex model - So fast that they won't even log.
             * The mesh data takes slightly longer, averaging at a few milliseconds for a complex model.
             * Very promising for the live editor working at 60fps.
             */
            System.DateTime pre = System.DateTime.Now;
            TesselateShapeElements(shape.Elements);
            Debug.Log("Calculating mesh data for shape took " + (DateTime.Now - pre).TotalMilliseconds + "ms.");
    
            pre = System.DateTime.Now;
            ResolveAllMatricesForShape(shape);
            Debug.Log("Calculating full shape matrices took " + (DateTime.Now - pre).TotalMilliseconds + "ms.");

        }

        public static void ResolveMatricesForShapeElementAndChildren(ShapeElement element)
        {
            stackMatrix.Clear();
            stackMatrix.PushIdentity();
            //Add the parents stored matrix first, if it exists.
            if (element.ParentElement != null)
            {
                stackMatrix.Push(element.ParentElement.meshData.storedMatrix);
            }
            ResolveMatricesForShapeElements(new ShapeElement[] { element });
        }

        public static void RecreateMeshesForShapeElement(ShapeElement element)
        {
            //Tesselate element now.
            MeshData elementMeshData = element.meshData;
            elementMeshData.Clear();
            elementMeshData.meshName = element.Name;
            TesselateShapeElement(elementMeshData, element);
            elementMeshData.jointID = element.JointId;
        }

        public static void TesselateShapeElements(ShapeElement[] elements)
        {
            foreach (ShapeElement element in elements)
            {
                //Tesselate element now.
                MeshData elementMeshData = new MeshData();
                elementMeshData.meshName = element.Name;
                element.meshData = elementMeshData;
                TesselateShapeElement(elementMeshData, element);
                elementMeshData.jointID = element.JointId;

                //Now do children.
                if (element.Children != null)
                {
                    TesselateShapeElements(element.Children);
                }
            }
        }

        private static void TesselateShapeElement(MeshData meshData, ShapeElement element)
        {
            Vector3 size = new Vector3(
                ((float)element.To[0] - (float)element.From[0]) / 16f,
                ((float)element.To[1] - (float)element.From[1]) / 16f,
                ((float)element.To[2] - (float)element.From[2]) / 16f);
            if (size == Vector3.zero) return;

            Vector3 relativeCenter = size / 2;

            for (int f = 0; f < 6; f++)
            {
                ShapeElementFace face = element.FacesResolved[f];
                if (face == null || !face.Enabled) continue;
                BlockFacing facing = BlockFacing.ALLFACES[f];

                Vector2 uv1 = new Vector2(face.Uv[0], face.Uv[3]);
                Vector2 uv2 = new Vector2(face.Uv[2], face.Uv[1]);

                Vector2 uvSize = uv2 - uv1;
                int rot = (int)(face.Rotation / 90);

                AddFace(meshData, facing, relativeCenter, size, uv1, uvSize, face.textureIndex, rot % 4);
            }
        }

        private static void AddFace(MeshData modeldata, BlockFacing facing, Vector3 relativeCenter, Vector3 size, Vector2 uvStart, Vector2 uvSize, int faceTextureIndex, int uvRotation)
        {
            int coordPos = facing.index * 12; // 4 * 3 xyz's perface
            int uvPos = facing.index * 8;     // 4 * 2 uvs per face
            int lastVertexNumber = modeldata.vertices.Count;

            Vector2 storedTexSize = Vector2.one;
            if (faceTextureIndex >= 0 && faceTextureIndex < TextureManager.main.loadedTextures.Count)
            {
                storedTexSize = TextureManager.main.loadedTextures[faceTextureIndex].storedTextureSizeMultiplier;
            }

            for (int i = 0; i < 4; i++)
            {
                int uvIndex = 2 * ((uvRotation + i) % 4) + uvPos;
                modeldata.vertices.Add(new Vector3(relativeCenter.x + size.x * CubeVertices[coordPos++] / 2,
                    relativeCenter.y + size.y * CubeVertices[coordPos++] / 2,
                    -(relativeCenter.z + size.z * CubeVertices[coordPos++] / 2)));
                modeldata.uvs.Add(new Vector2(uvStart.x + uvSize.x * CubeUvCoords[uvIndex],
                    uvStart.y + uvSize.y * CubeUvCoords[uvIndex + 1]) / (TextureManager.main.maxTextureSize * storedTexSize));
                modeldata.textureIndices.Add(faceTextureIndex);
            }

            // 2 triangles = 6 indices per face
            modeldata.indices.Add(lastVertexNumber + 0);
            modeldata.indices.Add(lastVertexNumber + 2);
            modeldata.indices.Add(lastVertexNumber + 1);
            modeldata.indices.Add(lastVertexNumber + 0);
            modeldata.indices.Add(lastVertexNumber + 3);
            modeldata.indices.Add(lastVertexNumber + 2);

            int[] lines = new int[4];
            lines[0] = lastVertexNumber;
            lines[1] = lastVertexNumber + 1;
            lines[2] = lastVertexNumber + 2;
            lines[3] = lastVertexNumber + 3;
            modeldata.lineIndices.Add(lines);

        }

        public static void ResolveAllMatricesForShape(Shape shape)
        {
            stackMatrix.Clear();
            stackMatrix.PushIdentity();

            ResolveMatricesForShapeElements(shape.Elements);
        }

        private static void ResolveMatricesForShapeElements(ShapeElement[] elements)
        {
            Vector3 rotationOrigin = Vector3.zero;
            foreach (ShapeElement element in elements)
            {
                stackMatrix.Push();
                if (element.RotationOrigin == null)
                {
                    rotationOrigin = Vector3.zero;
                }
                else
                {
                    rotationOrigin = new Vector3((float)element.RotationOrigin[0], (float)element.RotationOrigin[1], (float)element.RotationOrigin[2]);
                    stackMatrix.Translate(rotationOrigin.x / 16, rotationOrigin.y / 16, rotationOrigin.z / 16);
                }

                stackMatrix.Rotate(element.RotationX, element.RotationY, element.RotationZ);
                stackMatrix.Scale(element.ScaleX, element.ScaleY, element.ScaleZ);
                stackMatrix.Translate((element.From[0] - rotationOrigin.x) / 16.0f, (element.From[1] - rotationOrigin.y) / 16.0f, (element.From[2] - rotationOrigin.z) / 16.0);

                //Clone the matrix for the element.
                element.meshData.storedMatrix = stackMatrix.Top * Matrix4x4.identity;

                //Now do children.
                if (element.Children != null)
                {
                    ResolveMatricesForShapeElements(element.Children);
                }
                stackMatrix.Pop();
            }
        }

    }
}