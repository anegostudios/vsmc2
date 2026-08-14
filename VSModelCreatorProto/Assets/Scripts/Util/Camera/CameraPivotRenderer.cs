using UnityEngine;
using UnityEngine.Animations;

public class CameraPivotRenderer : MonoBehaviour
{

    public Material linesMaterial;
    public Color[] axisColors;
    public float lineLength = 0.1f;

    void OnRenderObject()
    {
        //It appears we can't use Camera.current as it always returns null, but we _can_ use the current render texture!
        if (Camera.main.targetTexture.width != RenderTexture.active.width || Camera.main.targetTexture.height != RenderTexture.active.height)
        {
            return;
        }
        //if (Camera.current == null || Camera.current.tag != "MainCamera") return;
        linesMaterial.SetPass(0);
        GL.PushMatrix();
        GL.modelview = Camera.main.worldToCameraMatrix;
        GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
        GL.Begin(GL.LINES);
        GL.Color(axisColors[0]);
        GL.Vertex(new Vector3(-1, 0, 0) * lineLength + transform.position);
        GL.Vertex(new Vector3(1, 0, 0) * lineLength + transform.position);
        GL.Color(axisColors[1]);
        GL.Vertex(new Vector3(0, -1, 0) * lineLength + transform.position);
        GL.Vertex(new Vector3(0, 1, 0) * lineLength + transform.position);
        GL.Color(axisColors[2]);
        GL.Vertex(new Vector3(0, 0, -1) * lineLength + transform.position);
        GL.Vertex(new Vector3(0, 0, 1) * lineLength + transform.position);
        GL.End();
        GL.PopMatrix();
    }
    

}
