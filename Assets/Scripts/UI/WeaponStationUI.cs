using TMPro;
using UnityEngine;

public class WeaponStationUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Dropdowns")]
    public TMP_Dropdown attackDropdown;
    public TMP_Dropdown elementDropdown;

    [Header("Texts")]
    public TextMeshProUGUI resultText;

    [Header("Debug")]
    public bool logDebug = true;

    private void Start()
    {
        PrepareDropdowns();
        RefreshFromConfig();

        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        if (WeaponStation.Instance == null)
            return;

        if (WeaponStation.Instance.playerInside && Input.GetKeyDown(KeyCode.E))
            Open();
    }

    public void Open()
    {
        PrepareDropdowns();
        RefreshFromConfig();

        if (panel != null)
            panel.SetActive(true);

        if (resultText != null)
            resultText.text = "Выберите режим атаки и стартовую стихию.";
    }

    public void ApplySettings()
    {
        if (WeaponConfigManager.Instance == null)
        {
            if (resultText != null)
                resultText.text = "Ошибка: WeaponConfigManager не найден.";

            Debug.LogError("WeaponStationUI: WeaponConfigManager.Instance == null");
            return;
        }

        PrepareDropdowns();

        AttackMode selectedMode = GetSelectedAttackMode();
        Element selectedElement = GetSelectedElement();

        if (logDebug)
        {
            Debug.Log(
                $"WeaponStationUI ApplySettings: " +
                $"DropdownAttackValue={attackDropdown?.value}, " +
                $"SelectedMode={selectedMode}, " +
                $"DropdownElementValue={elementDropdown?.value}, " +
                $"SelectedElement={selectedElement}");
        }

        WeaponConfigManager.Instance.SetAttackMode(selectedMode);
        WeaponConfigManager.Instance.SetInitialElement(selectedElement);

        WeaponConfigManager.Instance.ApplyConfigToCurrentWeapons();

        if (resultText != null)
        {
            resultText.text =
                $"Принято: атака {WeaponConfigManager.Instance.GetAttackModeNameRu()}, " +
                $"стихия {WeaponConfigManager.Instance.GetInitialElementNameRu()}";
        }

        if (panel != null)
            panel.SetActive(false);
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private AttackMode GetSelectedAttackMode()
    {
        if (attackDropdown == null)
            return AttackMode.Single;

        // 0 — одиночный, 1 — по площади.
        return attackDropdown.value == 1 ? AttackMode.Area : AttackMode.Single;
    }

    private Element GetSelectedElement()
    {
        if (elementDropdown == null)
            return Element.Fire;

        // 0 — огонь, 1 — вода.
        return elementDropdown.value == 1 ? Element.Water : Element.Fire;
    }

    private void PrepareDropdowns()
    {
        if (attackDropdown != null)
        {
            attackDropdown.options.Clear();
            attackDropdown.options.Add(new TMP_Dropdown.OptionData("Одиночный выстрел"));
            attackDropdown.options.Add(new TMP_Dropdown.OptionData("Выстрел по площади"));
            attackDropdown.RefreshShownValue();
        }

        if (elementDropdown != null)
        {
            elementDropdown.options.Clear();
            elementDropdown.options.Add(new TMP_Dropdown.OptionData("Огонь"));
            elementDropdown.options.Add(new TMP_Dropdown.OptionData("Вода"));
            elementDropdown.RefreshShownValue();
        }
    }

    private void RefreshFromConfig()
    {
        if (WeaponConfigManager.Instance == null)
            return;

        if (attackDropdown != null)
        {
            attackDropdown.value =
                WeaponConfigManager.Instance.selectedAttackMode == AttackMode.Area ? 1 : 0;

            attackDropdown.RefreshShownValue();
        }

        if (elementDropdown != null)
        {
            elementDropdown.value =
                WeaponConfigManager.Instance.selectedInitialElement == Element.Water ? 1 : 0;

            elementDropdown.RefreshShownValue();
        }
    }
}