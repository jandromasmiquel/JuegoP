using UnityEngine;

[CreateAssetMenu(menuName = "JuegoP/Inventory/Weapon Data")]
public class WeaponItemData : ItemData
{
    [Header("Visuales y Lógica en Mano")]
    [SerializeField] private GameObject equippedPrefab; // El prefab que tiene el sprite del bate y el script de ataque

    [Header("Estadísticas de Combate")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float attackRadius = 0.75f;

    // Propiedades públicas para que el Prefab lea los datos desde aquí
    public GameObject EquippedPrefab => equippedPrefab;
    public int Damage => damage;
    public float Cooldown => cooldown;
    public float AttackRadius => attackRadius;
}