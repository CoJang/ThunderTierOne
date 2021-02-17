using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public string MenuName;
    [HideInInspector] public bool IsOpened;

    public void Open()
    {
        IsOpened = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        IsOpened = false;
        gameObject.SetActive(false);
    }
}
