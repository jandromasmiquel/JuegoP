using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool destroyOnDeath;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> Changed;
    public event Action Died;
    
    // NUEVO: Evento específico para cuando se recibe daño (lo usará la UI y la cámara)
    public event Action Damaged; 

    private void Awake()
    {
        CurrentHealth = maxHealth;
        Changed?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        Changed?.Invoke(CurrentHealth, maxHealth);
        
        // NUEVO: Avisamos a los que estén escuchando de que nos han pegado un golpe
        Damaged?.Invoke(); 

        if (IsDead)
        {
            Died?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        Changed?.Invoke(CurrentHealth, maxHealth);
    }
}