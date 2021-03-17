using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIGunState : MonoBehaviour
{
    [SerializeField] Image[] image;
    Transform[] transforms;

    private void Start()
    {
        transforms = new Transform[4];

        for (int i = 0; i < image.Length - 1; ++i)
        {
            transforms[i] = image[i].GetComponent<Transform>();
        }
    }

    public void OnSwapItem(int currentItemIndex)
    {
        for (int i = 0; i < image.Length - 1; ++i)
        {
            if (i == currentItemIndex)
            {
                image[i].color = new Color(1, 0.5f, 0);
                transforms[i].rotation = Quaternion.Euler(0, transforms[i].rotation.eulerAngles.y, 0);
            }
            else
            {
                image[i].color = new Color(1, 1, 1);
                transforms[i].rotation = Quaternion.Euler(0, transforms[i].rotation.eulerAngles.y, -15);
            }
        }
    }

    public void OnSpendBullet(int remainBullets, int maxBulletAmount)
    {
        image[4].fillAmount = remainBullets / (float)maxBulletAmount;
    }
}
