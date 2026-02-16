using UnityEngine;

public class ForceNoRotationForChildren : MonoBehaviour
{

    public bool callOnUpdate;

    // Update is called once per frame
    void Update()
    {
        if (callOnUpdate) ForceNoRotation();
    }

    public void ForceNoRotation()
    {
        foreach (Transform t in transform)
        {
            t.rotation = Quaternion.identity;
        }
    }
}
