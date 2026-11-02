using UnityEngine;

public class VSMCLineRenderer : MonoBehaviour
{

    public Material linesMaterial;
    public Color lineColor;
    public int priority;
    public Vector3[] positions;
    public bool doRendering;

    void Update()
    {
        VSMCLineManager.main.AddLineRenderer(this);
    }

    public void CopyFromOther(VSMCLineRenderer other)
    {
        lineColor = other.lineColor;
        priority = other.priority;
        positions = other.positions;
        doRendering = other.doRendering;        
    }

    public void RenderLines()
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
            GL.Vertex(transform.TransformPoint(positions[0]));
            GL.Vertex(transform.TransformPoint(positions[positions.Length - 1]));
            if (priority > 0 && positions.Length >= 2)
            {
                GL.Color(new Color(lineColor.r, lineColor.g, lineColor.b, lineColor.a / 2));
                GL.Vertex(transform.TransformPoint(positions[0]));
                GL.Vertex(transform.TransformPoint(positions[2]));
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