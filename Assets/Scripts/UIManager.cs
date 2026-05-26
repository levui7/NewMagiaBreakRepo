using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Совместимость со старыми скриптами, где используется UIManager.instance
    public static UIManager instance => Instance;

    [Header("Auto UI")]
    public bool autoCreateMissingUI = true;
    public bool showPlayer2UI = false;
    public Vector2 player1BottomLeftOffset = new Vector2(24f, 24f);
    public Vector2 player2BottomLeftOffset = new Vector2(24f, 140f);
    public int hudFontSize = 24;

    [Header("Player 1 UI")]
    public TextMeshProUGUI healthText1;
    public TextMeshProUGUI weaponText1;
    public TextMeshProUGUI statusText1;

    [Header("Player 2 UI")]
    public TextMeshProUGUI healthText2;
    public TextMeshProUGUI weaponText2;
    public TextMeshProUGUI statusText2;

    private Canvas runtimeCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (autoCreateMissingUI)
            EnsureHUDExists();
    }

    private void Start()
    {
        if (autoCreateMissingUI)
            EnsureHUDExists();
    }

    private void EnsureHUDExists()
    {
        if (healthText1 != null && weaponText1 != null && statusText1 != null)
            return;

        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("RuntimeHUDCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        runtimeCanvas = canvas;

        if (healthText1 == null || weaponText1 == null || statusText1 == null)
            CreatePlayerHUD(1, player1BottomLeftOffset, out healthText1, out weaponText1, out statusText1);

        if (showPlayer2UI && (healthText2 == null || weaponText2 == null || statusText2 == null))
            CreatePlayerHUD(2, player2BottomLeftOffset, out healthText2, out weaponText2, out statusText2);
    }

    private void CreatePlayerHUD(
        int playerNumber,
        Vector2 bottomLeftOffset,
        out TextMeshProUGUI healthText,
        out TextMeshProUGUI weaponText,
        out TextMeshProUGUI statusText)
    {
        GameObject panelObject = new GameObject("HUD_Player" + playerNumber + "_BottomLeft");
        panelObject.transform.SetParent(runtimeCanvas.transform, false);

        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.anchoredPosition = bottomLeftOffset;
        panelRect.sizeDelta = new Vector2(760f, 130f);

        healthText = CreateText("HealthText_P" + playerNumber, panelRect, new Vector2(0f, 86f));
        weaponText = CreateText("WeaponText_P" + playerNumber, panelRect, new Vector2(0f, 43f));
        statusText = CreateText("StatusText_P" + playerNumber, panelRect, new Vector2(0f, 0f));

        healthText.text = "Игрок " + playerNumber + " HP: --";
        weaponText.text = "Оружие: --";
        statusText.text = "Статусы: Нет";
    }

    private TextMeshProUGUI CreateText(string objectName, RectTransform parent, Vector2 anchoredPosition)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(760f, 38f);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = hudFontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        text.raycastTarget = false;

        return text;
    }

    public void UpdatePlayerHUD(PlayerController player, WeaponManager weapon)
    {
        if (player == null)
            return;

        if (autoCreateMissingUI)
            EnsureHUDExists();

        TextMeshProUGUI healthText;
        TextMeshProUGUI weaponText;
        TextMeshProUGUI statusText;

        if (player.playerID == 2)
        {
            healthText = healthText2;
            weaponText = weaponText2;
            statusText = statusText2;
        }
        else
        {
            healthText = healthText1;
            weaponText = weaponText1;
            statusText = statusText1;
        }

        if (healthText != null)
        {
            healthText.text = $"Игрок {player.playerID} HP: {player.GetCurrentHealth()}/{player.GetMaxHealth()}";
        }

        if (weaponText != null && weapon != null)
        {
            weaponText.text = $"Оружие: {weapon.GetElementNameRu()} | {weapon.GetAmmoText()}";
        }

        if (statusText != null)
        {
            StatusEffectController status = player.GetComponent<StatusEffectController>();

            if (status != null)
                statusText.text = "Статусы: " + status.ToString();
            else
                statusText.text = "Статусы: Нет";
        }
    }

    public void UpdateStatus(PlayerController player)
    {
        if (player == null)
            return;

        if (autoCreateMissingUI)
            EnsureHUDExists();

        TextMeshProUGUI statusText;

        if (player.playerID == 2)
            statusText = statusText2;
        else
            statusText = statusText1;

        if (statusText == null)
            return;

        StatusEffectController status = player.GetComponent<StatusEffectController>();

        if (status != null)
            statusText.text = "Статусы: " + status.ToString();
        else
            statusText.text = "Статусы: Нет";
    }

    public void UpdateStatus(int playerID)
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            if (player != null && player.playerID == playerID)
            {
                UpdateStatus(player);
                return;
            }
        }
    }

    public void UpdateStatus(StatusEffectController status)
    {
        if (status == null)
            return;

        PlayerController player = status.GetComponent<PlayerController>();

        if (player != null)
            UpdateStatus(player);
    }

    public void UpdateStatus(PlayerController player, string statusTextValue)
    {
        if (player == null)
            return;

        if (autoCreateMissingUI)
            EnsureHUDExists();

        TextMeshProUGUI targetStatusText = player.playerID == 2 ? statusText2 : statusText1;

        if (targetStatusText != null)
            targetStatusText.text = "Статусы: " + statusTextValue;
    }

    public void UpdateStatus(int playerID, string statusTextValue)
    {
        if (autoCreateMissingUI)
            EnsureHUDExists();

        TextMeshProUGUI targetStatusText = playerID == 2 ? statusText2 : statusText1;

        if (targetStatusText != null)
            targetStatusText.text = "Статусы: " + statusTextValue;
    }

    public void UpdateStatus(StatusEffectController statusController, string statusTextValue)
    {
        if (statusController == null)
            return;

        PlayerController player = statusController.GetComponent<PlayerController>();

        if (player != null)
            UpdateStatus(player, statusTextValue);
    }

    public void UpdateStatus(PlayerController player, StatusEffectController statusController)
    {
        if (player == null)
            return;

        if (statusController != null)
            UpdateStatus(player, statusController.ToString());
        else
            UpdateStatus(player, "Нет");
    }

    public void UpdateStatus(int playerID, StatusEffectController statusController)
    {
        if (statusController != null)
            UpdateStatus(playerID, statusController.ToString());
        else
            UpdateStatus(playerID, "Нет");
    }
}