using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponCell : MonoBehaviour, IPointerClickHandler
{
    public int cellIndex = 1;

    [Header("Visual")]
    public Image backgroundImage;
    public Color normalColor = new Color(0.745f, 0.72f, 0.72f, 1f);
    public Color equippedColor = new Color(0.62f, 0.62f, 0.62f, 1f);
    public Color notOwnedColor = new Color(0.745f, 0.72f, 0.72f, 1f);
    public float equippedScale = 1.08f;

    private Vector3 initialScale;
    private bool initialized;

    void Awake()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        initialScale = transform.localScale;
        initialized = true;
    }

    void Start()
    {
        RefreshVisual();
    }

    void Update()
    {
        RefreshVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance == null) return;

        if (cellIndex == 1)
        {
            GameManager.Instance.EquipFists();
            RefreshVisual();
        }
        else if (cellIndex == 2)
        {
            if (!GameManager.Instance.IsSwordOwned())
            {
                var shop = FindObjectOfType<SaberShop>();
                if (shop != null) shop.ShowNotOwnedMessage();
                return;
            }
            GameManager.Instance.EquipCell2Sword();
            RefreshVisual();
        }
    }

    public void RefreshVisual()
    {
        if (!initialized)
        {
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            initialScale = transform.localScale;
            initialized = true;
        }
        if (backgroundImage == null) return;
        if (GameManager.Instance == null)
        {
            backgroundImage.color = normalColor;
            return;
        }

        bool isEquipped = false;
        bool isOwned = true;

        if (cellIndex == 1)
        {
            isOwned = true;
            isEquipped = !GameManager.Instance.IsSwordEquipped();
        }
        else if (cellIndex == 2)
        {
            isOwned = GameManager.Instance.IsSwordOwned();
            isEquipped = GameManager.Instance.IsSwordEquipped();
        }

        if (!isOwned)
        {
            backgroundImage.color = notOwnedColor;
            transform.localScale = initialScale;
        }
        else if (isEquipped)
        {
            backgroundImage.color = equippedColor;
            if (cellIndex == 1)
                transform.localScale = initialScale;
            else
                transform.localScale = initialScale * equippedScale;
        }
        else
        {
            backgroundImage.color = normalColor;
            transform.localScale = initialScale;
        }
    }
}
