using UnityEngine;

public class LineRendererScaler : MonoBehaviour
{

    public float initialLineSize = 1;
    LineRenderer lines;
    Transform cameraTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraTransform = Camera.main.transform;
        lines = GetComponent<LineRenderer>();    
    }

    // Update is called once per frame
    void Update()
    {
        if (!lines.enabled) return;
        //Ideally we want to control the size of the gizmos based on distance from the camera.
        float dist = Vector3.Distance(transform.position, cameraTransform.transform.position);
        float scale = Mathf.Max(dist, 0.1f) / initialLineSize; //Minimum value of 0.1f distance so lines are always visible even at very close distances.
        lines.startWidth = scale;
        lines.endWidth = scale;
    }
}
