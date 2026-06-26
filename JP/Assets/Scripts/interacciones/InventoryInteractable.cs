using UnityEngine;

[RequireComponent(typeof(InventoryContainer))]
public class InventoryInteractable : Interactable
{
    [SerializeField] private InventoryContainer container;
    [SerializeField] private string title = "Contenedor";

    public InventoryContainer Container => container;
    public string Title => title;

    private void Awake()
    {
        if (container == null)
        {
            container = GetComponent<InventoryContainer>();
        }
    }

    protected override void OnInteract()
    {
        InventoryUIController ui = FindAnyObjectByType<InventoryUIController>();
        if (ui == null)
        {
            Debug.LogWarning("No hay InventoryUIController en la escena.");
            return;
        }

        ui.OpenExternalContainer(container, title);
    }
}
