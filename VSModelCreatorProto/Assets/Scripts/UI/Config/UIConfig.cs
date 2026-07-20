using UnityEngine;

public abstract class UIConfig : MonoBehaviour
{

    public void Start()
    {
        RefreshUIFromConfig(UIConfigManager.main);
    }

    public abstract void RefreshUIFromConfig(UIConfigManager config);

}
