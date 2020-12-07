using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    //health
    [SerializeField]
    private float maxHealth;
    [SerializeField]
    private float currentHealth;

    //health bar
    public Slider slider;

    private AudioSource _audioSource;
    public AudioClip enemay_death;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = CalculateHealth();
        if (currentHealth <= 0)
        {
            //Kill Monster
        }
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    //Calculates percentage of health remaining
    private float CalculateHealth()
    {
        return currentHealth / maxHealth;
    }

    public void SetMaxHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
    }

    public void UpdateHealth(float health)
    {
        currentHealth = health;
    }

    /// <summary>
    /// Monster takes damage, if health is 0 they die
    /// </summary>
    /// <param name="damage">Amount of damage taken</param>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHealth(currentHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            _audioSource.PlayOneShot(enemay_death);
            Destroy(gameObject);
        }
    }
}