using UnityEngine;
using UnityEngine.UI;

public class SaberShop : MonoBehaviour
{
    public int price = 500;
    public int katanaPrice = 1000;
    public Image equippedLayerImage;
    public Sprite saberSprite;
    public Sprite katanaSprite;
    public Text feedbackText;

    public const string OwnedKey = "saberOwned";
    public const string KatanaOwnedKey = "katanaOwned";
    private const string DefaultSaberPath = "Sprites/Weapon2";
    private const string DefaultKatanaPath = "Sprites/Katana";

    private Sprite defaultSprite;

    void Awake()
    {
        if (equippedLayerImage != null)
            defaultSprite = equippedLayerImage.sprite;
    }

    private Sprite GetSaberSprite()
    {
        if (saberSprite != null)
            return saberSprite;

        return Resources.Load<Sprite>(DefaultSaberPath);
    }

    private Sprite GetKatanaSprite()
    {
        if (katanaSprite != null)
            return katanaSprite;

        return Resources.Load<Sprite>(DefaultKatanaPath);
    }

    void Start()
    {
        Debug.Log("[SaberShop] Start. Owned = " + IsOwned() + ", Image = " + (equippedLayerImage != null ? equippedLayerImage.name : "null"));

        RefreshEquipVisual();
    }

    public void BuySaber()
    {
        Debug.Log("[SaberShop] BuySaber. Owned = " + IsOwned() + ", Coins = " + (GameManager.Instance != null ? GameManager.Instance.coins : -1));

        if (IsOwned())
        {
            // уже куплено - экипируем
            EquipSaber();
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.TrySpendCoins(price))
        {
            ShowMessage("Недостаточно монет");
            return;
        }

        PlayerPrefs.SetInt(OwnedKey, 1);
        PlayerPrefs.Save();

        RefreshEquipVisual();
        ShowMessage("Сабля куплена! Нажми на ячейку 2 чтобы экипировать");
    }

    public void EquipSaber()
    {
        Debug.Log("[SaberShop] EquipSaber. Owned = " + IsOwned() + ", Image = " + (equippedLayerImage != null ? equippedLayerImage.name : "null"));

        if (!IsOwned())
        {
            ShowNotOwnedMessage();
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.EquipSaber();

        RefreshEquipVisual();
        ShowMessage("Экипировано: Меч");
    }

    private void RefreshEquipVisual()
    {
        if (equippedLayerImage == null)
        {
            Debug.LogWarning("[SaberShop] equippedLayerImage не назначен!");
            return;
        }

        equippedLayerImage.gameObject.SetActive(true);

        bool saberEq = GameManager.Instance != null && GameManager.Instance.saberEquipped;
        bool katanaEq = GameManager.Instance != null && GameManager.Instance.katanaEquipped;

        if (katanaEq)
        {
            Sprite s = GetKatanaSprite();
            if (s != null)
                equippedLayerImage.sprite = s;
            else
                Debug.LogWarning("[SaberShop] Не удалось загрузить спрайт катаны!");
            equippedLayerImage.color = Color.white;
        }
        else if (saberEq)
        {
            Sprite s = GetSaberSprite();
            if (s != null)
                equippedLayerImage.sprite = s;
            else
                Debug.LogWarning("[SaberShop] Не удалось загрузить спрайт сабли! Положи Weapon2.png в Assets/Resources/Sprites или назначь в поле Saber Sprite.");
            equippedLayerImage.color = Color.white;
        }
        else if (IsKatanaOwned())
        {
            Sprite s = GetKatanaSprite();
            if (s != null)
                equippedLayerImage.sprite = s;
            equippedLayerImage.color = new Color(1f, 1f, 1f, 0.5f);
        }
        else if (IsOwned())
        {
            Sprite s = GetSaberSprite();
            if (s != null)
                equippedLayerImage.sprite = s;
            equippedLayerImage.color = new Color(1f, 1f, 1f, 0.5f);
        }
        else
        {
            equippedLayerImage.sprite = defaultSprite;
            equippedLayerImage.color = Color.white;
        }

        // обновляем подсветку ячеек
        foreach (var cell in FindObjectsOfType<WeaponCell>())
            cell.RefreshVisual();
    }

    public bool IsOwned()
    {
        return PlayerPrefs.GetInt(OwnedKey, 0) == 1;
    }

    public bool IsKatanaOwned()
    {
        return PlayerPrefs.GetInt(KatanaOwnedKey, 0) == 1;
    }

    public void BuyKatana()
    {
        Debug.Log("[SaberShop] BuyKatana. Owned = " + IsKatanaOwned() + ", Coins = " + (GameManager.Instance != null ? GameManager.Instance.coins : -1));

        if (IsKatanaOwned())
        {
            EquipKatana();
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.TrySpendCoins(katanaPrice))
        {
            ShowMessage("Недостаточно монет");
            return;
        }

        PlayerPrefs.SetInt(KatanaOwnedKey, 1);
        PlayerPrefs.Save();

        RefreshEquipVisual();
        ShowMessage("Катана куплена! Нажми на ячейку 2 чтобы экипировать");
    }

    public void EquipKatana()
    {
        Debug.Log("[SaberShop] EquipKatana. Owned = " + IsKatanaOwned());

        if (!IsKatanaOwned())
        {
            ShowNotOwnedMessage();
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.EquipKatana();

        RefreshEquipVisual();
        ShowMessage("Экипировано: Катана");
    }

    public void ResetSaber()
    {
        RefreshEquipVisual();
    }

    public void ResetKatana()
    {
        RefreshEquipVisual();
    }

    public void ShowNotOwnedMessage()
    {
        ShowMessage("Сначала купи оружие в магазине");
    }

    private void ShowMessage(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }
}
