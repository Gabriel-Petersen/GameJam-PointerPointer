using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    private PlayerInventory inventory;
    private readonly List<InventoryItemEntry> itemgrid = new();


    [SerializeField] private InventoryItemEntry prefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private Image descIcon;
    [SerializeField] private TextMeshProUGUI descItemName;
    [SerializeField] private TextMeshProUGUI descDescription;
    [SerializeField] private Button discardButton;
    void Start()
    {
        inventory = FindAnyObjectByType<PlayerController>().inventory;
    }

    private void Build()
    {
        foreach (var obj in inventory.inventory)
        {
            var newObj = Instantiate(prefab, gridParent);
            newObj.SetupEntry(this, obj.Value, discardButton);
            itemgrid.Add(newObj);
            UpdateDescriptor(obj.Key);
        }
    }

    public void OpenInventory()
    {
        Invoke(nameof(Build), 0.1f);
    }
    
    public void CloseInventory()
    {
        int qtd = itemgrid.Count;
        for (int i = 0; i < qtd; i++)
        {
            Destroy(itemgrid[i].gameObject);
        }
        itemgrid.Clear();
    }

    public void UpdateDescriptor(Item item)
    {
        descIcon.sprite = item.icon;
        descDescription.text = item.description;
        descItemName.text = item.itemName;
    }

    public void Discard(Item item)
    {
        inventory.inventory[item].RemoveItem(1);
        var stItem = item as StaticItem;
        if (stItem != null) inventory.Controller.speedBonus += stItem.wheight / 15.0f;
        if (inventory.inventory[item].amount == 0) inventory.inventory.Remove(item);

        CloseInventory();
        Build();
    }
}
