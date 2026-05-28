using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class UpgradeStation2D : MonoBehaviour
{
    [Header("UI")]
    public bool autoCreatePrompt = true;
    public TextMeshProUGUI promptText;

    [Header("Prompt")]
    public int fontSize = 24;
    public Vector2 bottomCenterOffset = new Vector2(0f, 150f);

    private PlayerController currentPlayer;
    private Canvas canvas;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (autoCreatePrompt)
            EnsurePromptUI();

        HidePrompt();
    }

    private void Update()
    {
        if (currentPlayer == null)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (PlayerProgressManager.Instance == null)
            return;

        bool upgraded = false;

        if (keyboard.fKey.wasPressedThisFrame)
            upgraded = PlayerProgressManager.Instance.UpgradeDamage();

        if (keyboard.gKey.wasPressedThisFrame)
            upgraded = PlayerProgressManager.Instance.UpgradeHealth();

        if (keyboard.hKey.wasPressedThisFrame)
            upgraded = PlayerProgressManager.Instance.UpgradeSpeed();

        if (upgraded)
        {
            PlayerProgressManager.Instance.ApplyUpgradesToPlayer(currentPlayer);

            if (MaterialsHUD2D.Instance != null)
                MaterialsHUD2D.Instance.Refresh();
        }

        RefreshPrompt();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        currentPlayer = player;

        ShowPrompt();
        RefreshPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        if (player == currentPlayer)
        {
            currentPlayer = null;
            HidePrompt();
        }
    }

    private void EnsurePromptUI()
    {
        if (promptText != null)
            return;

        canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("UpgradeStationCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            UnityEngine.UI.CanvasScaler scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        GameObject textObject = new GameObject("UpgradePromptText");
        textObject.transform.SetParent(canvas.transform, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = bottomCenterOffset;
        rect.sizeDelta = new Vector2(900f, 220f);

        promptText = textObject.AddComponent<TextMeshProUGUI>();
        promptText.fontSize = fontSize;
        promptText.color = Color.white;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.raycastTarget = false;
    }

    private void ShowPrompt()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(true);
    }

    private void HidePrompt()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    private void RefreshPrompt()
    {
        if (promptText == null)
            return;

        if (PlayerProgressManager.Instance == null)
        {
            promptText.text = "Станция прокачки";
            return;
        }

        promptText.text =
            "Станция прокачки\n" +
            PlayerProgressManager.Instance.GetProgressText() +
            "\n\n" +
            PlayerProgressManager.Instance.GetUpgradeHelpText();
    }
}