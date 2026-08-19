using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using VSMC;

public class SceneSettings : MonoBehaviour
{

    public static SceneSettings main;

    public Light mainLight;
    public TMP_Text shadowSettingText;
    public GameObject lightingEnabledToggleIcon;
    public GameObject texturesEnabledToggleIcon;
    public TMP_Text texturesDisabledWarningText;
    public GameObject ssaoEnabledToggleIcon;

    public Material[] sceneMaterialsForTextureControl;
    public Color lightingEnabledColor;
    public Color lightingDisabledColor;
    public UniversalRendererData rendererData;

    public GameObject modelOpacityOverlay;
    public float modelOpacity;
    public Slider modelOpacitySlider;
    public TMP_InputField modelOpacityInput;
    public Material transparentMaterialForOpacityControl;
    public GameObject shapeHolder;
    public bool forceTransparentShader;

    public GameObject gridEnabledToggleIcon;
    public GameObject grid;
    public GameObject compassEnabledToggleIcon;
    public GameObject compass;

    void Awake()
    {
        main = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        modelOpacityInput.onEndEdit.AddListener(x =>
        {
            try
            {
                modelOpacitySlider.value = float.Parse(x);
            }
            catch { modelOpacityInput.SetTextWithoutNotify(modelOpacitySlider.value.ToString("0.00")); }
        });
        modelOpacitySlider.onValueChanged.AddListener(x =>
        {
            try
            {
                modelOpacityInput.SetTextWithoutNotify(x.ToString("0.00"));
            }
            catch { };
        });
        modelOpacitySlider.value = 1;
        ApplyModelOpacity();
        RefreshSceneSettings();
    }

    void RefreshSceneSettings()
    {
        mainLight.shadows = (LightShadows)GetShadowMode();
        shadowSettingText.text = "Shadows: " + ((LightShadows)GetShadowMode()).ToString();

        bool lightingEnabled = GetLightingEnabled();
        mainLight.enabled = lightingEnabled;
        RenderSettings.ambientSkyColor = lightingEnabled ? lightingEnabledColor : lightingDisabledColor;
        lightingEnabledToggleIcon.SetActive(lightingEnabled);

        bool texturesEnabled = GetTexturesEnabled();
        texturesDisabledWarningText.text = texturesEnabled ? "" : "Warning: Textures are currently disabled in settings!";
        texturesEnabledToggleIcon.SetActive(texturesEnabled);
        foreach (Material m in sceneMaterialsForTextureControl)
        {
            m.SetInt("_TexturesEnabled", texturesEnabled ? 1 : 0);
        }

        bool ssaoEnabled = GetSSAOEnabled();
        ssaoEnabledToggleIcon.SetActive(ssaoEnabled);
        try
        {
            rendererData.rendererFeatures.Find(x => x is ScreenSpaceAmbientOcclusion).SetActive(ssaoEnabled);
        }
        catch
        {
            InfoLogger.main.LogText("Setting SSAO failed!");
        }
        gridEnabledToggleIcon.SetActive(grid.activeSelf);
        compassEnabledToggleIcon.SetActive(compass.activeSelf);
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

    public void OpenModelOpacityOverlay()
    {
        modelOpacityOverlay.SetActive(true);
        modelOpacitySlider.SetValueWithoutNotify(modelOpacity);
        modelOpacityInput.SetTextWithoutNotify(modelOpacity.ToString("0.00"));
    }

    public void OnModelOpacityScroll(BaseEventData e)
    {
        if (Input.mouseScrollDelta.y > Mathf.Epsilon) { modelOpacitySlider.value += Input.GetKey(KeyCode.LeftShift) ? 0.01f : 0.1f; }
        else if (Input.mouseScrollDelta.y < -Mathf.Epsilon) { modelOpacitySlider.value -= Input.GetKey(KeyCode.LeftShift) ? 0.01f : 0.1f; }
    }

    public void ApplyModelOpacity()
    {
        modelOpacity = modelOpacitySlider.value;
        transparentMaterialForOpacityControl.SetFloat("_OpacityMultiplier", modelOpacity);

        //Refresh all game objects in the shape holder - This will also process deleted elements.
        foreach (ShapeElementGameObject go in shapeHolder.GetComponentsInChildren<ShapeElementGameObject>(true))
        {
            go.RefreshMaterialChoice();
        }
    }

    public void ToggleGridVisibility()
    {
        grid.SetActive(!grid.activeSelf);
        gridEnabledToggleIcon.SetActive(grid.activeSelf);
    }
    
    public void ToggleCompassVisibility()
    {
        compass.SetActive(!compass.activeSelf);
        compassEnabledToggleIcon.SetActive(compass.activeSelf);
    }

}
