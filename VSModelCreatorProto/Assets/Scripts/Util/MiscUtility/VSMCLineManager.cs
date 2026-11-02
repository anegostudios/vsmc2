using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VSMCLineManager : MonoBehaviour
{
    public static VSMCLineManager main;

    List<VSMCLineRenderer> lines;

    void Awake()
    {
        main = this;
        lines = new List<VSMCLineRenderer>();
    }

    public void AddLineRenderer(VSMCLineRenderer renderer)
    {
        lines.Add(renderer);
    }

    void OnRenderObject()
    {
        foreach (VSMCLineRenderer renderer in lines.OrderBy(x => x.priority))
        {
            if (renderer != null)
            {
                renderer.RenderLines();
            }
        }
        lines.Clear();
    }

}