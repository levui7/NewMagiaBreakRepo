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
            resultText.text = "Выберите режим атаки и стартовую стихию";
    }

    public void ApplySettings()
    {
        if (WeaponConfigManager.Instance == null)
        {
            if (resultText != null)
                resultText.text = "Ошибка: WeaponConfigManager не найден";

            return;
        }

        AttackMode attackMode = attackDropdown != null && attackDropdown.value == 1
            ? AttackMode.Area
            : AttackMode.Single;

        Element startElement = elementDropdown != null && elementDropdown.value == 1
            ? Element.Water
            : Element.Fire;

        WeaponConfigManager.Instance.SetAttackMode(attackMode);
        WeaponConfigManager.Instance.SetInitialElement(startElement);
        WeaponConfigManager.Instance.ApplyConfigToCurrentWeapons();

        if (resultText != null)
        {
            resultText.text =
                $"Выбрано: {WeaponConfigManager.Instance.GetAttackModeNameRu()}, " +
                $"стихия: {WeaponConfigManager.Instance.GetInitialElementNameRu()}";
        }

        if (panel != null)
            panel.SetActive(false);
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void PrepareDropdowns()
    {
        if (attackDropdown != null && attackDropdown.options.Count == 0)
        {
            attackDropdown.options.Clear();
            attackDropdown.options.Add(new TMP_Dropdown.OptionData("Одиночный выстрел"));
            attackDropdown.options.Add(new TMP_Dropdown.OptionData("Выстрел по площади"));
        }

        if (elementDropdown != null && elementDropdown.options.Count == 0)
        {
            elementDropdown.options.Clear();
            elementDropdown.options.Add(new TMP_Dropdown.OptionData("Огонь"));
            elementDropdown.options.Add(new TMP_Dropdown.OptionData("Вода"));
        }
    }

    private void RefreshFromConfig()
    {
        if (WeaponConfigManager.Instance == null)
            return;

        if (attackDropdown != null)
        {
            attackDropdown.value = WeaponConfigManager.Instance.selectedAttackMode == AttackMode.Area ? 1 : 0;
            attackDropdown.RefreshShownValue();
        }

        if (elementDropdown != null)
        {
            elementDropdown.value = WeaponConfigManager.Instance.selectedInitialElement == Element.Water ? 1 : 0;
            elementDropdown.RefreshShownValue();
        }
    }
}
