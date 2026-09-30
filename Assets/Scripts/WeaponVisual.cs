using UnityEngine;

public class WeaponVisual : MonoBehaviour
{
    [Header("Arm swap")]
    [Tooltip("Рука (W1) меняет текстуру на руку-с-оружием")]
    public bool useArmSwap = true;
    public Sprite armSaberSprite;
    public Sprite armKatanaSprite;
    public SpriteRenderer armRenderer;

    private Sprite armDefaultSprite;
    private bool armSwapReady;

    void Start()
    {
        InitArmSwap();
    }

    private void InitArmSwap()
    {
        if (armRenderer == null)
        {
            // скрипт висит на W1 — его же renderer и есть рука с оружием
            armRenderer = GetComponent<SpriteRenderer>();
            if (armRenderer == null)
            {
                GameObject w1 = GameObject.Find("W1");
                if (w1 != null)
                    armRenderer = w1.GetComponent<SpriteRenderer>();
            }
        }

        if (armRenderer != null)
            armDefaultSprite = armRenderer.sprite;

        armSwapReady = useArmSwap && armRenderer != null && armSaberSprite != null && armKatanaSprite != null;
    }

    void Update()
    {
        if (!armSwapReady || armRenderer == null)
            return;

        GameManager gm = GameManager.Instance;
        bool saber = gm != null && gm.saberEquipped;
        bool katana = gm != null && gm.katanaEquipped;

        Sprite want = katana ? armKatanaSprite : (saber ? armSaberSprite : armDefaultSprite);
        if (want != null && armRenderer.sprite != want)
            armRenderer.sprite = want;
    }
}
