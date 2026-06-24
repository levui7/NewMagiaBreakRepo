using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public Image hpFill;
    public Image reloadFill;
    public Image elementIcon;

    public Sprite physicalSprite;
    public Sprite fireSprite;
    public Sprite waterSprite;

    public void UpdateHealth(float current, float max)
    {
        hpFill.fillAmount = current / max;
    }

    public void UpdateReload(float currentAmmo, float maxAmmo)
    {
        reloadFill.fillAmount = currentAmmo / maxAmmo;
    }

    public void UpdateElement(Element element)
    {
        switch (element)
        {
            case Element.Fire:
                elementIcon.sprite = fireSprite;
                break;

            case Element.Water:
                elementIcon.sprite = waterSprite;
                break;

            default:
                elementIcon.sprite = physicalSprite;
                break;
        }
    }
}