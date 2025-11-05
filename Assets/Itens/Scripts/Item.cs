using UnityEngine;

public abstract class Item : ScriptableObject
{
    public float evilness;
    public bool causesSoundAlert;
    public float soundAlertRadius;
    public AudioClip soundAlert;
    public int maxStack;

    [TextArea]
    public string description;
    public string itemName;
    public Sprite icon;

    public abstract void PlaySound();
    public static void PlayAllertSound (Item item)
    {
        if (item.causesSoundAlert && item.soundAlert != null)
        {
            Debug.Log($"{item.itemName} fez barulho!");
            // TODO: fazer o som tocar no jogo
        }
        else if (item.causesSoundAlert && item.soundAlert == null)
        {
            Debug.LogWarning($"{item.itemName} era para causar som mas não há arquivo de áudio anexado");
        }
        else if (!item.causesSoundAlert && item.soundAlert != null)
        {
            Debug.LogWarning($"{item.itemName} possui arquivo de áudio anexado, mas não devia causar som");
        }
    }
}

public abstract class InteractableItem : Item
{
    public PlayerController player;
    public abstract void UseItem();
}

[System.Serializable]
public class ItemStack
{
    public Item item;
    public int amount;
    private readonly bool isNull;
    public ItemStack(Item i, int q, bool isNull = false)
    {
        item = i;
        amount = q;
        this.isNull = isNull;
    }

    public void AddItem(int qtd)
    {
        if (!isNull)
            amount = (amount + qtd > item.maxStack) ? item.maxStack : amount + qtd;
    }

    public void RemoveItem(int qtd)
    {
        if (!isNull)
            amount = (amount - qtd < 0) ? 0 : amount - qtd;
    }

    public static bool IsValid(ItemStack itemStack)
    {
        if (itemStack == null)
            return false;
        if (itemStack.item == null)
            return false;
        if (itemStack.isNull)
        {
            Debug.LogWarning($"ItemStack {itemStack.item.name} is marked as null");
            return false;
        }
        else
            return true;
    }
}
