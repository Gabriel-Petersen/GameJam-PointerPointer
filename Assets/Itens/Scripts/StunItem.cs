using UnityEngine;

[CreateAssetMenu(fileName = "NewStunnableItem", menuName = "Item/Interactable/StunnableItem")]
public class StunItem : InteractableItem
{
    [SerializeField] private float actionRadius;
    [SerializeField] private float stunTime;
    [SerializeField] private float offset;
    [SerializeField] private LayerMask npcLayer;

    public override bool TryUseItem(PlayerController player)
    {
        Vector2 interactionPoint = (Vector2)player.transform.position + player.GetMouseDir().normalized * offset;
        var hit = Physics2D.OverlapCircle(interactionPoint, actionRadius, npcLayer);
        if (hit == null) return false;
        if (hit.gameObject.TryGetComponent(out NpcIA npc))
        {
            npc.BecomeStunned(stunTime);
        }
        else
        {
            Debug.LogWarning($"Objeto {hit.gameObject.name} está na tag de inimigos mas não possui script");
            return false;
        }
        return true;
    }
}
