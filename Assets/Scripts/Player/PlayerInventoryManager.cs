using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance { get; private set; }

    [Header("Player 1")]
    public PlayerWeaponData player1 =
    new PlayerWeaponData();

    [Header("Player 2")]
    public PlayerWeaponData player2 =
        new PlayerWeaponData();

    [Header("Magazine")]
    public int magazineSize = 6;

    private const string HasInventoryKey = "Run_HasInventory";
    private const string MagazineSizeKey = "Run_MagazineSize";

    private const string P1CurrentAmmoKey = "Run_P1_CurrentAmmo";
    private const string P1FireAmmoKey = "Run_P1_FireAmmo";
    private const string P1WaterAmmoKey = "Run_P1_WaterAmmo";
    private const string P1CurrentElementKey = "Run_P1_CurrentElement";

    private const string P2CurrentAmmoKey = "Run_P2_CurrentAmmo";
    private const string P2FireAmmoKey = "Run_P2_FireAmmo";
    private const string P2WaterAmmoKey = "Run_P2_WaterAmmo";
    private const string P2CurrentElementKey = "Run_P2_CurrentElement";

    private void Awake()
    {
        Debug.Log($"PlayerInventoryManager Awake {GetInstanceID()}");

        if (Instance != null && Instance != this)
        {
            Debug.Log("DESTROY DUPLICATE INVENTORY");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadInventoryFromPrefs();
    }

    public void ResetInventory(bool saveToPrefs = true)
    {
        Debug.Log("RESET INVENTORY CALLED");

        magazineSize = Mathf.Max(1, magazineSize);

        player1.currentAmmo = magazineSize;
        player1.fireAmmo = 0;
        player1.waterAmmo = 0;
        player1.currentElement = Element.Physical;

        player2.currentAmmo = magazineSize;
        player2.fireAmmo = 0;
        player2.waterAmmo = 0;
        player2.currentElement = Element.Physical;

        if (saveToPrefs)
            SaveInventoryToPrefs();
    }

    public void SaveInventoryToPrefs()
    {
        PlayerPrefs.SetInt(HasInventoryKey, 1);
        PlayerPrefs.SetInt(MagazineSizeKey, Mathf.Max(1, magazineSize));

        PlayerPrefs.SetInt(P1CurrentAmmoKey, player1.currentAmmo);
        PlayerPrefs.SetInt(P1FireAmmoKey, player1.fireAmmo);
        PlayerPrefs.SetInt(P1WaterAmmoKey, player1.waterAmmo);
        PlayerPrefs.SetInt(P1CurrentElementKey, (int)player1.currentElement);

        PlayerPrefs.SetInt(P2CurrentAmmoKey, player2.currentAmmo);
        PlayerPrefs.SetInt(P2FireAmmoKey, player2.fireAmmo);
        PlayerPrefs.SetInt(P2WaterAmmoKey, player2.waterAmmo);
        PlayerPrefs.SetInt(P2CurrentElementKey, (int)player2.currentElement);

        PlayerPrefs.Save();
    }

    public void LoadInventoryFromPrefs()
    {
        if (PlayerPrefs.GetInt(HasInventoryKey, 0) != 1)
            return;

        magazineSize = Mathf.Max(1, PlayerPrefs.GetInt(MagazineSizeKey, magazineSize));

        player1.currentAmmo = PlayerPrefs.GetInt(P1CurrentAmmoKey, 6);
        player1.fireAmmo = PlayerPrefs.GetInt(P1FireAmmoKey, 0);
        player1.waterAmmo = PlayerPrefs.GetInt(P1WaterAmmoKey, 0);
        player1.currentElement = IntToElement(PlayerPrefs.GetInt(P1CurrentElementKey, 0));

        player2.currentAmmo = PlayerPrefs.GetInt(P2CurrentAmmoKey, 6);
        player2.fireAmmo = PlayerPrefs.GetInt(P2FireAmmoKey, 0);
        player2.waterAmmo = PlayerPrefs.GetInt(P2WaterAmmoKey, 0);
        player2.currentElement = IntToElement(PlayerPrefs.GetInt(P2CurrentElementKey, 0));
    }

    private Element IntToElement(int value)
    {
        if (System.Enum.IsDefined(typeof(Element), value))
            return (Element)value;

        return Element.Physical;
    }

    public static void ClearSavedInventoryPrefs()
    {
        PlayerPrefs.DeleteKey(HasInventoryKey);
        PlayerPrefs.DeleteKey(MagazineSizeKey);

        PlayerPrefs.DeleteKey(P1CurrentAmmoKey);
        PlayerPrefs.DeleteKey(P1FireAmmoKey);
        PlayerPrefs.DeleteKey(P1WaterAmmoKey);
        PlayerPrefs.DeleteKey(P1CurrentElementKey);

        PlayerPrefs.DeleteKey(P2CurrentAmmoKey);
        PlayerPrefs.DeleteKey(P2FireAmmoKey);
        PlayerPrefs.DeleteKey(P2WaterAmmoKey);
        PlayerPrefs.DeleteKey(P2CurrentElementKey);

        PlayerPrefs.Save();
    }
}