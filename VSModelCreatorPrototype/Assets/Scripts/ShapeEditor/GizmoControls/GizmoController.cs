using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace VSMC
{
    /// <summary>
    /// The gizmo controller is responsible for all 3D gizmos for editing controls.
    /// There are four different gizmo modes for the three different transformation types, as well as the rotation origin.
    /// Only one gizmo mode will be selected at any one point, but they all work very similarly.
    /// </summary>
    public class GizmoController : ISceneRaycaster
    {
        public GizmoMode cGizmoMode;

        [Header("Unity References")]
        public ModelEditor shapeModelEditor;
        public GameObject gizmosHolderParent;
        public GameObject[] gizmoParentsByMode;
        public string XGizmoLayer;
        public string YGizmoLayer;
        public string ZGizmoLayer;
        public float initialiCorrectDistanceForGizmos;

        //Each gizmo type has three axes that can be moved.
        public ShapeElementGameObject cSelected;
        public bool isAnyAxisSelected = false;
        public EnumAxis cSelAxis;
        public GizmoObject cHoveredGizmo = null;

        //Temp movement values
        public Vector2 sceneMousePosOnGizmoDown;
        public Vector2 cGizmoPositiveDirection;
        float distFromCam;
        double cFromVal;
        Transform mainCameraPos;
        int randomTransformationUID;

        private void Start()
        {
            EditModeManager.RegisterForOnModeSelect(OnModeSelect);
            ObjectSelector.main.RegisterForObjectSelectedEvent(OnObjectSelected);
            ObjectSelector.main.RegisterForObjectDeselectedEvent(OnObjectDeselected);
            mainCameraPos = Camera.main.transform;
            UndoManager.RegisterForAnyActionDoneOrUndone(OnAnyAction);
        }

        private void Update()
        {
            //Ideally we want to control the size of the gizmos based on distance from the camera.
            float dist = Vector3.Distance(gizmosHolderParent.transform.position, mainCameraPos.transform.position);
            float scale = dist / initialiCorrectDistanceForGizmos;
            gizmosHolderParent.transform.localScale = scale * Vector3.one;
        }

        public void OnModeSelect(VSEditMode mode)
        {
            if (mode != VSEditMode.Model)
            {
                gizmosHolderParent.gameObject.SetActive(false);
            }
            else
            {
                if (cSelected)
                {
                    SetAppropriateTransformOfGizmos();
                    gizmosHolderParent.gameObject.SetActive(true);
                }
            }
        }

        public void OnAnyAction()
        {
            if (cSelected == null || cSelected.element == null) return;
            SetAppropriateTransformOfGizmos();
        }

        public void SetGizmoMode(int mode)
        {
            if (mode == (int)cGizmoMode) return;
            gizmoParentsByMode[(int)cGizmoMode].SetActive(false);
            cGizmoMode = (GizmoMode)mode;
            gizmoParentsByMode[mode].SetActive(true);
        }

        /// <summary>
        /// Calculates and sets the position and rotation of the gizmos.
        /// </summary>
        public void SetAppropriateTransformOfGizmos()
        {
            Vector3 rotOrig = new Vector3((float)cSelected.element.RotationOrigin[0], (float)cSelected.element.RotationOrigin[1], (float)cSelected.element.RotationOrigin[2]);
            Vector3 from = new Vector3((float)cSelected.element.From[0], (float)cSelected.element.From[1], (float)cSelected.element.From[2]);
            
            //This code is noteworthy - It's how we access the real rotation point from a shape element.
            gizmosHolderParent.transform.position = cSelected.transform.position - (cSelected.transform.rotation * (from / 16f)) + (cSelected.transform.rotation * (rotOrig / 16f));
            gizmosHolderParent.transform.rotation = cSelected.transform.rotation;
        }

        public override bool OnSceneViewMouseDown(Vector2 mouseClickScenePositionForCamera, PointerEventData data)
        {
            if (data.button != 0 || cSelected == null || EditModeManager.main.cEditMode != VSEditMode.Model) return false;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(mouseClickScenePositionForCamera), out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Edit Pulleys")))
            {
                GizmoObject gizmo = hit.collider.GetComponent<GizmoObject>();
                if (gizmo != null)
                {
                    randomTransformationUID = Random.Range(int.MinValue, int.MaxValue);
                    isAnyAxisSelected = true;
                    cSelAxis = gizmo.gizmoAxis;
                    sceneMousePosOnGizmoDown = mouseClickScenePositionForCamera;
                    Vector3 screenSpaceOfObject = Camera.main.worldToCameraMatrix * (gizmo.transform.position);
                    distFromCam = screenSpaceOfObject.z;
                    Vector3 screenSpaceOfPointOfMovement = Camera.main.worldToCameraMatrix * (gizmo.transform.position + gizmo.PointingDirection());
                    cGizmoPositiveDirection = (screenSpaceOfPointOfMovement - screenSpaceOfObject).normalized;
                    cFromVal = cSelected.element.From[(int)cSelAxis];

                    //Need to reverse the Z axis due to model flipping.
                    /*
                    if (cSelAxis == EnumAxis.Z)
                    {
                        cGizmoPositiveDirection *= -1;
                    }
                    */
                }
                return true;
            }

            return false;
        }

        public override void OnUpdateOverSceneView(Vector2 mouseScenePositionForCamera)
        {
            //Highlight hovered axis.
            if (Physics.Raycast(Camera.main.ScreenPointToRay(mouseScenePositionForCamera), out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Edit Pulleys")))
            {
                GizmoObject gizmo = hit.collider.GetComponent<GizmoObject>();
                if (gizmo != cHoveredGizmo)
                {
                    gizmo.OnHoverStart();
                    cHoveredGizmo?.OnHoverEnd();
                    cHoveredGizmo = gizmo;
                }
            }
            else
            {
                cHoveredGizmo?.OnHoverEnd();
                cHoveredGizmo = null;
            }

            if (isAnyAxisSelected)
            {
                Vector2 diff = mouseScenePositionForCamera - sceneMousePosOnGizmoDown;
                Vector2 relDiff = diff * cGizmoPositiveDirection;
                float relDiffFloat = (relDiff.x + relDiff.y) / 2f;
                float roundedDiff = (relDiffFloat * (1 / 16f));

                //We now have the value to move by, but it needs potentially rounding.
                //Default snapping value is 0.5f, but this could be changed easily.
                //If Ctrl held down, do not clamp.
                float nearestMultiple = roundedDiff;
                if (!Input.GetKey(KeyCode.LeftControl))
                {
                    float factor = (1 / 2f);
                    nearestMultiple = (int)System.Math.Round((roundedDiff / factor), System.MidpointRounding.AwayFromZero) * factor;
                }
                shapeModelEditor.SetPosition(cSelAxis, (float)(cFromVal + nearestMultiple), randomTransformationUID);
            }
        }

        public override bool OnSceneViewMouseScroll(PointerEventData data)
        {
            return false;
        }

        public override bool OnSceneViewMouseUp(PointerEventData data)
        {
            if (isAnyAxisSelected)
            {
                isAnyAxisSelected = false;
                UndoManager.main.MergeTopTasks();
                return true;
            }
            return false;
        }

        public void OnObjectSelected(GameObject selected)
        {
            cSelected = selected.GetComponent<ShapeElementGameObject>();
            //Replace the selection, but if not in model mode, do not show the gizmos.
            if (EditModeManager.main.cEditMode != VSEditMode.Model) return;
            SetAppropriateTransformOfGizmos();
            gizmosHolderParent.gameObject.SetActive(true);
        }

        public void OnObjectDeselected(GameObject deselected)
        {
            cSelected = null;
            gizmosHolderParent.gameObject.SetActive(false);
        }
    }
}