using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VSMCUpdateChecker : MonoBehaviour
{

    public string versionFileURL = "https://raw.githubusercontent.com/anegostudios/vsmc2/refs/heads/main/VSModelCreatorProto/Assets/version.txt";

    public GameObject updateAvailable;
    public GameObject updateNotAvailable;
    public GameObject waitingForUpdateConnection;
    public GameObject failedToFindUpdateFile;
    public Toggle updateCheckToggle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ProgramPreferences.EnableUpdateChecking.GetValue())
        {
            updateCheckToggle.SetIsOnWithoutNotify(true);
            StartCoroutine(PerformUpdateChecking());
        }
    }
    
    public void OpenGitHubReleases()
    {
        Application.OpenURL("https://github.com/anegostudios/vsmc2/tags");
    }

    public  void OnEnableUpdateCheckingToggle(bool toggle)
    {
        ProgramPreferences.EnableUpdateChecking.SetValue(toggle);
        if (toggle)
        {
            StartCoroutine(PerformUpdateChecking());
        }
    }

    IEnumerator PerformUpdateChecking()
    {
        //Create and send update file request.
        waitingForUpdateConnection.SetActive(true);
        UnityWebRequest updateFile = UnityWebRequest.Get(versionFileURL);
        yield return updateFile.SendWebRequest();
        waitingForUpdateConnection.SetActive(false);
        updateNotAvailable.SetActive(true);
        try
        {
            if (updateFile.result == UnityWebRequest.Result.Success)
            {
                string updateCode = updateFile.downloadHandler.text;
                if (updateCode.Trim() != Application.version)
                {
                    //There may be an update!
                    updateAvailable.SetActive(true);
                    updateNotAvailable.SetActive(false);
                }
            }
            else
            {
                failedToFindUpdateFile.SetActive(true);
                updateNotAvailable.SetActive(false);
                Debug.Log("Failed to access version file.");
            }
        }
        catch
        {
            failedToFindUpdateFile.SetActive(true);
            updateNotAvailable.SetActive(false);
            Debug.Log("Failed to access version file.");
        }
    }
}
