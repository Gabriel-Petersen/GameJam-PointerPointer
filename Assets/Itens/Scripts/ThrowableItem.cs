using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowableItem", menuName = "Item/Interactable/ThrowableItem")]
public class ThrowableItem : InteractableItem
{
    public override void PlaySound()
    {
        PlayAllertSound(this);
    }

    public override void UseItem()
    {
        throw new System.NotImplementedException();
    }
}