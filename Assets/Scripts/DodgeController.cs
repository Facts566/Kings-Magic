using UnityEngine;
using UnityEngine.UI;

public class DodgeController : MonoBehaviour
{
    public EnemyAttack enemyAttack;
    public Text buttonText;
    public float cooldown = 15f;
    public int energyCost = 25;
    [Range(0f, 100f)] public float dodgeChance = 1f;

    public bool dodgeReady = false;
    public float cooldownLeft = 0f;

    void Start()
    {
        if (enemyAttack == null)
            enemyAttack = FindObjectOfType<EnemyAttack>();

        if (buttonText == null)
            buttonText = GetComponentInChildren<Text>();

        UpdateButtonText();
    }

    void Update()
    {
        if (cooldownLeft > 0f)
        {
            cooldownLeft = Mathf.Max(0f, cooldownLeft - Time.deltaTime);
            UpdateButtonText();
        }
    }

    private void UpdateButtonText()
    {
        if (buttonText == null) return;

        if (cooldownLeft > 0f)
            buttonText.text = "Рывок\n" + Mathf.CeilToInt(cooldownLeft).ToString() + "с";
        else
            buttonText.text = "Рывок\nГотов";
    }

    public void OnDodgePressed()
    {
        if (cooldownLeft > 0f || dodgeReady) return;
        if (enemyAttack == null || !enemyAttack.TrySpendEnergy(energyCost)) return;

        cooldownLeft = cooldown;
        UpdateButtonText();

        if (Random.value * 100f < dodgeChance)
            dodgeReady = true;
    }

    public bool ConsumeDodge()
    {
        if (!dodgeReady) return false;
        dodgeReady = false;
        UpdateButtonText();
        return true;
    }
}
