using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// This should be attached to the camera anchor. The camera itself will rotate around this.
/// </summary>
public class CameraController : MonoBehaviour
{

    public GameObject cameraChild;

    public float rotX;
    public float rotY;

    public Vector3 cameraAnchorPos;
    public float distFromAnchor;

    public Vector2 minMaxRotX;
    public Vector2 minMaxDistance;

    InputAction mouseScrollwheelAction;
    InputAction rmbAction;
    InputAction mousePosAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mouseScrollwheelAction = InputSystem.actions.FindAction("ScrollWheel");
        rmbAction = InputSystem.actions.FindAction("RightClick");
        mousePosAction = InputSystem.actions.FindAction("Look");
    }

    // Update is called once per frame
    void Update()
    {
        DoMouseUpdates();
        gameObject.transform.localPosition = cameraAnchorPos;
        gameObject.transform.localEulerAngles = new Vector3(rotX, rotY, 0);
        cameraChild.transform.localPosition = new Vector3(0, 0, -distFromAnchor);
    }

    void DoMouseUpdates()
    {
        if (rmbAction.IsPressed())
        {
            Vector2 mouseMovement = mousePosAction.ReadValue<Vector2>();
            rotY = (rotY + mouseMovement.x) % 360;
            rotX -= mouseMovement.y;
        }

        distFromAnchor -= mouseScrollwheelAction.ReadValue<Vector2>().y;
        distFromAnchor = Mathf.Clamp(distFromAnchor, minMaxDistance.x, minMaxDistance.y);
        rotX = Mathf.Clamp(rotX, minMaxRotX.x, minMaxRotX.y);
    }
}
