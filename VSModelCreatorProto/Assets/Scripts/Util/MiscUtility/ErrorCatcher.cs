using TMPro;
using UnityEngine;

namespace VSMC
{
    public class ErrorCatcher : MonoBehaviour
    {
        [Header("Unity References")]
        public GameObject errorOverlay;
        public TMP_Text logText;
        public bool showWarningsToUserToo;

        private void Awake()
        {
            Application.logMessageReceived += OnLogReceived;
        }

        void OnLogReceived(string condition, string stackTrace, LogType type)
        {
            //Replace my home path in the log.
            stackTrace = stackTrace.Replace("/home/nat/Documents/GitHub/vsmc2/VSModelCreatorProto", "vsmc2");
            //Only show exceptions, errors, and potentially warnings to the user.
            if (type == LogType.Exception || type == LogType.Error || (type == LogType.Warning && showWarningsToUserToo))
            {
                OpenWithLog(condition, stackTrace);
            }
        }

        public void OpenWithLog(string condition, string stackTrace)
        {
            if (errorOverlay.activeSelf)
            {
                logText.text += "\n\n"+condition+"\n"+stackTrace;
            }
            else
            {
                logText.text = condition + "\n"+stackTrace;
                errorOverlay.SetActive(true);
            }
        }

        public void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = logText.text;
        }
    }
}
