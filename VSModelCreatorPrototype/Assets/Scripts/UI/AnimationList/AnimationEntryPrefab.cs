using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class AnimationEntryPrefab : MonoBehaviour
{
    static bool AlternateColor = false;
    public string animID;
    public TMP_Text animationName;
    
    public bool isPlaying = false;
    public TMP_Text animationPlaying;

    private ShapeTester shapeTester;

    public void InitializePrefab(string animName, string animID, ShapeTester shapeTester)
    {
        Color c = GetComponent<Image>().color;
        GetComponent<Image>().color = new Color(c.r, c.g, c.b, AlternateColor ? 0.15f : 0.25f);
        AlternateColor = !AlternateColor;
        animationPlaying.text = "Play";
        animationName.text = animName;
        this.animID = animID;
        this.shapeTester = shapeTester;
    }

    public void OnPlayPausePressed()
    {
        isPlaying = !isPlaying;
        shapeTester.SetAnimationPlaying(animID, isPlaying);
        animationPlaying.text = isPlaying ? "Pause" : "Play";
    }

}
