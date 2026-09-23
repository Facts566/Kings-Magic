using UnityEngine;

public class WeaponVisual : MonoBehaviour
{
    public Sprite saberSprite;
    public Sprite katanaSprite;
    public Vector2 handOffset = new Vector2(1.8f, -0.5f);
    public float handScale = 15f;
    public float handRotation = 30f;
    public int sortingOrder = 1000;

    private const string DefaultSaberPath = "Sprites/Weapon2";
    private const string DefaultKatanaPath = "Sprites/Katana";

    private const float HandX = 1.8f;
    private const float HandY = -0.5f;
    private const float HandYKatana = 0.4f;
    private const float HandZ = 0f;
    private const float RotX = 0f;
    private const float RotY = 0f;
    private const float RotZ = 30f;
    private const float Scale = 15f;

    private SpriteRenderer saberRenderer;

    void Start()
    {
        CreateSaber();
    }

    void Update()
    {
        if (saberRenderer == null)
            return;

        GameManager gm = GameManager.Instance;
        bool show = gm != null && (gm.saberEquipped || gm.katanaEquipped);
        saberRenderer.enabled = show;

        if (!show)
            return;

        Sprite s;
        Vector3 pos;
        if (gm.katanaEquipped)
        {
            s = katanaSprite != null ? katanaSprite : Resources.Load<Sprite>(DefaultKatanaPath);
            pos = new Vector3(HandX, HandYKatana, HandZ);
        }
        else
        {
            s = saberSprite != null ? saberSprite : Resources.Load<Sprite>(DefaultSaberPath);
            pos = new Vector3(HandX, HandY, HandZ);
        }

        if (saberRenderer.transform.localPosition != pos)
            saberRenderer.transform.localPosition = pos;

        if (s != null && saberRenderer.sprite != s)
            saberRenderer.sprite = s;
    }

    private void CreateSaber()
    {
        GameObject saber = new GameObject("Saber");
        saber.transform.SetParent(transform, false);
        saber.transform.localPosition = new Vector3(HandX, HandY, HandZ);
        saber.transform.localRotation = Quaternion.Euler(RotX, RotY, RotZ);
        saber.transform.localScale = new Vector3(Scale, Scale, Scale);

        saberRenderer = saber.AddComponent<SpriteRenderer>();
        saberRenderer.sprite = saberSprite != null ? saberSprite : Resources.Load<Sprite>(DefaultSaberPath);
        saberRenderer.sortingOrder = sortingOrder;
        saberRenderer.enabled = false;
    }
}