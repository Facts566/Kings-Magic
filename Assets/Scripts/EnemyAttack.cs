using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyAttack : MonoBehaviour
{
    public int player_hp = 100;
    public int max_player_hp = 100;
    public Slider player_hp_slider;
    public Text player_hp_text;

    public int player_energy = 100;
    public int max_player_energy = 100;
    public Slider player_energy_slider;
    public Text player_energy_text;

    public int enemy_damage = 3;
    public float attackCooldown = 1f;
    public float playerRespawnDelay = 3f;

    public PlayerAttack PA;

    public bool isPlayerDead = false;

    public DodgeController dodgeController;

    private const string SaveHpKey = "player_hp";
    private const string SaveEnergyKey = "player_energy";

    void Awake()
    {
        LoadHp();
    }

    void Start()
    {
        if (max_player_hp < player_hp)
            max_player_hp = player_hp;

        if (player_energy_slider == null || player_energy_text == null)
            FindEnergyUI();

        if (dodgeController == null)
            dodgeController = FindObjectOfType<DodgeController>();

        // clamp loaded values to new max
        player_hp = Mathf.Clamp(player_hp, 0, max_player_hp);
        player_energy = Mathf.Clamp(player_energy, 0, max_player_energy);

        if (player_hp_slider != null)
            player_hp_slider.maxValue = max_player_hp;

        if (player_energy_slider != null)
            player_energy_slider.maxValue = max_player_energy;

        UpdatePlayerUI();
        StartCoroutine(AutoAttack());
        StartCoroutine(RegenCoroutine());
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveHp();
    }

    void OnApplicationQuit()
    {
        SaveHp();
    }

    void OnDisable()
    {
        SaveHp();
    }

    private void LoadHp()
    {
        if (PlayerPrefs.HasKey(SaveHpKey))
            player_hp = PlayerPrefs.GetInt(SaveHpKey, player_hp);
        if (PlayerPrefs.HasKey(SaveEnergyKey))
            player_energy = PlayerPrefs.GetInt(SaveEnergyKey, player_energy);
    }

    private void SaveHp()
    {
        PlayerPrefs.SetInt(SaveHpKey, player_hp);
        PlayerPrefs.SetInt(SaveEnergyKey, player_energy);
        PlayerPrefs.Save();
    }

    private void FindEnergyUI()
    {
        foreach (Slider s in FindObjectsOfType<Slider>())
        {
            if (s.name == "EnergySlider")
            {
                player_energy_slider = s;
                break;
            }
        }

        if (player_energy_slider != null)
        {
            Text t = player_energy_slider.GetComponentInChildren<Text>();
            if (t != null)
                player_energy_text = t;
        }
    }

    private IEnumerator AutoAttack()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackCooldown);

            if (!isPlayerDead && (PA == null || !PA.isEnemyDead))
            {
                if (dodgeController != null && dodgeController.ConsumeDodge())
                {
                    Debug.Log("Игрок увернулся от атаки!");
                }
                else
                {
                    player_hp -= enemy_damage;
                    if (player_hp < 0)
                        player_hp = 0;

                    UpdatePlayerUI();
                    SaveHp();

                    if (player_hp <= 0)
                    {
                        isPlayerDead = true;
                        // штраф: обнуление денег и -1 уровень
                        if (GameManager.Instance != null)
                            GameManager.Instance.OnPlayerDied();
                        SaveHp();
                        StartCoroutine(RespawnPlayer());
                    }
                }
            }
        }
    }

    private IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(playerRespawnDelay);

        player_hp = max_player_hp;
        player_energy = max_player_energy;
        isPlayerDead = false;
        UpdatePlayerUI();
        SaveHp();
    }

    private IEnumerator RegenCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (!isPlayerDead)
            {
                bool changed = false;
                if (player_hp < max_player_hp)
                {
                    player_hp++;
                    if (player_hp > max_player_hp)
                        player_hp = max_player_hp;
                    changed = true;
                }

                if (player_energy < max_player_energy)
                {
                    player_energy++;
                    if (player_energy > max_player_energy)
                        player_energy = max_player_energy;
                    changed = true;
                }

                if (changed)
                {
                    UpdatePlayerUI();
                    SaveHp();
                }
            }
        }
    }

    private void UpdatePlayerUI()
    {
        if (player_hp_slider != null)
            player_hp_slider.value = player_hp;

        if (player_hp_text != null)
            player_hp_text.text = player_hp.ToString() + "/" + max_player_hp.ToString();

        if (player_energy_slider != null)
            player_energy_slider.value = player_energy;

        if (player_energy_text != null)
            player_energy_text.text = player_energy.ToString() + "/" + max_player_energy.ToString();
    }

    public void SetMaxStats(int newMaxHp, int newMaxEnergy)
    {
        max_player_hp = newMaxHp;
        max_player_energy = newMaxEnergy;

        // сохраняем текущее хп как у врага - только кламп, без автоисцеления при повышении максимума
        if (player_hp > max_player_hp)
            player_hp = max_player_hp;
        if (player_energy > max_player_energy)
            player_energy = max_player_energy;
        if (player_hp < 0) player_hp = 0;
        if (player_energy < 0) player_energy = 0;

        if (player_hp_slider != null)
            player_hp_slider.maxValue = max_player_hp;

        if (player_energy_slider != null)
            player_energy_slider.maxValue = max_player_energy;

        UpdatePlayerUI();
        SaveHp();
    }

    public bool TrySpendEnergy(int cost)
    {
        if (player_energy < cost)
            return false;

        player_energy -= cost;
        UpdatePlayerUI();
        SaveHp();
        return true;
    }
}
