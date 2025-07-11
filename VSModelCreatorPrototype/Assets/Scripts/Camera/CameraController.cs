using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace VSMC
{
    /// <summary>
    /// This should be attached to the camera anchor. The camera itself will rotate around this.
    /// This is a very primitive camera controller. It has no smoothing or other such QOL features.
    /// </summary>
    public class CameraController : ISceneRaycaster
    {

        enum CameraMode
        {
            Orbital = 0,
            Free = 1
        }

        [Header("Unity References")]
        public GameObject cameraChild;
        public GameObject pivotChild;
        public GameObject cameraCompass;
        public RawImage sceneViewRawImage;

        [Header("Button References")]
        public TMP_Text cameraModeButtonText;

        float rotX;
        float rotY;

        Vector3 cameraAnchorPos;
        float distFromAnchor = 1;

        [Header("Orbital Settings")]
        public Vector2 minMaxRotX;
        public Vector2 minMaxDistance;

        [Header("Speeds")]
        public float movementSpeed = 0.1f;
        public float rotationSpeed = 0.5f;
        public float zoomSpeed = 0.2f;


        public ShapeModelEditor editor;
        InputAction mousePosAction;

        bool lmbDown = false;
        bool hasMovedSinceLmbDown = false;
        bool rmbDown = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            mousePosAction = InputSystem.actions.FindAction("Look");
            cameraAnchorPos = new Vector3();
            cameraModeButtonText.text = CurrentCameraMode.ToString();
        }

        // Update is called once per frame
        void Update()
        {
            DoMouseUpdates();
            gameObject.transform.localPosition = cameraAnchorPos;
            gameObject.transform.localEulerAngles = new Vector3(rotX, rotY, 0);

            cameraChild.transform.localPosition = new Vector3(0, 0, CurrentCameraMode == CameraMode.Orbital ? -distFromAnchor : 0);
            cameraCompass.transform.localEulerAngles = new Vector3(0, 0, rotY);
            if (pivotChild != null)
            {
                pivotChild.SetActive(CurrentCameraMode == CameraMode.Orbital);
                pivotChild.transform.rotation = Quaternion.identity;
            }
        }

        public override bool OnSceneViewMouseDown(Vector2 mouseClickScenePositionForCamera, PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left)
            {
                hasMovedSinceLmbDown = false;
                lmbDown = true;
                return false;
            }
            else if (data.button == PointerEventData.InputButton.Right) rmbDown = true;
            return true;
        }


        public override bool OnSceneViewMouseUp(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left)
            {
                //This may be a useless check, but I'd like to add it just in case.
                if (lmbDown)
                {
                    lmbDown = false;
                    //if the user has moved the camera, make sure the object selection does not kick in.
                    return hasMovedSinceLmbDown;
                }
            }
            else if (data.button == PointerEventData.InputButton.Right)
            {
                rmbDown = false;
                return true;
            }
            return false;
        }

        public override bool OnSceneViewMouseScroll(PointerEventData data)
        {
            if (CurrentCameraMode == CameraMode.Orbital)
            {
                distFromAnchor -= data.scrollDelta.y * zoomSpeed;
                distFromAnchor = Mathf.Clamp(distFromAnchor, minMaxDistance.x, minMaxDistance.y);
            }
            else
            {
                cameraAnchorPos += data.scrollDelta.y * zoomSpeed * (cameraChild.transform.forward);
            }
            return true;
        }

        void DoMouseUpdates()
        {
            Vector2 mouseMovement = mousePosAction.ReadValue<Vector2>();
            if (lmbDown)
            {
                //Using 'transform.right' and up here allow us to move the camera anchor in reference to the camera's angle.
                if (mouseMovement.sqrMagnitude >= Mathf.Epsilon)
                {
                    cameraAnchorPos += cameraChild.transform.right * mouseMovement.x * movementSpeed;
                    cameraAnchorPos += cameraChild.transform.up * mouseMovement.y * movementSpeed;
                    hasMovedSinceLmbDown = true;
                }
            }

            if (rmbDown)
            {
                rotY = (rotY + (mouseMovement.x * rotationSpeed)) % 360;
                rotX -= (mouseMovement.y * rotationSpeed);
            }

            rotX = Mathf.Clamp(rotX, minMaxRotX.x, minMaxRotX.y);
        }

        public void SwapCameraType()
        {
            if (CurrentCameraMode == CameraMode.Orbital)
            {
                CurrentCameraMode = CameraMode.Free;
                cameraAnchorPos -= (cameraChild.transform.forward) * distFromAnchor;
            }
            else
            {
                CurrentCameraMode = CameraMode.Orbital;
                cameraAnchorPos += (cameraChild.transform.forward) * distFromAnchor;
            }
            cameraModeButtonText.text = CurrentCameraMode.ToString();
        }

        public void ResetCamera()
        {
            rotX = 0;
            rotY = 0;
            distFromAnchor = 10;
            cameraAnchorPos = Vector3.zero;
        }

        CameraMode CurrentCameraMode
        {
            get
            {
                return (CameraMode)ProgramPreferences.CurrentCameraMode.GetValue();
            }
            set
            {
                ProgramPreferences.CurrentCameraMode.SetValue((int)value);
            }
        }


    }
}