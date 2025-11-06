using UnityEngine;

public interface IInteractable
{
    public void Interact(PlayerController player);
}

public class CatchableItem : MonoBehaviour, IInteractable
{
    [SerializeField] Item itemBase;
    public void Interact(PlayerController player)
    {
        player.inventory.CatchItem(itemBase);
        Destroy(gameObject);
    }
}