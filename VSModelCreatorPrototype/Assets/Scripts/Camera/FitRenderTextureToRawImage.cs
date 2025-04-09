using UnityEngine;

/// <summary>
/// We're using a render texture to draw the scene to, which gives us more control over the scene camera's positioning.
/// This will automatically change the size of the render texture to the same size as the raw image. 
/// </summary>
public class FitRenderTextureToRawImage : MonoBehaviour
{

    public RenderTexture renderTexture;

    //Note that this is also called when the rect transform is initialized.
    private void OnRectTransformDimensionsChange()
    {
        RectTransform rect = GetComponent<RectTransform>();
        
        //Removes an error caused by the rect not being created yet.
        if (rect.rect.width == 0 || rect.rect.height == 0) return;

        //RendTex sizes can't be changed while they exist. So release the texture, change the size, and recreate it.
        //Camera doesn't always update automatically - Refreshing the target texture should fix this.
        Camera.main.targetTexture = null;
        renderTexture.Release();
        renderTexture.width = (int)rect.rect.width;
        renderTexture.height = (int)rect.rect.height;
        renderTexture.Create();
        Camera.main.targetTexture = renderTexture;
    }

}
