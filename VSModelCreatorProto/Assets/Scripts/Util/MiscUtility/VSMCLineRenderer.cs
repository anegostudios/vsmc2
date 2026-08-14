using UnityEngine;

public class VSMCLineRenderer : MonoBehaviour
{

    public Material linesMaterial;
    public Color lineColor;
    public Vector3[] positions;
    public bool doRendering;


    void OnRenderObject()
    {
        if (!doRendering) return;
        //It appears we can't use Camera.current as it always returns null, but we _can_ use the current render texture!
        if (Camera.main.targetTexture.width != RenderTexture.active.width || Camera.main.targetTexture.height != RenderTexture.active.height)
        {
            return;
        }

        if (positions == null || positions.Length == 0) return;

        if (linesMaterial.SetPass(0))
        {
            GL.PushMatrix();
            GL.modelview = Camera.main.worldToCameraMatrix;
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.Begin(GL.LINES);
            GL.Color(lineColor);
            for (int i = 0; i < positions.Length - 1; i++)
            {
                GL.Vertex(transform.TransformPoint(positions[i]));
                GL.Vertex(transform.TransformPoint(positions[i + 1]));
            }
            GL.End();
            GL.PopMatrix();
        }
        else
        {
            Debug.Log("Failed to set line material pass.");
        }
    }

}