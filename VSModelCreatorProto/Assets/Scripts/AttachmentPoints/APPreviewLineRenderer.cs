using UnityEngine;

public class APPreviewLineRenderer : MonoBehaviour
{

    public Material linesMaterial;
    public Color[] axisColors;
    public float lineLength = 0.1f;

    public void OnRenderObject()
    {
        //It appears we can't use Camera.current as it always returns null, but we _can_ use the current render texture!
        if (Camera.main.targetTexture.width != RenderTexture.active.width || Camera.main.targetTexture.height != RenderTexture.active.height)
        {
            return;
        }
        linesMaterial.SetPass(0);
        GL.PushMatrix();
        GL.modelview = Camera.main.worldToCameraMatrix;
        GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
        GL.Begin(GL.LINES);
        GL.Color(axisColors[0]);
        GL.Vertex(Vector3.zero + transform.position);
        GL.Vertex(transform.right * lineLength + transform.position);
        GL.Color(axisColors[1]);
        GL.Vertex(Vector3.zero + transform.position);
        GL.Vertex(transform.up * lineLength + transform.position);
        GL.Color(axisColors[2]);
        GL.Vertex(Vector3.zero + transform.position);
        GL.Vertex(transform.forward * -lineLength + transform.position);
        GL.End();
        GL.PopMatrix();
    }

}
