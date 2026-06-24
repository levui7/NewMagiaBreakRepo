using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image hpFill;

    public void SetHealth(float current, float max)
    {
        hpFill.fillAmount = current / max;
    }
}