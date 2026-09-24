using UnityEngine;

public class WeaponVisual : MonoBehaviour
{
    public Sprite saberSprite;
    public Sprite katanaSprite;
    public Vector2 handOffset = new Vector2(0.33f, 0.07f);
    public float handScale = 1.5f;
    public float handRotation = 30f;
    public int sortingOrder = 1000;

    private const string DefaultSaberPath = "Sprites/Weapon2";
    private const string DefaultKatanaPath = "Sprites/Katana";

    private const float HandX = 0.33f;
    private const float HandY = 0.07f;
    private const float HandXKatana = 0.35f;
    private const float HandYKatana = 0.18f;
    private const float SaberScale = 1.32f;
    private const float HandZ = 0f;
    private const float RotX = 0f;
    private const float RotY = 0f;
    private const float RotZ = 30f;
    private const float Scale = 1.5f;

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

        // сабля чуть левее/выше и меньше, катана без изменений 0.35/0.15
        bool isKatana = gm.katanaEquipped;
        Vector2 effOffset = handOffset;
        if (Mathf.Abs(effOffset.x) > 1f || Mathf.Abs(effOffset.y) > 1f)
            effOffset = new Vector2(HandX, HandY);
        Sprite s;
        Vector3 offset;
        if (isKatana)
            offset = new Vector3(HandXKatana, HandYKatana, HandZ);
        else
            offset = new Vector3(effOffset.x, effOffset.y, HandZ);
        s = isKatana
            ? (katanaSprite != null ? katanaSprite : Resources.Load<Sprite>(DefaultKatanaPath))
            : (saberSprite != null ? saberSprite : Resources.Load<Sprite>(DefaultSaberPath));

        // следуем за рукой: позиция руки + небольшой оффсет
        Vector3 targetLocalPos;
        if (handTransform != null)
            targetLocalPos = handTransform.localPosition - transform.localPosition + offset;
        else
            targetLocalPos = offset;

        if (saberRenderer.transform.localPosition != targetLocalPos)
            saberRenderer.transform.localPosition = targetLocalPos;

        if (s != null && saberRenderer.sprite != s)
            saberRenderer.sprite = s;

        // масштаб: сабля чуть меньше (~0.88), катана как была
        float effScale = handScale;
        if (effScale > 5f) effScale = Scale;
        float targetScale = isKatana ? effScale : effScale * 0.88f;
        // для сабли дефолт 1.32 при effScale 1.5, если инспектор не трогали
        if (Mathf.Abs(handScale - 1.5f) < 0.01f && !isKatana) targetScale = SaberScale;
        Vector3 wantScale = new Vector3(targetScale, targetScale, targetScale);
        if (saberRenderer.transform.localScale != wantScale)
            saberRenderer.transform.localScale = wantScale;
        Quaternion wantRot = Quaternion.Euler(RotX, RotY, handRotation);
        if (saberRenderer.transform.localRotation != wantRot)
            saberRenderer.transform.localRotation = wantRot;
    }

    private Transform handTransform;

    private void CreateSaber()
    {
        GameObject armObj = GameObject.Find("Arm");
        handTransform = armObj != null ? armObj.transform : null;

        GameObject saber = new GameObject("Saber");
        saber.transform.SetParent(transform, false);
        saber.transform.localRotation = Quaternion.Euler(RotX, RotY, RotZ);
        saber.transform.localScale = new Vector3(Scale, Scale, Scale);

        saberRenderer = saber.AddComponent<SpriteRenderer>();
        saberRenderer.sprite = saberSprite != null ? saberSprite : Resources.Load<Sprite>(DefaultSaberPath);
        saberRenderer.sortingOrder = sortingOrder;
        saberRenderer.enabled = false;
    }
}