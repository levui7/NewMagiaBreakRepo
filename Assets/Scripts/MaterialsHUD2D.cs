using UnityEngine;
using TMPro;

public class MaterialsHUD2D : MonoBehaviour
{
    public static MaterialsHUD2D Instance { get; private set; }

    [Header("Auto Create")]
    public bool autoCreateUI = true;
    public Vector2 topLeftOffset = new Vector2(24f, -24f);
    public int fontSize = 24;

    [Header("Manual UI")]
    public TextMeshProUGUI materialsText;

    private Canvas canvas;
    private float refreshTimer;

    private void Awake()
    {
        Instance = this;

        if (autoCreateUI)
            EnsureUI();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        if (autoCreateUI)
            EnsureUI();

        Subscribe();
        Refresh();
    }

    private void Update()
    {
        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = 0.25f;
            Refresh();
        }
    }

    private void Subscribe()
    {
        if (PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.OnProgressChanged -= Refresh;
            PlayerProgressManager.Instance.OnProgressChanged += Refresh;
        }
    }

    private void Unsubscribe()
    {
        if (PlayerProgressManager.Instance != null)
            PlayerProgressManager.Instance.OnProgressChanged -= Refresh;
    }

    private void EnsureUI()
    {
        if (materialsText != null)
            return;

        canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("MaterialsHUDCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            UnityEngine.UI.CanvasScaler scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        GameObject textObject = new GameObject("MaterialsText");
        textObject.transform.SetParent(canvas.transform, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = topLeftOffset;
        rect.sizeDelta = new Vector2(850f, 110f);

        materialsText = textObject.AddComponent<TextMeshProUGUI>();
        materialsText.fontSize = fontSize;
        materialsText.color = Color.white;
        materialsText.alignment = TextAlignmentOptions.Left;
        materialsText.raycastTarget = false;
    }

    public void Refresh()
    {
        if (autoCreateUI)
            EnsureUI();

        if (materialsText == null)
            return;

        if (PlayerProgressManager.Instance == null)
        {
            materialsText.text = "Монеты: 0 | Кристаллы: 0";
            return;
        }

        materialsText.text = PlayerProgressManager.Instance.GetProgressText();
    }
}