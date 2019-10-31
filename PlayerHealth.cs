using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth singleton;
    public float currentHealth;
    public float maxHealth = 100f;
    public Slider healthSlider;
    public Text healthCounter;
    public bool isDead = false;
    [Header("Damage Screen")]
    public Color damageColor;
    public Image damageImage;
    float colorSmoothing = 6f;
    bool isTakingDamage = false;

    private void Awake()
    {
        singleton = this;
    }
    void Start()
    {
        currentHealth = maxHealth;
        healthSlider.value = maxHealth;
        UpdateHealthUI();
    }

    private void Update()
    {
        if(isTakingDamage)
        {
            damageImage.color = damageColor;
        }
        else
        {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, colorSmoothing * Time.deltaTime);
        }
        isTakingDamage = false;
    }




    public void PlayerDamage(float damage)
    {
        if(currentHealth > 0)
        {
            if (damage >= currentHealth)
            {
                isTakingDamage = true;
                Dead();
            }
            else
            {
                isTakingDamage = true;
                currentHealth -= damage;
            }
            UpdateHealthUI();
        }
    }
    public void AddHealth(float healthAmount)
    {
        currentHealth += healthAmount;
        UpdateHealthUI();
    }

    void Dead()
    {
        currentHealth = 0;
        isDead = true;
        healthSlider.value = 0;
        UpdateHealthUI();
        Debug.Log("Player Is Dead");
    }

    public void UpdateHealthUI()
    {
        healthCounter.text = currentHealth.ToString();
        healthSlider.value = currentHealth;
    }
}
