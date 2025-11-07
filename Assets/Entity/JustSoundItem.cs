using UnityEngine;

public class JustSoundItem : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private float noiseRadius;
    [SerializeField] private LayerMask npcLayer;
    public void Interact(PlayerController player)
    {
        AudioSource.PlayClipAtPoint(clip, transform.position);
        if (noiseRadius <= 0) return;

        var hitNPCs = Physics2D.OverlapCircleAll(transform.position, noiseRadius, npcLayer);

        foreach (var hit in hitNPCs)
        {
            //Debug.Log($"Npc {hit.gameObject.name} na área do projétil {name}");
            if (hit.gameObject.TryGetComponent(out NpcIA npc))
            {
                npc.HearDistraction(transform.position);
            }
            else
            {
                Debug.LogWarning($"Npc {hit.gameObject.name} está na tag NPC mas não possui componente");
            }
        }

        Destroy(gameObject);
    }
}
