using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth singleton;
    public float currentHealth;
    public float maxHealth = 100f;
    public bool isDead = false;

    private void Awake()
    {
        singleton = this;
    }
    void Start()
    {
        singleton = this;
        currentHealth = maxHealth;
    }

    
    void Update()
    {
        if(currentHealth < 0)
        {
            currentHealth = 0;
        }
    }

    public void PlayerDamage(float damage)
    {
        if(currentHealth > 0)
        {
            currentHealth -= damage;
        }
        else
        {
            Dead();
        }
    }

    void Dead()
    {
        currentHealth = 0;
        isDead = true;
        Debug.Log("Player Is Dead");
    }
}
