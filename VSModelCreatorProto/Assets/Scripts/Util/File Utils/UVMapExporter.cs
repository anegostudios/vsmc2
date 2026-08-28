using System;
using System.IO;
using SFB;
using UnityEngine;
using VSMC;

public class UVMapExporter
{

    public static Color[] ColorsByFace = new Color[] { //North, East, South, West, Up, Down

            new Color(179 / 255f, 193 / 255f, 255 / 255f),
			new Color(255 / 255f, 179 / 255f, 179 / 255f),
            new Color(179 / 255f, 193 / 255f, 255 / 255f),
            new Color(255 / 255f, 179 / 255f, 179 / 255f),
            new Color(187 / 255f, 255 / 255f, 179 / 255f),
            new Color(187 / 255f, 255 / 255f, 179 / 255f)
    };

public static void CalculateAndExportUVMap()
    {

        string path = StandaloneFileBrowser.SaveFilePanel("Export UV Image", "", "", "png");
        if (String.IsNullOrEmpty(path))
        {
            return;
        }
        if (!path.EndsWith(".png"))
        {
            path += ".png";
        }

        int sx = 0;
        int sy = 0; 
        foreach (LoadedTexture t in TextureManager.main.loadedTextures)
        {
            if (t.storedWidth > sx) sx = t.storedWidth;
            if (t.storedHeight > sy) sy = t.storedHeight;
        }
        Texture2D uvTex = new Texture2D(sx * 2, sy * 2);
        for (int i = 0; i < uvTex.width; i++)
        {
            for (int j = 0; j < uvTex.height; j++)
            {
                uvTex.SetPixel(i, j, Color.clear);
            }
        }

        foreach (ShapeElement e in ShapeElementRegistry.main.GetAllShapeElements())
        {
            for (int i = 0; i < 6; i++)
            {
                ShapeElementFace f = e.FacesResolved[i];
                if (!f.Enabled) continue;
                int uvsx = Mathf.FloorToInt(f.Uv[0] * 2);
                int uvey = uvTex.height - Mathf.FloorToInt(f.Uv[1] * 2);
                int uvex = Mathf.CeilToInt(f.Uv[2] * 2);
                int uvsy = uvTex.height - Mathf.CeilToInt(f.Uv[3] * 2);

                Color c = ColorsByFace[i] * e.GetFaceBrightness(i);
                c.a = 76 / 255f;

                int size = (uvex - uvsx) * (uvey - uvsy);
                Color[] cs = new Color[size];
                for (int ci = 0; ci < size; ci++)
                {
                    cs[ci] = c;
                }

                uvTex.SetPixels(uvsx, uvsy, uvex - uvsx, uvey - uvsy, cs);
            }
        }

        uvTex.Apply();
        File.WriteAllBytes(path, uvTex.EncodeToPNG());
        InfoLogger.main.LogText("Successfully exported UV map.");
    }



}
