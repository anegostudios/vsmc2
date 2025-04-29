using UnityEngine;
using UnityEngine.InputSystem;
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

    InputAction mouseScrollwheelAction;
    InputAction rmbAction;
    InputAction mousePosAction;
    InputAction lmbAction;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mouseScrollwheelAction = InputSystem.actions.FindAction("ScrollWheel");
        rmbAction = InputSystem.actions.FindAction("RightClick");
        mousePosAction = InputSystem.actions.FindAction("Look");
        lmbAction = InputSystem.actions.FindAction("Click");
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

    void DoMouseUpdates()
    {
        Vector2 mouseMovement = mousePosAction.ReadValue<Vector2>();
        if (lmbAction.IsPressed())
        {
            //Using 'transform.right' and up here allow us to move the camera anchor in reference to the camera's angle.
            cameraAnchorPos += cameraChild.transform.right * mouseMovement.x * lmbMovementSpeed;
            cameraAnchorPos += cameraChild.transform.up * mouseMovement.y * lmbMovementSpeed;
        }

        if (rmbAction.IsPressed())
        {
            rotY = (rotY + (mouseMovement.x * rmbRotationSpeed)) % 360;
            rotX -= (mouseMovement.y * rmbRotationSpeed);
        }

        distFromAnchor -= mouseScrollwheelAction.ReadValue<Vector2>().y * zoomSpeed;
        distFromAnchor = Mathf.Clamp(distFromAnchor, minMaxDistance.x, minMaxDistance.y);
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
