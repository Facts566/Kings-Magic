using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    public int enemy_hp = 100;
    public Slider enemy_hp_slider;
    public Text enemy_hp_text;

    public int player_damage = 5;
    public float attackCooldown = 1f;
    public float respawnDelay = 5f;

    public bool isEnemyDead = false;

    public GameObject enemyObject;

    [Header("Hit animation")]
    public float hitScale = 1.15f;
    public float hitDuration = 0.12f;

    [Header("Bar smoothing")]
    [Tooltip("Скорость плавности полоски HP врага: меньше = медленнее и плавнее")]
    public float barSmoothness = 8f;

    private float displayedEnemyHp;
    private bool enemyBarInit;

    [Header("Hit shake")]
    [Tooltip("Тряска фона при ударе")]
    public float backgroundShakeAmp = 0.12f;
    [Tooltip("Тряска рук при ударе (слабее фона)")]
    public float armsShakeAmp = 0.05f;
    public float shakeDuration = 0.18f;

    [Header("Damage popup")]
    [Tooltip("Всплывающая цифра урона над врагом")]
    public float popupRiseHeight = 80f;
    public float popupLifetime = 0.8f;
    public int popupFontSize = 45;
    public Color popupColor = new Color(1f, 0.85f, 0.3f, 1f);

    private Coroutine shakeCoroutine;
    private Transform bgTransform;
    private Transform armTransform;
    private Vector3 bgInitPos;
    private Vector3 w1InitPos;
    private Vector3 armInitPos;
    private bool shakeInit;

    [Header("HP bar follow")]
    [Tooltip("Отступ панели HP над головой врага в мировых единицах")]
    public float hpBarAboveHead = 1.2f;

    private RectTransform enemyHpPanel;
    private RectTransform hpPanelParent;
    private Canvas parentCanvas;
    private Camera mainCam;

    [Header("Enemy tiers")]
    public int tier2MinPlayerLevel = 10;
    public int tier3MinPlayerLevel = 15;
    public int tier4MinPlayerLevel = 25;
    public int tier5MinPlayerLevel = 30;
    // Грубиян спавнится на 40-55 уровнях игрока
    public int tier6MinPlayerLevel = 40;
    public int tier6MaxPlayerLevel = 55;
    // Шеф спавнится на 55-60 уровнях игрока (дальше остаётся финальным тиром)
    public int tier7MinPlayerLevel = 55;
    public int tier7MaxPlayerLevel = 60;
    public string tier1Name = "Бандит";
    public int tier1EnemyLevel = 5;
    public int tier1Hp = 100;
    public int tier1Damage = 3;
    public int tier1Coins = 75;
    public int tier1Xp = 70;
    public int tier1Honor = 0;
    public string tier2Name = "Джунглевый житель";
    public int tier2EnemyLevel = 14;
    public int tier2Hp = 170;
    public int tier2Damage = 9;
    public int tier2Coins = 85;
    public int tier2Xp = 230;
    public int tier2Honor = 0;
    public string tier3Name = "Горилла";
    public int tier3EnemyLevel = 20;
    public int tier3Hp = 220;
    public int tier3Damage = 14;
    public int tier3Coins = 205;
    public int tier3Xp = 750;
    public int tier3Honor = 0;
    public string tier4Name = "Джунглевый Президент";
    public int tier4EnemyLevel = 25;
    public int tier4Hp = 950;
    public int tier4Damage = 28;
    public int tier4Coins = 2150;
    public int tier4Xp = 11000;
    public int tier4Honor = 625;
    public string tier5Name = "Пират";
    public int tier5EnemyLevel = 35;
    public int tier5Hp = 700;
    public int tier5Damage = 22;
    public int tier5Coins = 300;
    public int tier5Xp = 1300;
    public int tier5Honor = 0;
    public string tier6Name = "Грубиян";
    public int tier6EnemyLevel = 45;
    public int tier6Hp = 615;
    public int tier6Damage = 29;
    public int tier6Coins = 350;
    public int tier6Xp = 2200;
    public int tier6Honor = 0;
    public string tier7Name = "Шеф";
    public int tier7EnemyLevel = 55;
    public int tier7Hp = 1780;
    public int tier7Damage = 56;
    public int tier7Coins = 8100;
    public int tier7Xp = 50500;
    public int tier7Honor = 1375;
    [Header("Enemy scale")]
    [Tooltip("Масштаб врага для каждого тира, 1 = как выставлено в сцене")]
    public float tier1EnemyScale = 1f;
    public float tier2EnemyScale = 1f;
    public float tier3EnemyScale = 1f;
    public float tier4EnemyScale = 1f;
    public float tier5EnemyScale = 0.3333f; // Пират втрое меньше остальных
    public float tier6EnemyScale = 1f;
    public float tier7EnemyScale = 1f;
    public Text enemyNameText;
    public Sprite tier1EnemySprite;
    public Sprite tier2EnemySprite;
    public Sprite tier3EnemySprite;
    public Sprite tier4EnemySprite;
    public Sprite tier5EnemySprite;
    public Sprite tier6EnemySprite;
    public Sprite tier7EnemySprite;
    public Sprite tier1BackgroundSprite;
    public Sprite tier2BackgroundSprite;
    public Sprite tier3BackgroundSprite;
    public Sprite tier4BackgroundSprite;
    public Sprite tier5BackgroundSprite;
    public Sprite tier6BackgroundSprite;
    public Sprite tier7BackgroundSprite;

    private EnemyAttack enemyAttack;
    private int maxEnemyHp;
    private Coroutine hitAnimCoroutine;
    private Vector3 enemyInitialScale;
    private Vector3 baseEnemyScale;
    private bool enemyScaleInit;

    private const string SaveEnemyHpKey = "enemy_hp";
    private int inspectorMaxHp;

    void Awake()
    {
        inspectorMaxHp = enemy_hp;
    }

    // 0 = Бандит (<10), 1 = Житель (10+), 2 = Горилла (15+), 3 = Президент (25+),
    // 4 = Пират (30+), 5 = Грубиян (40-55), 6 = Шеф (55+, босс)
    public int GetTierIndex()
    {
        if (GameManager.Instance == null)
            return 0;
        int lvl = GameManager.Instance.level;
        if (lvl >= tier7MinPlayerLevel) return 6;
        if (lvl >= tier6MinPlayerLevel) return 5;
        if (lvl >= tier5MinPlayerLevel) return 4;
        if (lvl >= tier4MinPlayerLevel) return 3;
        if (lvl >= tier3MinPlayerLevel) return 2;
        if (lvl >= tier2MinPlayerLevel) return 1;
        return 0;
    }

    private void TierStats(int t, out string name, out int level, out int hp, out int damage, out Sprite enemySprite, out Sprite bgSprite, out float scale)
    {
        switch (t)
        {
            case 6:
                name = tier7Name; level = tier7EnemyLevel; hp = tier7Hp; damage = tier7Damage;
                enemySprite = tier7EnemySprite; bgSprite = tier7BackgroundSprite; scale = tier7EnemyScale;
                break;
            case 5:
                name = tier6Name; level = tier6EnemyLevel; hp = tier6Hp; damage = tier6Damage;
                enemySprite = tier6EnemySprite; bgSprite = tier6BackgroundSprite; scale = tier6EnemyScale;
                break;
            case 4:
                name = tier5Name; level = tier5EnemyLevel; hp = tier5Hp; damage = tier5Damage;
                enemySprite = tier5EnemySprite; bgSprite = tier5BackgroundSprite; scale = tier5EnemyScale;
                break;
            case 3:
                name = tier4Name; level = tier4EnemyLevel; hp = tier4Hp; damage = tier4Damage;
                enemySprite = tier4EnemySprite; bgSprite = tier4BackgroundSprite; scale = tier4EnemyScale;
                break;
            case 2:
                name = tier3Name; level = tier3EnemyLevel; hp = tier3Hp; damage = tier3Damage;
                enemySprite = tier3EnemySprite; bgSprite = tier3BackgroundSprite; scale = tier3EnemyScale;
                break;
            case 1:
                name = tier2Name; level = tier2EnemyLevel; hp = tier2Hp; damage = tier2Damage;
                enemySprite = tier2EnemySprite; bgSprite = tier2BackgroundSprite; scale = tier2EnemyScale;
                break;
            default:
                name = tier1Name; level = tier1EnemyLevel; hp = tier1Hp; damage = tier1Damage;
                enemySprite = tier1EnemySprite; bgSprite = tier1BackgroundSprite; scale = tier1EnemyScale;
                break;
        }
    }

    private void TierRewards(int t, out int coins, out int xp, out int honor)
    {
        switch (t)
        {
            case 6: coins = tier7Coins; xp = tier7Xp; honor = tier7Honor; break;
            case 5: coins = tier6Coins; xp = tier6Xp; honor = tier6Honor; break;
            case 4: coins = tier5Coins; xp = tier5Xp; honor = tier5Honor; break;
            case 3: coins = tier4Coins; xp = tier4Xp; honor = tier4Honor; break;
            case 2: coins = tier3Coins; xp = tier3Xp; honor = tier3Honor; break;
            case 1: coins = tier2Coins; xp = tier2Xp; honor = tier2Honor; break;
            default: coins = tier1Coins; xp = tier1Xp; honor = tier1Honor; break;
        }
    }

    private void ApplyTier()
    {
        int t = GetTierIndex();
        TierStats(t, out string eName, out int eLevel, out int eHp, out int eDamage, out Sprite eSprite, out Sprite bSprite, out float eScale);

        maxEnemyHp = eHp;
        if (maxEnemyHp <= 0) maxEnemyHp = inspectorMaxHp;
        if (maxEnemyHp <= 0) maxEnemyHp = 100;

        if (enemyAttack != null)
            enemyAttack.enemy_damage = eDamage;

        if (enemyObject != null)
        {
            SpriteRenderer sr = enemyObject.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && eSprite != null)
                sr.sprite = eSprite;
        }

        ApplyEnemyScale(eScale);

        SpriteRenderer bsr = ResolveBackground();
        if (bsr != null && bSprite != null)
            bsr.sprite = bSprite;

        if (enemyNameText != null)
            enemyNameText.text = eName + " " + eLevel.ToString() + " ур";

        if (enemy_hp_slider != null)
            enemy_hp_slider.maxValue = maxEnemyHp;
    }

    // Масштаб врага = базовый масштаб из сцены * множитель тира.
// enemyInitialScale всегда хранит уже итоговый размер — от него считаются
// анимация удара и высота HP-бара над головой.
private void ApplyEnemyScale(float scale)
    {
        if (enemyObject == null)
            return;

        if (!enemyScaleInit)
        {
            baseEnemyScale = enemyObject.transform.localScale;
            enemyScaleInit = true;
        }

        if (scale <= 0f)
            scale = 1f;

        enemyInitialScale = baseEnemyScale * scale;

        if (!isEnemyDead)
            enemyObject.transform.localScale = enemyInitialScale;
    }

    void Start()
    {
        if (enemyAttack == null)
        {
            enemyAttack = FindObjectOfType<EnemyAttack>();
            if (enemyAttack != null)
                enemyObject = enemyAttack.gameObject;
        }

        // базовый масштаб врага снимаем до ApplyTier(), он его потом умножит на множитель тира
        if (enemyObject != null)
        {
            baseEnemyScale = enemyObject.transform.localScale;
            enemyScaleInit = true;
        }

        ApplyTier();
        if (maxEnemyHp <= 0) maxEnemyHp = 100;
        if (PlayerPrefs.HasKey(SaveEnemyHpKey))
        {
            int saved = PlayerPrefs.GetInt(SaveEnemyHpKey, maxEnemyHp);
            if (saved > 0 && saved <= maxEnemyHp)
            {
                enemy_hp = saved;
                isEnemyDead = false;
            }
            else if (saved <= 0)
            {
                enemy_hp = maxEnemyHp;
            }
            else
            {
                enemy_hp = Mathf.Clamp(saved, 0, maxEnemyHp);
            }
        }
        else
        {
            enemy_hp = maxEnemyHp;
        }

        if (enemy_hp_slider != null)
        {
            enemy_hp_slider.maxValue = maxEnemyHp;
            enemy_hp_slider.value = enemy_hp;
        }

        displayedEnemyHp = enemy_hp;
        enemyBarInit = true;

        InitHitShake();

        if (enemy_hp_slider != null)
        {
            enemyHpPanel = enemy_hp_slider.GetComponent<RectTransform>();
            if (enemyHpPanel != null)
            {
                hpPanelParent = enemyHpPanel.parent as RectTransform;
                parentCanvas = enemyHpPanel.GetComponentInParent<Canvas>();
            }
        }
        mainCam = Camera.main;

        UpdateEnemyUI();
        if (enemy_hp <= 0 && !isEnemyDead)
        {
            // ensure dead state visually if loaded 0
            isEnemyDead = true;
            SetEnemyVisible(false);
            SetEnemyUIEnabled(false);
            StartCoroutine(RespawnEnemy());
        }
        else
        {
            StartCoroutine(AutoAttack());
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveEnemyHp();
    }

    void OnApplicationQuit()
    {
        SaveEnemyHp();
    }

    void OnDisable()
    {
        SaveEnemyHp();
    }

    private void SaveEnemyHp()
    {
        PlayerPrefs.SetInt(SaveEnemyHpKey, enemy_hp);
        PlayerPrefs.Save();
    }

    private IEnumerator AutoAttack()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackCooldown);

            if (!isEnemyDead && (enemyAttack == null || !enemyAttack.isPlayerDead))
            {
                int damage = player_damage;

                if (GameManager.Instance != null)
                {
                    int baseDamage = GameManager.Instance.GetBaseDamage(player_damage);
                    damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * GameManager.Instance.GetDamageMultiplier()));
                }

                enemy_hp -= damage;
                if (enemy_hp < 0)
                    enemy_hp = 0;

                UpdateEnemyUI();
                SaveEnemyHp();
                PlayHitAnimation();
                PlayHitShake();
                SpawnDamagePopup(damage);

                if (enemy_hp <= 0)
                {
                    isEnemyDead = true;
                    SetEnemyVisible(false);
                    SetEnemyUIEnabled(false);

                    if (GameManager.Instance != null)
                    {
                        TierRewards(GetTierIndex(), out int rc, out int rx, out int rh);
                        GameManager.Instance.OnEnemyKilled(rc, rx, rh);
                    }

                    StartCoroutine(RespawnEnemy());
                }
            }
        }
    }

    private void PlayHitAnimation()
    {
        if (enemyObject == null) return;
        if (!enemyScaleInit)
        {
            baseEnemyScale = enemyObject.transform.localScale;
            enemyInitialScale = baseEnemyScale;
            enemyScaleInit = true;
        }
        if (hitAnimCoroutine != null)
            StopCoroutine(hitAnimCoroutine);
        hitAnimCoroutine = StartCoroutine(HitAnimation());
    }

    private IEnumerator HitAnimation()
    {
        if (enemyObject == null) yield break;
        Transform t = enemyObject.transform;
        Vector3 start = enemyInitialScale;
        Vector3 peak = start * hitScale;
        float half = hitDuration * 0.5f;

        // scale up
        float e = 0f;
        while (e < half)
        {
            e += Time.deltaTime;
            float p = Mathf.Clamp01(e / half);
            t.localScale = Vector3.Lerp(start, peak, p);
            yield return null;
        }
        // scale down
        e = 0f;
        while (e < half)
        {
            e += Time.deltaTime;
            float p = Mathf.Clamp01(e / half);
            t.localScale = Vector3.Lerp(peak, start, p);
            yield return null;
        }
        t.localScale = start;
        hitAnimCoroutine = null;
    }

    // Фон в сцене называется "Background" (исторически искали "Background1"),
// поэтому ищем по SpriteRenderer, а не по точному имени объекта
private SpriteRenderer bgRenderer;

    private static bool IsBackgroundName(string n)
    {
        return n == "Background1" || n == "Background" || n.StartsWith("Background_");
    }

    private SpriteRenderer ResolveBackground()
    {
        if (bgRenderer != null)
            return bgRenderer;

        SpriteRenderer[] all = FindObjectsOfType<SpriteRenderer>();
        SpriteRenderer best = null;
        float bestArea = 0f;

        // 1) спрайты на объектах с именем фона
        for (int i = 0; i < all.Length; i++)
        {
            SpriteRenderer sr = all[i];
            if (sr == null || sr.sprite == null) continue;
            if (enemyObject != null && sr.transform.IsChildOf(enemyObject.transform)) continue;
            if (!IsBackgroundName(sr.gameObject.name)) continue;
            float area = sr.bounds.size.x * sr.bounds.size.y;
            if (area > bestArea) { bestArea = area; best = sr; }
        }

        // 2) запасной путь: самый большой спрайт в сцене (фон всегда крупнее врага)
        if (best == null)
        {
            for (int i = 0; i < all.Length; i++)
            {
                SpriteRenderer sr = all[i];
                if (sr == null || sr.sprite == null) continue;
                if (enemyObject != null && sr.transform.IsChildOf(enemyObject.transform)) continue;
                float area = sr.bounds.size.x * sr.bounds.size.y;
                if (area > bestArea) { bestArea = area; best = sr; }
            }
        }

        bgRenderer = best;
        if (bgRenderer != null)
            bgTransform = bgRenderer.transform;
        return bgRenderer;
    }

    private void InitHitShake()
    {
        SpriteRenderer bsr = ResolveBackground();
        if (bsr != null)
        {
            bgTransform = bsr.transform;
            bgInitPos = bgTransform.position;
        }

        GameObject arm = GameObject.Find("Arm");
        if (arm != null)
        {
            armTransform = arm.transform;
            armInitPos = armTransform.position;
        }

        w1InitPos = transform.position;
        shakeInit = bgTransform != null || armTransform != null;
    }

    private void PlayHitShake()
    {
        if (!shakeInit)
            return;
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            RestoreShakePositions();
        }
        shakeCoroutine = StartCoroutine(HitShake());
    }

    private void RestoreShakePositions()
    {
        if (bgTransform != null)
            bgTransform.position = bgInitPos;
        transform.position = w1InitPos;
        if (armTransform != null)
            armTransform.position = armInitPos;
    }

    // Цифра урона над головой врага: всплывает и гаснет
    private void SpawnDamagePopup(int damage)
    {
        if (hpPanelParent == null || enemyObject == null)
            return;

        float halfH = 1f;
        SpriteRenderer sr = enemyObject.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float sy = enemyScaleInit ? enemyInitialScale.y : enemyObject.transform.localScale.y;
            if (Mathf.Abs(sy) < 0.01f) sy = 1f;
            halfH = sr.sprite.bounds.extents.y * Mathf.Abs(sy);
        }
        Vector3 world = enemyObject.transform.position + new Vector3(0f, halfH + 0.5f, 0f);
        Vector3 screen = mainCam != null ? mainCam.WorldToScreenPoint(world) : world;

        Camera uiCam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? (parentCanvas.worldCamera != null ? parentCanvas.worldCamera : mainCam)
            : null;

        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(hpPanelParent, screen, uiCam, out local))
            return;
        local.x += Random.Range(-30f, 30f);

        GameObject go = new GameObject("DamagePopup");
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.SetParent(hpPanelParent, false);
        rt.sizeDelta = new Vector2(300f, 80f);
        rt.anchoredPosition = local;

        Text t = go.AddComponent<Text>();
        t.font = enemy_hp_text != null ? enemy_hp_text.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = popupFontSize;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        t.color = popupColor;
        t.text = "-" + damage.ToString();

        StartCoroutine(DamagePopupAnim(rt, t));
    }

    private IEnumerator DamagePopupAnim(RectTransform rt, Text t)
    {
        float e = 0f;
        Vector2 start = rt.anchoredPosition;
        while (e < popupLifetime)
        {
            e += Time.deltaTime;
            float p = Mathf.Clamp01(e / popupLifetime);
            if (rt != null)
                rt.anchoredPosition = start + new Vector2(0f, popupRiseHeight * p);
            if (t != null)
            {
                Color c = t.color;
                c.a = 1f - p;
                t.color = c;
            }
            yield return null;
        }
        if (rt != null)
            Destroy(rt.gameObject);
    }

    private IEnumerator HitShake()
    {
        float e = 0f;
        while (e < shakeDuration)
        {
            e += Time.deltaTime;
            float k = Mathf.Clamp01(1f - e / shakeDuration);
            Vector3 bgOff = (Vector3)(Random.insideUnitCircle * backgroundShakeAmp * k);
            Vector3 armsOff = (Vector3)(Random.insideUnitCircle * armsShakeAmp * k);
            if (bgTransform != null)
                bgTransform.position = bgInitPos + bgOff;
            transform.position = w1InitPos + armsOff;
            if (armTransform != null)
                armTransform.position = armInitPos + armsOff;
            yield return null;
        }
        RestoreShakePositions();
        shakeCoroutine = null;
    }

    // Пересчёт тира на живом враге (например, уровень упал 10→9 после смерти):
    // фон, спрайт, урон и макс. хп меняются сразу, текущее хп клампится
    public void RefreshTierLive()
    {
        ApplyTier();
        if (!isEnemyDead)
        {
            if (enemy_hp > maxEnemyHp)
                enemy_hp = maxEnemyHp;
            UpdateEnemyUI();
            SaveEnemyHp();
        }
    }

    private IEnumerator RespawnEnemy()
    {
        yield return new WaitForSeconds(respawnDelay);

        // уровень мог измениться (килл дал опыт) — пересчитываем тир: фон, спрайт, урон, хп
        ApplyTier();
        enemy_hp = maxEnemyHp;
        isEnemyDead = false;
        SetEnemyVisible(true);
        SetEnemyUIEnabled(true);
        UpdateEnemyUI();
        SnapEnemyBar();
        SaveEnemyHp();
        // restart auto attack if it ended (we keep same coroutine, but ensure loop continues)
        // AutoAttack is infinite, but if enemy was dead we still loop; no need to restart
        if (enemyObject != null && enemyScaleInit)
            enemyObject.transform.localScale = enemyInitialScale;
    }

    // ТЕСТОВЫЕ ЧИТЫ (временные) — вызываются из DebugCheats.cs
    // Мгновенно убивает врага и выдаёт дроп его тира (удобно проверять награды)
    public void CheatKillEnemy()
    {
        if (isEnemyDead)
            return;

        enemy_hp = 0;
        displayedEnemyHp = 0;
        isEnemyDead = true;
        SetEnemyVisible(false);
        SetEnemyUIEnabled(false);
        UpdateEnemyUI();
        SaveEnemyHp();

        if (GameManager.Instance != null)
        {
            TierRewards(GetTierIndex(), out int rc, out int rx, out int rh);
            GameManager.Instance.OnEnemyKilled(rc, rx, rh);
        }

        StartCoroutine(RespawnEnemy());
    }

    // ТЕСТОВЫЙ ЧИТ (временный) — полный откат HP врага к максимуму
    public void CheatRespawnEnemyNow()
    {
        StartCoroutine(RespawnEnemy());
    }

    private void SetEnemyUIEnabled(bool visible)
    {
        if (enemy_hp_slider != null)
            enemy_hp_slider.gameObject.SetActive(visible);

        if (enemy_hp_text != null)
            enemy_hp_text.gameObject.SetActive(visible);
    }

    private void SetEnemyVisible(bool visible)
    {
        if (enemyObject == null) return;

        foreach (SpriteRenderer renderer in enemyObject.GetComponentsInChildren<SpriteRenderer>())
            renderer.enabled = visible;
    }

    void Update()
    {
        if (!enemyBarInit || enemy_hp_slider == null)
            return;

        float k = 1f - Mathf.Exp(-Mathf.Max(0.1f, barSmoothness) * Time.deltaTime);
        displayedEnemyHp = Mathf.Lerp(displayedEnemyHp, enemy_hp, k);
        if (Mathf.Abs(displayedEnemyHp - enemy_hp) < 0.05f)
            displayedEnemyHp = enemy_hp;
        enemy_hp_slider.value = displayedEnemyHp;

        FollowEnemyHpBar();
    }

    // Панель HP (полоска + цифры + имя) всегда висит над головой врага,
    // высота учитывает реальный размер спрайта — у босса полоска выше
    private void FollowEnemyHpBar()
    {
        if (enemyObject == null || enemyHpPanel == null || hpPanelParent == null)
            return;
        if (!enemy_hp_slider.gameObject.activeInHierarchy)
            return;

        // базовая высота без анимации удара, чтобы полоска не прыгала при попадании
        float halfH = 1f;
        SpriteRenderer sr = enemyObject.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float sy = enemyScaleInit ? enemyInitialScale.y : enemyObject.transform.localScale.y;
            if (Mathf.Abs(sy) < 0.01f) sy = 1f;
            halfH = sr.sprite.bounds.extents.y * Mathf.Abs(sy);
        }

        Vector3 world = enemyObject.transform.position + new Vector3(0f, halfH + hpBarAboveHead, 0f);
        Vector3 screen = mainCam != null ? mainCam.WorldToScreenPoint(world) : world;

        Camera uiCam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? (parentCanvas.worldCamera != null ? parentCanvas.worldCamera : mainCam)
            : null;

        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(hpPanelParent, screen, uiCam, out local))
            enemyHpPanel.anchoredPosition = local;
    }

    private void UpdateEnemyUI()
    {
        // цифры — сразу точные, полоска догоняет плавно в Update()
        if (enemy_hp_slider != null)
            enemy_hp_slider.maxValue = maxEnemyHp;

        if (enemy_hp_text != null)
            enemy_hp_text.text = enemy_hp.ToString() + "/" + maxEnemyHp.ToString();
    }

    private void SnapEnemyBar()
    {
        displayedEnemyHp = enemy_hp;
        if (enemy_hp_slider != null)
            enemy_hp_slider.value = displayedEnemyHp;
    }
}
