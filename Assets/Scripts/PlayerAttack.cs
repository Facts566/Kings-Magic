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

    void Start()
    {
        maxEnemyHp = inspectorMaxHp;
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

        if (enemyAttack == null)
        {
            enemyAttack = FindObjectOfType<EnemyAttack>();
            if (enemyAttack != null)
                enemyObject = enemyAttack.gameObject;
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
                        GameManager.Instance.OnEnemyKilled();

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

    private IEnumerator RespawnEnemy()
    {
        yield return new WaitForSeconds(respawnDelay);

        enemy_hp = maxEnemyHp;
        isEnemyDead = false;
        SetEnemyVisible(true);
        SetEnemyUIEnabled(true);
        UpdateEnemyUI();
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

    private void UpdateEnemyUI()
    {
        if (enemy_hp_slider != null)
            enemy_hp_slider.value = enemy_hp;

        if (enemy_hp_text != null)
            enemy_hp_text.text = enemy_hp.ToString() + "/" + maxEnemyHp.ToString();
    }
}
