using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coinsPerKill = 75;
    public int xpPerKill = 70;
    public int maxLevel = 2800;
    public int statPointsPerLevel = 3;

    public int baseMaxHp = 100;
    public int baseMaxEnergy = 100;
    public int defenseHpPerLevel = 5;
    public int meleeEnergyPerLevel = 5;
    public float damagePercentPerLevel = 0.05f;

    public int coins;
    public int level = 1;
    public int xp;
    public int statPoints;

    public int meleeLevel = 1;
    public int defenseLevel = 1;
    public int swordLevel = 1;
    public int weaponLevel = 1;
    public int magicLevel = 1;

    public int saberDamage = 3;
    public bool saberEquipped = false;
    public int katanaDamage = 6;
    public bool katanaEquipped = false;

    public WeaponType currentWeaponType = WeaponType.CombatStyle;

    public Text coinsText;
    public Text levelText;
    public Text pointsText;
    public Text meleeText;
    public Text defenseText;
    public Text swordText;
    public Text weaponText;
    public Text magicText;

    private EnemyAttack enemyAttack;

    void Awake()
    {
        Instance = this;
        Load();
    }

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (enemyAttack == null)
            enemyAttack = FindObjectOfType<EnemyAttack>();

        ApplyStatBonuses();
        RefreshUI();
        EnsureWeaponCells();
    }

    private void EnsureWeaponCells()
    {
        // Найти контейнер Weapons и гарантировать что ячейки 1 и 2 имеют WeaponCell
        Transform weapons = null;
        var all = FindObjectsOfType<RectTransform>();
        foreach (var rt in all)
        {
            if (rt.name == "Weapons")
            {
                weapons = rt;
                break;
            }
        }
        if (weapons == null) return;
        for (int i = 0; i < weapons.childCount; i++)
        {
            var child = weapons.GetChild(i);
            int desiredIndex = -1;
            if (child.name == "1") desiredIndex = 1;
            else if (child.name == "2") desiredIndex = 2;
            if (desiredIndex == -1) continue;
            var wc = child.GetComponent<WeaponCell>();
            if (wc == null)
                wc = child.gameObject.AddComponent<WeaponCell>();
            wc.cellIndex = desiredIndex;
            if (wc.backgroundImage == null)
                wc.backgroundImage = child.GetComponent<Image>();
        }
    }

    public int XpNeededForLevelUp()
    {
        return level * 100;
    }

    public void OnEnemyKilled()
    {
        coins += coinsPerKill;
        AddExperience(xpPerKill);
        RefreshUI();
        Save();
    }

    public bool TrySpendCoins(int amount)
    {
        if (coins < amount)
            return false;

        coins -= amount;
        RefreshUI();
        Save();
        return true;
    }

    public void AddExperience(int amount)
    {
        if (level >= maxLevel)
        {
            xp = 0;
            return;
        }

        xp += amount;

        while (level < maxLevel && xp >= XpNeededForLevelUp())
        {
            xp -= XpNeededForLevelUp();
            level++;
            statPoints += statPointsPerLevel;
        }

        if (level >= maxLevel)
            xp = 0;
    }

    public void SpendMelee() { SpendPoint(StatType.Melee); }
    public void SpendDefense() { SpendPoint(StatType.Defense); }
    public void SpendSword() { SpendPoint(StatType.Sword); }
    public void SpendWeapon() { SpendPoint(StatType.Weapon); }
    public void SpendMagic() { SpendPoint(StatType.Magic); }

    private void SpendPoint(StatType type)
    {
        if (statPoints <= 0)
            return;

        statPoints--;

        switch (type)
        {
            case StatType.Melee: meleeLevel++; break;
            case StatType.Defense: defenseLevel++; break;
            case StatType.Sword: swordLevel++; break;
            case StatType.Weapon: weaponLevel++; break;
            case StatType.Magic: magicLevel++; break;
        }

        ApplyStatBonuses();
        RefreshUI();
        Save();
    }

    private void ApplyStatBonuses()
    {
        if (enemyAttack == null)
            enemyAttack = FindObjectOfType<EnemyAttack>();

        if (enemyAttack != null)
        {
            int maxHp = baseMaxHp + defenseHpPerLevel * Mathf.Max(0, defenseLevel - 1);
            int maxEnergy = baseMaxEnergy + meleeEnergyPerLevel * Mathf.Max(0, meleeLevel - 1);
            enemyAttack.SetMaxStats(maxHp, maxEnergy);
        }
    }

    public void EquipFists()
    {
        saberEquipped = false;
        katanaEquipped = false;
        currentWeaponType = WeaponType.CombatStyle;
        RefreshUI();
        Save();
    }

    private const string LastSwordKey = "lastSwordIsKatana";

    public void EquipSaber()
    {
        if (PlayerPrefs.GetInt(SaberShop.OwnedKey, 0) != 1)
            return;

        saberEquipped = true;
        katanaEquipped = false;
        currentWeaponType = WeaponType.Sword;
        PlayerPrefs.SetInt(LastSwordKey, 0);
        RefreshUI();
        Save();
    }

    public void EquipKatana()
    {
        if (PlayerPrefs.GetInt(SaberShop.KatanaOwnedKey, 0) != 1)
            return;

        katanaEquipped = true;
        saberEquipped = false;
        currentWeaponType = WeaponType.Sword;
        PlayerPrefs.SetInt(LastSwordKey, 1);
        RefreshUI();
        Save();
    }

    public void EquipCell2Sword()
    {
        bool hasSaber = PlayerPrefs.GetInt(SaberShop.OwnedKey, 0) == 1;
        bool hasKatana = PlayerPrefs.GetInt(SaberShop.KatanaOwnedKey, 0) == 1;
        if (!hasSaber && !hasKatana) return;

        bool lastIsKatana = PlayerPrefs.GetInt(LastSwordKey, hasKatana ? 1 : 0) == 1;

        // если последним был катана и он есть - надеваем его, иначе саблю
        if (lastIsKatana && hasKatana)
            EquipKatana();
        else if (hasSaber)
            EquipSaber();
        else if (hasKatana)
            EquipKatana();
    }

    public bool IsSwordOwned()
    {
        return PlayerPrefs.GetInt(SaberShop.OwnedKey, 0) == 1 || PlayerPrefs.GetInt(SaberShop.KatanaOwnedKey, 0) == 1;
    }

    public bool IsSwordEquipped()
    {
        return saberEquipped || katanaEquipped;
    }

    public void OnPlayerDied()
    {
        coins = 0;
        if (level > 0)
            level = Mathf.Max(0, level - 1);
        xp = 0;
        RefreshUI();
        Save();
    }

    public int GetBaseDamage(int fistDamage)
    {
        if (katanaEquipped)
            return katanaDamage;
        if (saberEquipped)
            return saberDamage;
        return fistDamage;
    }

    public float GetDamageMultiplier()
    {
        int statLevel;

        switch (currentWeaponType)
        {
            case WeaponType.CombatStyle: statLevel = meleeLevel; break;
            case WeaponType.Sword: statLevel = swordLevel; break;
            case WeaponType.Weapon: statLevel = weaponLevel; break;
            case WeaponType.Magic: statLevel = magicLevel; break;
            default: statLevel = meleeLevel; break;
        }

        return 1f + damagePercentPerLevel * Mathf.Max(0, statLevel - 1);
    }

    public void RefreshUI()
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();

        if (levelText != null)
            levelText.text = "Lvl " + level.ToString();

        if (pointsText != null)
            pointsText.text = "Очки:               " + statPoints.ToString();

        if (meleeText != null)
            meleeText.text = "Ближний бой: " + meleeLevel.ToString();

        if (defenseText != null)
            defenseText.text = "Защита: " + defenseLevel.ToString();

        if (swordText != null)
            swordText.text = "Меч: " + swordLevel.ToString();

        if (weaponText != null)
            weaponText.text = "Оружие: " + weaponLevel.ToString();

        if (magicText != null)
            magicText.text = "Магия: " + magicLevel.ToString();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("coins");
        PlayerPrefs.DeleteKey("level");
        PlayerPrefs.DeleteKey("xp");
        PlayerPrefs.DeleteKey("statPoints");
        PlayerPrefs.DeleteKey("meleeLevel");
        PlayerPrefs.DeleteKey("defenseLevel");
        PlayerPrefs.DeleteKey("swordLevel");
        PlayerPrefs.DeleteKey("weaponLevel");
        PlayerPrefs.DeleteKey("magicLevel");
        PlayerPrefs.DeleteKey("currentWeaponType");
        PlayerPrefs.DeleteKey("saberOwned");
        PlayerPrefs.DeleteKey("saberEquipped");
        PlayerPrefs.DeleteKey(SaberShop.KatanaOwnedKey);
        PlayerPrefs.DeleteKey("katanaEquipped");
        PlayerPrefs.DeleteKey(LastSwordKey);
        PlayerPrefs.DeleteKey("player_hp");
        PlayerPrefs.DeleteKey("player_energy");
        PlayerPrefs.DeleteKey("enemy_hp");
        PlayerPrefs.Save();

        coins = 0;
        level = 1;
        xp = 0;
        statPoints = 0;
        meleeLevel = 1;
        defenseLevel = 1;
        swordLevel = 1;
        weaponLevel = 1;
        magicLevel = 1;
        currentWeaponType = WeaponType.CombatStyle;
        saberEquipped = false;
        katanaEquipped = false;

        ApplyStatBonuses();
        RefreshUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetProgress();

            SaberShop shop = FindObjectOfType<SaberShop>();
            if (shop != null)
                shop.ResetSaber();
        }
    }

    public enum WeaponType { CombatStyle, Sword, Weapon, Magic }

    private enum StatType { Melee, Defense, Sword, Weapon, Magic }

    private void Save()
    {
        PlayerPrefs.SetInt("coins", coins);
        PlayerPrefs.SetInt("level", level);
        PlayerPrefs.SetInt("xp", xp);
        PlayerPrefs.SetInt("statPoints", statPoints);
        PlayerPrefs.SetInt("meleeLevel", meleeLevel);
        PlayerPrefs.SetInt("defenseLevel", defenseLevel);
        PlayerPrefs.SetInt("swordLevel", swordLevel);
        PlayerPrefs.SetInt("weaponLevel", weaponLevel);
        PlayerPrefs.SetInt("magicLevel", magicLevel);
        PlayerPrefs.SetInt("currentWeaponType", (int)currentWeaponType);
        PlayerPrefs.SetInt("saberEquipped", saberEquipped ? 1 : 0);
        PlayerPrefs.SetInt("katanaEquipped", katanaEquipped ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        coins = PlayerPrefs.GetInt("coins", 0);
        level = PlayerPrefs.GetInt("level", 1);
        if (!PlayerPrefs.HasKey("level"))
            level = 1;
        level = Mathf.Clamp(level, 0, maxLevel);
        xp = PlayerPrefs.GetInt("xp", 0);
        statPoints = PlayerPrefs.GetInt("statPoints", 0);
        meleeLevel = Mathf.Max(1, PlayerPrefs.GetInt("meleeLevel", 1));
        defenseLevel = Mathf.Max(1, PlayerPrefs.GetInt("defenseLevel", 1));
        swordLevel = Mathf.Max(1, PlayerPrefs.GetInt("swordLevel", 1));
        weaponLevel = Mathf.Max(1, PlayerPrefs.GetInt("weaponLevel", 1));
        magicLevel = Mathf.Max(1, PlayerPrefs.GetInt("magicLevel", 1));
        currentWeaponType = (WeaponType)PlayerPrefs.GetInt("currentWeaponType", (int)WeaponType.CombatStyle);
        saberEquipped = PlayerPrefs.GetInt("saberEquipped", 0) == 1 && PlayerPrefs.GetInt(SaberShop.OwnedKey, 0) == 1;
        katanaEquipped = PlayerPrefs.GetInt("katanaEquipped", 0) == 1 && PlayerPrefs.GetInt(SaberShop.KatanaOwnedKey, 0) == 1;
        if (saberEquipped)
            currentWeaponType = WeaponType.Sword;
        if (katanaEquipped)
            currentWeaponType = WeaponType.Sword;
        if (!IsSwordOwned())
        {
            saberEquipped = false;
            katanaEquipped = false;
            currentWeaponType = WeaponType.CombatStyle;
        }
    }
}
