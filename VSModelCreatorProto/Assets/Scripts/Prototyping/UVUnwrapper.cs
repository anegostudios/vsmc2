using UnityEngine;
namespace VSMC
{
    public static class UVUnwrapper
    {

        // Cases:
        // N center, E left, W right, U above, D below, S very right
        // E center, S left, N right, U above, D below, W very right
        // S center, W left, E right, U above, D below, N very right
        // W center, N left, S right, U above, D below, E very right
        // U center, W left, E right, S above, N below, D very right
        // D center, E left, W right, N above, S below, U very right

        // First index = blockfacing.index
        // Second index:
        // 0 = center
        // 1 = left
        // 2 = right
        // 3 = above
        // 4 = below
        // 5 = very right
        // resulting value = blockfacing index:
        // 0 = N
        // 1 = E
        // 2 = S
        // 3 = W
        // 4 = U
        // 5 = D
        static int[][] allUvPositions = new int[][] {
		// N
		new int[] { 0, 1, 3, 4, 5, 2 },
		// E
		new int[] { 1, 2, 0, 4, 5, 3 },
		// S
		new int[] { 2, 3, 1, 4, 5, 0 },
		// W
		new int[] { 3, 0, 2, 4, 5, 1 },
		// U
		new int[] { 4, 3, 1, 0, 2, 5 },
		// D
		new int[] { 5, 1, 3, 0, 2, 4 },
		// U (Saratymode)
		new int[] { 4, 2, 0, 3, 1, 5 }
    };

        // Cases:
        // N center, D left, U right, W above, E below, S very right
        // E center, D left, U right, N above, S below, W very right
        // S center, D left, U right, E above, W below, N very right
        // W center, D left, U right, S above, N below, E very right
        // U center, N left, S right, E above, W below, D very right
        // D center, S left, N right, W above, E below, U very right
        static int[][] allUvPositionsAlternate = new int[][] {
		// N
		new int[] { 0, 4, 5, 3, 1, 2 },
		// E
		new int[] { 1, 4, 5, 0, 2, 3 },
		// S
		new int[] { 2, 4, 5, 1, 3, 0 },
		// W
		new int[] { 3, 4, 5, 2, 0, 1 },
		// U
		new int[] { 4, 0, 2, 1, 3, 5 },
		// D
		new int[] { 5, 0, 2, 3, 1, 4 },
		// U (Saratymode)
		new int[] { 4, 2, 0, 3, 1, 5 }
    };

        public static void DoAutoUV(ShapeElement elem)
        {
            if (elem.entityTextureUV == null)
            {
                elem.entityTextureUV = new double[] { 0, 0 };
            }
            
            ShapeElementFace[] faces = elem.FacesResolved;
            int unwrapMode = elem.entityTextureUnwrapMode;
            int unwrapRotation = elem.entityTextureUnwrapRotationIndex;
            if (unwrapMode <= 0)
            {
                PerformDefaultUVUnwrapping(elem);
                return;
            }


            for (int i = 0; i < 6; i++)
            {
                faces[i].RotationIndex = unwrapRotation;
            }

            if (unwrapMode - 1 == 0) faces[4].RotationIndex = (unwrapRotation + 2) % 4;
            if (unwrapMode - 1 == 2) faces[5].RotationIndex = (unwrapRotation + 2) % 4;

            if (unwrapMode - 1 == 0)
            {
                if (unwrapRotation == 1) faces[2].RotationIndex = (unwrapRotation + 2) % 4; ;
            }

            if (unwrapMode - 1 == 1)
            {
                faces[4].RotationIndex = (unwrapRotation + 3) % 4;
                faces[5].RotationIndex = (unwrapRotation + 3) % 4;

                if (unwrapRotation == 1) faces[3].RotationIndex = (unwrapRotation + 2) % 4; ;
            }

            if (unwrapMode - 1 == 2)
            {
                if (unwrapRotation == 1) faces[0].RotationIndex = (unwrapRotation + 2) % 4; ;
            }

            if (unwrapMode - 1 == 3)
            {
                faces[4].RotationIndex = (unwrapRotation + 1) % 4;
                faces[5].RotationIndex = (unwrapRotation + 1) % 4;

                if (unwrapRotation == 1) faces[1].RotationIndex = (unwrapRotation + 2) % 4; ;
            }
            if (unwrapMode - 1 == 4)
            {
                faces[0].RotationIndex = (unwrapRotation + 2) % 4;
                faces[1].RotationIndex = (unwrapRotation + 1) % 4;
                faces[3].RotationIndex = (unwrapRotation + 3) % 4;

                if (unwrapRotation == 1)
                {
                    faces[5].RotationIndex = (unwrapRotation + 2) % 4;
                }
            }
            if (unwrapMode - 1 == 5)
            {
                faces[2].RotationIndex = (unwrapRotation + 2) % 4;
                faces[1].RotationIndex = (unwrapRotation + 1) % 4;
                faces[3].RotationIndex = (unwrapRotation + 3) % 4;

                if (unwrapRotation == 1) faces[4].RotationIndex = (unwrapRotation + 2) % 4;
            }

            if (unwrapMode - 1 == 6)
            {
                faces[0].RotationIndex = (unwrapRotation) % 4;
                faces[1].RotationIndex = (unwrapRotation + 3) % 4;
                faces[2].RotationIndex = (unwrapRotation + 2) % 4;
                faces[3].RotationIndex = (unwrapRotation + 1) % 4;
                faces[4].RotationIndex = (unwrapRotation + 2) % 4;
                faces[5].RotationIndex = (unwrapRotation + 0) % 4;
            }


            int[] uvPositions = unwrapRotation == 1 ? allUvPositionsAlternate[unwrapMode - 1] : allUvPositions[unwrapMode - 1];
            Vector2 scale = faces[0].GetVoxelToPixelScale();

            ShapeElementFace aboveFace = faces[uvPositions[3]];
            ShapeElementFace veryRightFace = faces[uvPositions[5]];
            ShapeElementFace leftFace = faces[uvPositions[1]];
            ShapeElementFace centerFace = faces[uvPositions[0]];
            ShapeElementFace rightFace = faces[uvPositions[2]];
            ShapeElementFace belowFace = faces[uvPositions[4]];

            //This'll ensure that all the UVs have the correct widths and whatnot before starting.
            for (int i = 0; i < 6; i++)
            {
                //if (!faces[i].Enabled) continue;
                faces[i].CalculateAutoUV(elem.GetFaceDimension(i), true);
            }


            // Row 1
            float x = (float)elem.entityTextureUV[0];
            float y = (float)elem.entityTextureUV[1];

            if (leftFace.Enabled)
            {
                x += leftFace.uvWidth(true);
                // Fix any float imprecision first
                x = Mathf.Round(x * 1000.0f) / 1000.0f;
                // Now round to the next closest pixel
                x = Mathf.Ceil(x * scale.x) / scale.x;
            }

            aboveFace.Uv[0] = x;
            aboveFace.Uv[1] = y;
            aboveFace.CalculateAutoUV(elem.GetFaceDimension(uvPositions[3]), true);

            // Row 2
            if (aboveFace.Enabled) y += aboveFace.uvHeight(true);

            // Fix any float imprecision first
            y = Mathf.Round(y * 1000.0f) / 1000.0f;
            // Now round to the next closest pixel
            y = Mathf.Ceil(y * scale.y) / scale.y;

            x = (float)elem.entityTextureUV[0];

            leftFace.Uv[0] = x;
            leftFace.Uv[1] = y;
            leftFace.CalculateAutoUV(elem.GetFaceDimension(uvPositions[1]), true);


            if (leftFace.Enabled)
            {
                x += leftFace.uvWidth(true);
                // Fix any float imprecision first
                x = Mathf.Round(x * 1000.0f) / 1000.0f;
                // Now round to the next closest pixel
                x = Mathf.Ceil(x * scale.x) / scale.x;
            }

            centerFace.Uv[0] = x;
            centerFace.Uv[1] = y;
            centerFace.CalculateAutoUV(elem.GetFaceDimension(uvPositions[0]), true);

            if (centerFace.Enabled)
            {
                x += centerFace.uvWidth(true);
                // Fix any float imprecision first
                x = Mathf.Round(x * 1000.0f) / 1000.0f;
                // Now round to the next closest pixel
                x = Mathf.Ceil(x * scale.x) / scale.x;
            }

            rightFace.Uv[0] = x;
            rightFace.Uv[1] = y;
            rightFace.CalculateAutoUV(elem.GetFaceDimension(uvPositions[2]), true);

            if (rightFace.Enabled)
            {
                x += rightFace.uvWidth(true);
                // Fix any float imprecision first
                x = Mathf.Round(x * 1000.0f) / 1000.0f;
                // Now round to the next closest pixel
                x = Mathf.Ceil(x * scale.x) / scale.x;
            }

            veryRightFace.Uv[0] = x;
            veryRightFace.Uv[1] = y;
            veryRightFace.CalculateAutoUV(elem.GetFaceDimension(uvPositions[5]), true);


            // Row 3
            x = (float)elem.entityTextureUV[0];
            if (leftFace.Enabled)
            {
                x += leftFace.uvWidth(true);
                // Fix any float imprecision first
                x = Mathf.Round(x * 1000.0f) / 1000.0f;
                // Now round to the next closest pixel			
                x = Mathf.Ceil(x * scale.x) / scale.x;
            }

            y += Mathf.Max(leftFace.uvHeight(true), centerFace.uvHeight(true), rightFace.uvHeight(true), veryRightFace.uvHeight(true));
            // Fix any float imprecision first
            y = Mathf.Round(y * 1000.0f) / 1000.0f;
            // Now round to the next closest pixel
            y = Mathf.Ceil(y * scale.y) / scale.y;

            belowFace.Uv[0] = x;
            belowFace.Uv[1] = y;
            belowFace.CalculateAutoUV(elem.GetFaceDimension(uvPositions[4]), true);
        }

        private static void PerformDefaultUVUnwrapping(ShapeElement elem)
        {
            ShapeElementFace[] faces = elem.FacesResolved;
            Vector2 scale = faces[0].GetVoxelToPixelScale();

            float x = (float)elem.entityTextureUV[0];
            float y = (float)elem.entityTextureUV[1];
            float maxTexHeight = 0;

            if (faces[4].Enabled)// && faces[4].autoResolutionForUV)
            {
                faces[4].Uv[0] = x;
                faces[4].Uv[1] = y;
                faces[4].CalculateAutoUV(elem.GetFaceDimension(4), true);

                maxTexHeight = faces[4].uvHeight(true);
                x += faces[4].uvWidth(true);
            }

            x = Mathf.Ceil(x * scale.x) / scale.x;

            if (faces[5].Enabled)// && faces[5].autoResolutionForUV)
            {
                faces[5].Uv[0] = x;
                faces[5].Uv[1] = y;
                faces[5].CalculateAutoUV(elem.GetFaceDimension(5), true);

                maxTexHeight = Mathf.Max(maxTexHeight, faces[5].uvHeight(true));
            }

            x = (float)elem.entityTextureUV[0];
            y += maxTexHeight;


            y = Mathf.Ceil(y * scale.y) / scale.y;

            for (int side = 0; side < 4; side++)
            {
                ShapeElementFace face = faces[side];
                if (!face.Enabled) continue;// || !faces[side].autoResolutionForUV) continue;

                face.Uv[0] = x;
                face.Uv[1] = y;
                face.CalculateAutoUV(elem.GetFaceDimension(side), true);

                x += face.uvWidth(true);
                x = Mathf.Ceil(x * scale.x) / scale.x;
            }
        }


    }
}