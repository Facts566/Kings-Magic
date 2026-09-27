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

    [Header("Enemy tiers")]
    public int tier2MinPlayerLevel = 10;
    public string tier1Name = "Бандит";
    public int tier1EnemyLevel = 5;
    public int tier1Hp = 100;
    public int tier1Damage = 3;
    public int tier1Coins = 75;
    public int tier1Xp = 70;
    public string tier2Name = "Джунглевый житель";
    public int tier2EnemyLevel = 14;
    public int tier2Hp = 170;
    public int tier2Damage = 9;
    public int tier2Coins = 85;
    public int tier2Xp = 230;
    public string tier1Type = "Враг";
    public string tier2Type = "Враг";
    public Text enemyNameText;
    public Sprite tier1EnemySprite;
    public Sprite tier2EnemySprite;
    public Sprite tier1BackgroundSprite;
    public Sprite tier2BackgroundSprite;

    private EnemyAttack enemyAttack;
    private int maxEnemyHp;
    private Coroutine hitAnimCoroutine;
    private Vector3 enemyInitialScale;
    private bool enemyScaleInit;

    private const string SaveEnemyHpKey = "enemy_hp";
    private int inspectorMaxHp;

    void Awake()
    {
        inspectorMaxHp = enemy_hp;
    }

    public bool IsTier2()
    {
        return GameManager.Instance != null && GameManager.Instance.level >= tier2MinPlayerLevel;
    }

    private void ApplyTier()
    {
        bool t2 = IsTier2();

        maxEnemyHp = t2 ? tier2Hp : tier1Hp;
        if (maxEnemyHp <= 0) maxEnemyHp = inspectorMaxHp;
        if (maxEnemyHp <= 0) maxEnemyHp = 100;

        if (enemyAttack != null)
            enemyAttack.enemy_damage = t2 ? tier2Damage : tier1Damage;

        if (enemyObject != null)
        {
            SpriteRenderer sr = enemyObject.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Sprite s = t2 ? tier2EnemySprite : tier1EnemySprite;
                if (s != null)
                    sr.sprite = s;
            }
        }

        GameObject bg = GameObject.Find("Background1");
        if (bg != null)
        {
            SpriteRenderer bsr = bg.GetComponent<SpriteRenderer>();
            if (bsr != null)
            {
                Sprite b = t2 ? tier2BackgroundSprite : tier1BackgroundSprite;
                if (b != null)
                    bsr.sprite = b;
            }
        }

        if (enemyNameText != null)
            enemyNameText.text = (t2 ? tier2Name : tier1Name) + " " + (t2 ? tier2EnemyLevel : tier1EnemyLevel).ToString() + " ур";

        if (enemy_hp_slider != null)
            enemy_hp_slider.maxValue = maxEnemyHp;
    }

    void Start()
    {
        if (enemyAttack == null)
        {
            enemyAttack = FindObjectOfType<EnemyAttack>();
            if (enemyAttack != null)
                enemyObject = enemyAttack.gameObject;
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

        if (enemyObject != null)
        {
            enemyInitialScale = enemyObject.transform.localScale;
            enemyScaleInit = true;
        }

        if (enemy_hp_slider != null)
        {
            enemy_hp_slider.maxValue = maxEnemyHp;
            enemy_hp_slider.value = enemy_hp;
        }

        displayedEnemyHp = enemy_hp;
        enemyBarInit = true;

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

                if (enemy_hp <= 0)
                {
                    isEnemyDead = true;
                    SetEnemyVisible(false);
                    SetEnemyUIEnabled(false);

                    if (GameManager.Instance != null)
                    {
                        bool t2 = IsTier2();
                        GameManager.Instance.OnEnemyKilled(t2 ? tier2Coins : tier1Coins, t2 ? tier2Xp : tier1Xp);
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
            enemyInitialScale = enemyObject.transform.localScale;
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
