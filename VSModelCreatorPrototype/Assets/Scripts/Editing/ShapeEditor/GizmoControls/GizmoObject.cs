using UnityEngine;

namespace VSMC {

    /// <summary>
    /// A single interactable 3D object that represents the transform gizmo. Used to control pulling direction.
    /// </summary>
    public class GizmoObject : MonoBehaviour
    {

        public EnumAxis gizmoAxis;
        public GizmoMode gizmoMode;
        public Color hoverColor;
        public Color defaultColor;
        MeshRenderer mr;

        private void Awake()
        {
            mr = GetComponent<MeshRenderer>();
            OnHoverEnd();
        }

        public Vector3 PointingDirection()
        {
            if (gizmoAxis == EnumAxis.X) return transform.right;
            else if (gizmoAxis == EnumAxis.Y) return transform.up;
            else return transform.forward;
        }

        public void OnHoverStart()
        {
            mr.material.color = hoverColor;
        }

        public void OnHoverEnd()
        {
            mr.material.color = defaultColor;
        }

    }
}