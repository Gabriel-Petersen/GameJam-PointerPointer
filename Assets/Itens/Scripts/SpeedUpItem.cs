using UnityEngine;

[CreateAssetMenu(fileName = "NewSpeedupItem", menuName = "Item/Interactable/SpeedUp")]
public class SpeedUpItem : InteractableItem
{
    public float speedBonus;
    public float speedupTime;
    public override bool TryUseItem(PlayerController player)
    {
        player.StartSpeedBonus(speedBonus, speedupTime);
        return true;
    }
}