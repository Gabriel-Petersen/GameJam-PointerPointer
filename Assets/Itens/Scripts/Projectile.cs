using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask npcLayer;
    private bool isCollidable;
    private bool inMoviment = false;
    private float speed;
    private float decelerationRate;
    private float maxDistance;
    private Vector2 moveDirection;
    private Rigidbody2D rig;
    private AudioClip collisionSound;
    private float accumulatedDistance;
    private float noiseRadius;
    private Vector2 lastPosition;
    public void SetupProjectile(bool col, float s, float deceleration, float md, AudioClip clip, Vector2 origin, float nr)
    {
        inMoviment = false;
        isCollidable = col;
        speed = s;
        decelerationRate = deceleration;
        maxDistance = md;
        collisionSound = clip;
        transform.position = lastPosition = origin;
        rig = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rig.gravityScale = 0f;
        rig.linearDamping = 0f;
        accumulatedDistance = 0f;
        noiseRadius = nr;
        
        rig.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Shoot(Vector2 direction)
    {
        moveDirection = direction;
        inMoviment = true;
        rig.AddForce(moveDirection * speed, ForceMode2D.Impulse);
    }

    private void GenerateSoundAlert()
    {
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
    }

    void FixedUpdate()
    {
        if (rig.linearVelocity == Vector2.zero) accumulatedDistance = 3 * maxDistance;
        if (!inMoviment)
        {
            rig.linearVelocity = Vector2.zero;
            return;
        }
        var rot = transform.rotation.eulerAngles;
        rot.z *= 0.4f;
        transform.rotation = Quaternion.Euler(rot);
        Vector2 deceleration = -rig.linearVelocity.normalized * decelerationRate;
        rig.AddForce(rig.mass * Time.fixedDeltaTime * deceleration, ForceMode2D.Force);

        if (rig.linearVelocity.sqrMagnitude < 0.01f)
        {
            rig.linearVelocity = Vector2.zero;
            accumulatedDistance = 3 * maxDistance;
        }

        float distanceTraveledThisFrame = Vector2.Distance(lastPosition, transform.position);
        accumulatedDistance += distanceTraveledThisFrame;
        
        lastPosition = transform.position;

        if (accumulatedDistance >= maxDistance)
        {
            GenerateSoundAlert();
            Destroy(gameObject); 
            return;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            Destroy(gameObject);
            return;
        }

        if (collisionSound != null) AudioSource.PlayClipAtPoint(collisionSound, transform.position);
        GenerateSoundAlert();
                
        if (!isCollidable)
        {
            inMoviment = false;
            rig.linearVelocity = Vector2.zero;
            Destroy(gameObject);
        }
        else
        {
            lastPosition = collision.GetContact(0).point;
        }
    }
}