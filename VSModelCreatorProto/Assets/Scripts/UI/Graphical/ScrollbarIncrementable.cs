using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollbarIncrementable : MonoBehaviour
{

    public Scrollbar scrollbar;
    public Button incUpButton;
    public Button incDownButton;

    bool upClick;
    bool downClick;
    float tSinceClick = 0;
    float sensitivity = 2;

    void Start()
    {
        sensitivity = UIConfigManager.main.scrollBarIncrementerSpeed;
    }

    public void IncUpMouseDown()
    {
        //UpClicked();
        upClick = true;
        tSinceClick = 0;
    }

    public void IncDownMouseDown()
    {
        //DownClicked();
        downClick = true;
        tSinceClick = 0;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            tSinceClick += Time.deltaTime;
            if (tSinceClick > 0.5f)
            {
                if (upClick)
                {
                    scrollbar.value = Mathf.Clamp01(scrollbar.value + scrollbar.size * sensitivity * Time.deltaTime);
                }
                else if (downClick)
                {
                    scrollbar.value = Mathf.Clamp01(scrollbar.value - scrollbar.size * sensitivity * Time.deltaTime);
                }
            }
        }
        else
        {
            if (upClick && tSinceClick < 0.5f)
            {
                UpClicked();
            }
            if (downClick && tSinceClick < 0.5f)
            {
                DownClicked();
            }
            upClick = false;
            downClick = false;
        }
    }

    void UpClicked()
    {
        scrollbar.value = Mathf.Clamp01(scrollbar.value + (scrollbar.size / 2));
    }
    
    void DownClicked()
    {
        scrollbar.value = Mathf.Clamp01(scrollbar.value - (scrollbar.size / 2));
    }
}
