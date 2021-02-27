using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
    [SerializeField] Sprite[] Sprites = new Sprite[(int)INDICATOR.SIZE];
    Image IndicatorImage;

    private void Start()
    {
        IndicatorImage = GetComponent<Image>();
    }

    public enum INDICATOR
    {
        NORMAL = 0,
        DOWNED,
        COVERED,

        SIZE
    }


    public void ChangeIndicator(INDICATOR indicator)
    {
        switch(indicator)
        {
            case INDICATOR.NORMAL:
            case INDICATOR.DOWNED:
            case INDICATOR.COVERED:
                IndicatorImage.sprite = Sprites[(int)indicator];
                break;
            default:
                break;
        }
    }
}
