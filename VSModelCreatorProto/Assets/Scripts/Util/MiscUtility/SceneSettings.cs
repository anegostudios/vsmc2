using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SceneSettings : MonoBehaviour
{

    public Light mainLight;
    public TMP_Text shadowSettingText;
    public TMP_Text lightingEnabledText;
    public TMP_Text texturesEnabledText;
    public TMP_Text texturesDisabledWarningText;
    public TMP_Text ssaoEnabledText;

    public Material[] sceneMaterialsForTextureControl;
    public Color lightingEnabledColor;
    public Color lightingDisabledColor;
    public UniversalRendererData rendererData;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RefreshSceneSettings();
    }

    void RefreshSceneSettings()
    {
        mainLight.shadows = (LightShadows)GetShadowMode();
        shadowSettingText.text = "Shadows: " + ((LightShadows)GetShadowMode()).ToString();

        bool lightingEnabled = GetLightingEnabled();
        mainLight.enabled = lightingEnabled;
        RenderSettings.ambientSkyColor = lightingEnabled ? lightingEnabledColor : lightingDisabledColor;
        lightingEnabledText.text = "Lighting: " + (lightingEnabled ? "Enabled" : "Disabled");

        bool texturesEnabled = GetTexturesEnabled();
        texturesDisabledWarningText.text = texturesEnabled ? "" : "Warning: Textures are currently disabled in settings!";
        texturesEnabledText.text = "Textures: " + (texturesEnabled ? "Enabled" : "Disabled");
        foreach (Material m in sceneMaterialsForTextureControl)
        {
            m.SetInt("_TexturesEnabled", texturesEnabled ? 1 : 0);
        }

        bool ssaoEnabled = GetSSAOEnabled();
        ssaoEnabledText.text = "SSAO: " + (ssaoEnabled ? "Enabled" : "Disabled");
        try
        {
            rendererData.rendererFeatures.Find(x => x is ScreenSpaceAmbientOcclusion).SetActive(ssaoEnabled);
        }
        catch
        {
            InfoLogger.main.LogText("Setting SSAO failed!");
        }
    }

    int GetShadowMode()
    {
        return ProgramPreferences.ShadowMode.GetValue();
    }

    public void SetShadowMode(int shadowMode)
    {
        ProgramPreferences.ShadowMode.SetValue(shadowMode);
        InfoLogger.main.LogText("Shadows set to: " + ((LightShadows)GetShadowMode()).ToString());
        RefreshSceneSettings();
    }

    public void IncrementShadowMode()
    {
        SetShadowMode((GetShadowMode() + 1) % 3);
    }

    bool GetLightingEnabled()
    {
        return ProgramPreferences.EnableLighting.GetValue();
    }

    public void SetLightingEnabled(bool enabled)
    {
        ProgramPreferences.EnableLighting.SetValue(enabled);
        InfoLogger.main.LogText("Lighting " + (enabled ? "enabled" : "disabled"));
        RefreshSceneSettings();
    }

    public void ToggleLightingEnabled()
    {
        SetLightingEnabled(!GetLightingEnabled());
    }

    public bool GetTexturesEnabled()
    {
        return ProgramPreferences.EnableTextures.GetValue();
    }

    public void SetTexturesEnabled(bool enabled)
    {
        ProgramPreferences.EnableTextures.SetValue(enabled);
        InfoLogger.main.LogText("Textures " + (enabled ? "enabled" : "disabled"));
        RefreshSceneSettings();
    }
    
    public void ToggleTexturesEnabled()
    {
        SetTexturesEnabled(!GetTexturesEnabled());
    }

    public bool GetSSAOEnabled()
    {
        return ProgramPreferences.EnableSSAO.GetValue();
    }

    public void SetSSAOEnabled(bool enabled)
    {
        ProgramPreferences.EnableSSAO.SetValue(enabled);
        InfoLogger.main.LogText("SSAO " + (enabled ? "enabled" : "disabled"));
        RefreshSceneSettings();
    }

    public void ToggleSSAOEnabled()
    {
        SetSSAOEnabled(!GetSSAOEnabled());
    }


}
