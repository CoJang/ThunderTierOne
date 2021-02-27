using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reticle : MonoBehaviour
{
    public float reticleSize = 120;

    RectTransform reticle;
    Image[] lines = new Image[4];

    float minSize = 120;
    float maxSize = 400;
    float decreaseSpeed = 50.0f;
    float increaseSpeed = 100.0f;
    Color32 lineColor = Color.white;

    private void Start()
    {
        reticleSize = minSize;

        reticle = GetComponent<RectTransform>();
        lines = GetComponentsInChildren<Image>();
    }

    private void Update()
    {
        
    }

    public void SetReticleColor(Color32 color)
    {
        if(!color.Equals(lineColor))
        {
            lineColor = color;
            OnChangeColor();
        }
    }

    public void SetReticleSize(float size)
    {
        if(reticleSize != size)
        {
            Mathf.Clamp(size, minSize, maxSize);

            reticleSize = size;
            OnChangeSize();
        }
    }

    void OnChangeSize()
    {
        float newSize = Mathf.Lerp(reticle.sizeDelta.x, reticleSize, Time.deltaTime * decreaseSpeed);
        reticle.sizeDelta = new Vector2(newSize, newSize);
    }

    void OnChangeColor()
    {
        foreach(Image reticles in lines)
        {
            reticles.color = lineColor;
        }
    }
}
