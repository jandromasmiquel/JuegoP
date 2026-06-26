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
    public event Action Damaged; 
    public event Action Healed; // NUEVO: Para enterarnos de las curaciones

    private void Awake()
    {
        CurrentHealth = maxHealth;
        // NOTA: Quitado el Invoke de aquí porque en el Awake los scripts de UI 
        // aún no se han suscrito y este evento se lanzaría al vacío.
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        Changed?.Invoke(CurrentHealth, maxHealth);
        Damaged?.Invoke(); 

        if (IsDead)
        {
            Died?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
        }
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        Changed?.Invoke(CurrentHealth, maxHealth);
        Healed?.Invoke(); // NUEVO: Avisamos de que nos hemos curado
    }
}