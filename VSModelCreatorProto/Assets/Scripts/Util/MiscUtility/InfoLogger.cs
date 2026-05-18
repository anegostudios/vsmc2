using TMPro;
using UnityEngine;

public class InfoLogger : MonoBehaviour
{
    public static InfoLogger main;
    public TMP_Text text;

    void Awake()
    {
        main = this;
        LogText("You are using version " + Application.version + " of VSMC2. Check the wiki page for instructions.");
    }

    public void LogText(string textToLog)
    {
        Debug.Log(textToLog);
        text.text = "["+ System.DateTime.Now.ToLongTimeString() + "]  " + textToLog;
    }

}
