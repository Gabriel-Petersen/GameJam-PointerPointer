using UnityEngine;

public abstract class Item : ScriptableObject
{
    public bool causesSoundAlert;
    public float soundAlertRadius;
    public AudioClip soundAlert;

    [TextArea]
    public string description;
    public string itemName;
    public Sprite icon;

    public void PlaySound(PlayerController player)
    {
        if (causesSoundAlert && soundAlert != null)
        {
            Debug.Log($"{itemName} fez barulho!");
            AudioSource.PlayClipAtPoint(soundAlert, player.transform.position);
        }
        else if (causesSoundAlert && soundAlert == null)
        {
            Debug.LogWarning($"{itemName} era para causar som mas não há arquivo de áudio anexado");
        }
        else if (!causesSoundAlert && soundAlert != null)
        {
            Debug.LogWarning($"{itemName} possui arquivo de áudio anexado, mas não devia causar som");
        }
    }
}

public abstract class InteractableItem : Item
{
    public abstract bool TryUseItem(PlayerController player);
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
            amount += qtd;
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
