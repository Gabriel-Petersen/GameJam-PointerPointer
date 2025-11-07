using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public HudController hudController;
    public HotBarController hotBarController;
    public InventoryManager inventoryController;
    [SerializeField] private AudioClip throwSound;
    public PlayerController Controller { get; set; }
    public readonly Dictionary<Item, ItemStack> inventory = new();
    public readonly List<InteractableItem> hotBar = new();
    [HideInInspector] public int hotBarIndex=0;
    public InteractableItem CurrentItem
    {
        get
        {
            if (hotBar == null || hotBar.Count <= 0) return null;
            return hotBar[hotBarIndex];
        }
    }

    public void Open()
    {
        hudController.gameObject.SetActive(false);
        inventoryController.gameObject.SetActive(true);
        inventoryController.OpenInventory();
    }
    
    public void Close()
    {
        inventoryController.CloseInventory();
        inventoryController.gameObject.SetActive(false);
        hudController.gameObject.SetActive(true);
    }

    public void ScrollItem(int direction)
    {
        if (hotBar.Count > 0)
        {
            hotBarIndex = (hotBarIndex + direction + hotBar.Count) % hotBar.Count;
            hotBarController.UpdateCurrentItem();
        }
    }

    public void CatchItem (Item newItem)
    {
        //Debug.Log("Adquirindo item novo: " + newItem.itemName);
        AudioSource.PlayClipAtPoint(throwSound, transform.position);
        newItem.PlaySound(Controller);
        if (inventory.ContainsKey(newItem))
        {
            inventory[newItem].AddItem(1);
        }
        else
        {
            inventory.Add(newItem, new(newItem, 1));
        }

        if (newItem is InteractableItem)
        {
            if (!hotBar.Contains(newItem as InteractableItem))
            {
                hotBar.Add(newItem as InteractableItem);
                hotBarController.UpdateHotBar();
            }
        }

        if (newItem is StaticItem)
        {
            var item = newItem as StaticItem;
            Controller.totalEvilness += item.evilness;
            Controller.speedBonus -= item.wheight / 15.0f;
            Controller.totalScore += item.scoreValue;
        }
    }

    public void UseSelectedItem ()
    {
        if (Controller.inInventory || !Controller.alive) return;
        if (CurrentItem == null) return; 

        if (CurrentItem.TryUseItem(Controller))
        {
            AudioSource.PlayClipAtPoint(throwSound, transform.position);
            Controller.animator.SetTrigger("throw");
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
            hotBarController.UpdateHotBar();
        }
    }
}