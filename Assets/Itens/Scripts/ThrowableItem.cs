using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowableItem", menuName = "Item/Interactable/ThrowableItem")]
public class ThrowableItem : InteractableItem
{
    [Space(5)]
    public float shootOffset;
    public float decelerationRate;
    public bool isCollidable;
    public float speed;
    public float maxDistance;
    public Projectile prefab;
    public AudioClip collisionSound;
    public float noiseRadius;

    public override bool TryUseItem(PlayerController player)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"Objeto {itemName} é arremessável e não possui projétil atrelado");
            return false;
        }

        Vector2 spawnPosition = (Vector2)player.transform.position + player.GetMouseDir().normalized * shootOffset;
        Projectile p = Instantiate(prefab, spawnPosition, Quaternion.identity);
        p.SetupProjectile(isCollidable, speed, decelerationRate, maxDistance, collisionSound, spawnPosition, noiseRadius);
        p.Shoot(player.GetMouseDir().normalized);

        return true;
    }
}