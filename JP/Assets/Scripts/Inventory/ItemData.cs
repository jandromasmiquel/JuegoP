using UnityEngine;

[CreateAssetMenu(menuName = "JuegoP/Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private bool stackable;
    [SerializeField] private int maxStack = 1;
    [Header("Animaciones")]
    public AnimatorOverrideController torsoOverride;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public bool Stackable => stackable;
    public int MaxStack => Mathf.Max(1, maxStack);
}
