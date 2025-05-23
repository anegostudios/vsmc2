using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace VSMC {
    public class GridManager : MonoBehaviour
    {

        //Grid Properties
        public GridPreferences prefs;

        [Space(16)]
        //Grid Objects
        public GameObject gridOuter;
        public GameObject gridFloorMinorHori;
        public GameObject gridFloorMinorVert;
        public GameObject gridFloorMajorHori;
        public GameObject gridFloorMajorVert;

        #region Default Grid Position Definitions
        Vector3[] gridOuterDefaultPositions = new Vector3[]
        {
            new Vector3(-1, 0, -1), //-1
            new Vector3(-1, 0, 1),
            new Vector3(1,0,1),
            new Vector3(1,0,-1),
            new Vector3(-1, 0, -1), //4
            new Vector3(-1, 1, -1),
            new Vector3(1,1,-1),
            new Vector3(1,0,-1),
            new Vector3(-1, 0, -1), //8
            new Vector3(-1, 1, -1),
            new Vector3(-1,1,1),
            new Vector3(-1,0,1),
            new Vector3(1, 0, 1), //12
            new Vector3(1, 1, 1),
            new Vector3(-1, 1,1),
            new Vector3(1,1,1),
            new Vector3(1,1,-1), //1
        };
        #endregion

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ResizeGrids();
        }

        public void ResizeGrids()
        {
            //Concept:
            //  - Grid origin point is always 8, 0, 8. This is the bottom-center of an in-game block.
            //  - All calculations are divided by 16 to get the real Unity size.
            //  - Default Grid is 16x16x16.

            //Outer grid...
            LineRenderer gridOuterLines = gridOuter.GetComponent<LineRenderer>();
            Vector3[] gridOuterLinePositions = new Vector3[gridOuterDefaultPositions.Length];
            Vector3 minVals = new Vector3(8 - (prefs.gridSizes.x / 2f), 0, 8 - (prefs.gridSizes.z / 2f));
            Vector3 maxVals = new Vector3(8 + (prefs.gridSizes.x / 2f), prefs.gridSizes.y, 8 + (prefs.gridSizes.z / 2f));
            for (int i = 0; i < gridOuterLinePositions.Length; i++)
            {
                gridOuterLinePositions[i] = ConvertToRangedValue(gridOuterDefaultPositions[i], minVals, maxVals) / 16f;
            }
            gridOuterLines.SetPositions(gridOuterLinePositions);

            //Inner Minor Grids...
            //The minor grid works a little differently - It doesn't have stored values but instead follows a pattern.
            //Hori
            float cX = minVals.x;
            Vector3[] innerHoriLinesPositions = new Vector3[prefs.gridSizes.x * 2];
            for (int i = 0; i < prefs.gridSizes.x * 2; i++)
            {
                innerHoriLinesPositions[i] = new Vector3(cX, 0, i % 4 < 2 ? minVals.z : maxVals.z) / 16f;
                if (i % 2 == 0) cX += 1;
            }
            LineRenderer innerHoriLines = gridFloorMinorHori.GetComponent<LineRenderer>();
            innerHoriLines.positionCount = innerHoriLinesPositions.Length;
            innerHoriLines.SetPositions(innerHoriLinesPositions);

            //Vert
            float cZ = minVals.z;
            Vector3[] innerVertLinesPositions = new Vector3[prefs.gridSizes.z * 2];
            for (int i = 0; i < prefs.gridSizes.z * 2; i++)
            {
                innerVertLinesPositions[i] = new Vector3(i % 4 < 2 ? minVals.x : maxVals.x, 0, cZ) / 16f;
                if (i % 2 == 0) cZ += 1;
            }
            LineRenderer innerVertLines = gridFloorMinorVert.GetComponent<LineRenderer>();
            innerVertLines.positionCount = innerVertLinesPositions.Length;
            innerVertLines.SetPositions(innerVertLinesPositions);

            //Inner Major Grids
            //Same as minor, except it increases in intervals of 8. Though, this does change the start position.
            //Hori
            int mjrX = 8;
            int mjrCount = 0;
            //cba to figure out the maths so just loop until we can't loop anymore.
            while (mjrX > minVals.x)
            {
                mjrCount++;
                mjrX -= 8;
            }

            List<Vector3> majorHoriLinesPositions = new List<Vector3>();
            for (int i = 0; i < mjrCount * 4; i++)
            {
                majorHoriLinesPositions.Add(new Vector3(mjrX, 0, i % 4 < 2 ? minVals.z : maxVals.z) / 16f);
                if (i % 2 == 0) mjrX += 8;
            }

            //If major grid lines do not precisely fit, chop off the extras.
            if (prefs.gridSizes.x % 8 != 0)
            {
                majorHoriLinesPositions.RemoveAt(0);
                majorHoriLinesPositions.RemoveAt(majorHoriLinesPositions.Count - 1);
            }

            LineRenderer majorHoriLines = gridFloorMajorHori.GetComponent<LineRenderer>();
            majorHoriLines.positionCount = majorHoriLinesPositions.Count;
            majorHoriLines.SetPositions(majorHoriLinesPositions.ToArray());

            //Vert
            int mjrZ = 8;
            mjrCount = 0;
            //cba to figure out the maths so just loop until we can't loop anymore.
            while (mjrZ > minVals.z)
            {
                mjrCount++;
                mjrZ -= 8;
            }

            List<Vector3> majorVertLinesPositions = new List<Vector3>();
            for (int i = 0; i < mjrCount * 4; i++)
            {
                majorVertLinesPositions.Add(new Vector3(i % 4 < 2 ? minVals.x : maxVals.x, 0, mjrZ) / 16f);
                if (i % 2 == 0) mjrZ += 8;
            }

            //If major grid lines do not precisely fit, chop off the extras.
            if (prefs.gridSizes.z % 8 != 0)
            {
                majorVertLinesPositions.RemoveAt(0);
                majorVertLinesPositions.RemoveAt(majorVertLinesPositions.Count - 1);
            }

            LineRenderer majorVertLines = gridFloorMajorVert.GetComponent<LineRenderer>();
            majorVertLines.positionCount = majorVertLinesPositions.Count;
            majorVertLines.SetPositions(majorVertLinesPositions.ToArray());

        }

        Vector3 ConvertToRangedValue(Vector3 value, Vector3 min, Vector3 max)
        {
            float x, y, z;
            x = value.x < 0 ? value.x * -min.x : value.x * max.x;
            y = value.y < 0 ? value.y * -min.y : value.y * max.y;
            z = value.z < 0 ? value.z * -min.z : value.z * max.z;
            return new Vector3 (x, y, z);
        }
    }
}