using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Совместимость со старыми скриптами, где используется UIManager.instance
    public static UIManager instance => Instance;

    [Header("Resources HUD")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI crystalText;

    public PlayerHUD player1HUD;
    public PlayerHUD player2HUD;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Invoke(nameof(RefreshResourceHUD), 0.1f);
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(RefreshResourceHUD), 0.2f, 0.5f);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(RefreshResourceHUD));
    }

    public void UpdatePlayerHUD(PlayerController player, WeaponManager weapon)
    {
        if (player == null || weapon == null)
            return;

        PlayerHUD hud = player.playerID == 1 ? player1HUD : player2HUD;

        if (hud == null)
            return;

        // Полоса здоровья
        hud.UpdateHealth(player.GetCurrentHealthFloat(), player.GetMaxHealthFloat());

        // Полоса патронов
        hud.UpdateReload(weapon.CurrentAmmo, weapon.MagazineSize);

        // Иконка стихии
        hud.UpdateElement(weapon.CurrentElement);
    }

    public void SetPlayersCount(int count)
    {
        if (player1HUD != null)
            player1HUD.gameObject.SetActive(true);

        if (player2HUD != null)
            player2HUD.gameObject.SetActive(count >= 2);
    }

    public void UpdateMaterialsHUD(int coins, int crystals)
    {
        if (coinText != null)
            coinText.text = $"Монеты: {coins}";

        if (crystalText != null)
            crystalText.text = $"Кристаллы: {crystals}";
    }

    public void RefreshResourceHUD()
    {
        if (PlayerProgressManager.Instance == null)
            return;

        UpdateMaterialsHUD(
            PlayerProgressManager.Instance.coins,
            PlayerProgressManager.Instance.crystals
        );
    }
}