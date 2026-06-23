using TMPro;
using UnityEngine;

public class WeaponStationUI : MonoBehaviour
{
    public GameObject panel;

    public TMP_Dropdown attackDropdown;

    private void Update()
    {
        if (WeaponStation.Instance.playerInside &&
            Input.GetKeyDown(KeyCode.E))
        {
            panel.SetActive(true);
        }
    }

    public void ApplySettings()
    {
        WeaponConfigManager.Instance.selectedAttackMode =
            (AttackMode)attackDropdown.value;

        panel.SetActive(false);
    }

    public void Close()
    {
        panel.SetActive(false);
    }
}