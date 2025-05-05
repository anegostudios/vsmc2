using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// This should be attached to the camera anchor. The camera itself will rotate around this.
/// This is a very primitive camera controller. It has no smoothing or other such QOL features.
/// </summary>
public class CameraController : MonoBehaviour
{

    public GameObject cameraChild;
    public GameObject pivotChild;
    public GameObject cameraCompass;
    public RawImage sceneViewRawImage;

    public float rotX;
    public float rotY;

    public Vector3 cameraAnchorPos;
    public float distFromAnchor;

    public Vector2 minMaxRotX;
    public Vector2 minMaxDistance;

    [Header("Speeds")]
    public float lmbMovementSpeed = 0.1f;
    public float rmbRotationSpeed = 0.5f;
    public float zoomSpeed = 0.2f;

    InputAction mousePosAction;
    bool lmbDown = false;
    bool rmbDown = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mousePosAction = InputSystem.actions.FindAction("Look");
        cameraAnchorPos = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        DoMouseUpdates();
        gameObject.transform.localPosition = cameraAnchorPos;
        gameObject.transform.localEulerAngles = new Vector3(rotX, rotY, 0);
        cameraChild.transform.localPosition = new Vector3(0, 0, -distFromAnchor);
        cameraCompass.transform.localEulerAngles = new Vector3(0,0,rotY);
    }

    public void SceneViewMouseDown(BaseEventData data)
    {
        if ((data as PointerEventData).button == PointerEventData.InputButton.Left) lmbDown = true;
        else if ((data as PointerEventData).button == PointerEventData.InputButton.Right) rmbDown = true;
    }

    public void SceneViewMouseUp(BaseEventData data)
    {
        if ((data as PointerEventData).button == PointerEventData.InputButton.Left) lmbDown = false;
        else if ((data as PointerEventData).button == PointerEventData.InputButton.Right) rmbDown = false;
    }

    public void SceneViewMouseScroll(BaseEventData data)
    {
        distFromAnchor -= (data as PointerEventData).scrollDelta.y * zoomSpeed;
        distFromAnchor = Mathf.Clamp(distFromAnchor, minMaxDistance.x, minMaxDistance.y);
    }

    void DoMouseUpdates()
    {
        Vector2 mouseMovement = mousePosAction.ReadValue<Vector2>();
        if (lmbDown)
        {
            //Using 'transform.right' and up here allow us to move the camera anchor in reference to the camera's angle.
            cameraAnchorPos += cameraChild.transform.right * mouseMovement.x * lmbMovementSpeed;
            cameraAnchorPos += cameraChild.transform.up * mouseMovement.y * lmbMovementSpeed;
        }

        if (rmbDown)
        {
            rotY = (rotY + (mouseMovement.x * rmbRotationSpeed)) % 360;
            rotX -= (mouseMovement.y * rmbRotationSpeed);
        }

        rotX = Mathf.Clamp(rotX, minMaxRotX.x, minMaxRotX.y);
    }

    public void ResetCamera()
    {
        rotX = 0;
        rotY = 0;
        distFromAnchor = 10;
        cameraAnchorPos = Vector3.zero;
    }
}
