using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public PlayerController Controller { private get; set; }
    public readonly Dictionary<Item, ItemStack> inventory = new();
    public readonly List<InteractableItem> hotBar = new();
    private int hotBarIndex=0;
    public InteractableItem CurrentItem
    {
        get
        {
            if (hotBar == null || hotBar.Count <= 0) return null;
            return hotBar[hotBarIndex];
        }
    }

    public void ScrollItem(int direction)
    {
        if (hotBar.Count > 0)
        {
            hotBarIndex = (hotBarIndex + direction + hotBar.Count) % hotBar.Count;
            // TODO: Atualizar UI da Hotbar
        }
    }

    public void CatchItem (Item newItem)
    {
        Debug.Log("Adquirindo item novo: " + newItem.itemName);
        newItem.PlaySound();
        if (inventory.ContainsKey(newItem))
        {
            inventory[newItem].AddItem(1);
        }
        else
        {
            inventory.Add(newItem, new(newItem, 1));
        }

        if (newItem is not InteractableItem) return;
        if (!hotBar.Contains(newItem as InteractableItem))
        {
            hotBar.Add(newItem as InteractableItem);
        }
    }

    public void UseSelectedItem ()
    {
        if (CurrentItem == null) return; 

        if (CurrentItem.TryUseItem(Controller))
        {
            Item usedItem = CurrentItem;
            inventory[usedItem].RemoveItem(1);
            
            if (inventory[usedItem].amount == 0)
            {
                inventory.Remove(usedItem);
                hotBar.Remove(usedItem as InteractableItem);
                if (hotBar.Count > 0)
                    hotBarIndex %= hotBar.Count;
                else
                    hotBarIndex = 0; 
            }
        }
    }
}