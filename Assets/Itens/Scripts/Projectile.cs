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
        rig.linearDamping = 0f;
        accumulatedDistance = 0f;
        noiseRadius = nr;
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
            if (hit.gameObject.TryGetComponent(out NpcIA npc))
            {
                npc.HearDistraction(transform.position); 
            }
        }
    }

    void FixedUpdate()
    {
        if (!inMoviment) 
        {
            rig.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 deceleration = -rig.linearVelocity.normalized * decelerationRate;
        rig.AddForce(rig.mass * Time.fixedDeltaTime * deceleration, ForceMode2D.Force);

        if (rig.linearVelocity.sqrMagnitude < 0.01f)
        {
            rig.linearVelocity = Vector2.zero;
            inMoviment = false; 
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
            
            Vector2 incomingDirection = rig.linearVelocity.normalized; 
            Vector2 surfaceNormal = collision.GetContact(0).normal; 
            Vector2 reflectedDirection = Vector2.Reflect(incomingDirection, surfaceNormal);

            float bounceSpeed = rig.linearVelocity.magnitude * 0.85f; 
            rig.linearVelocity = reflectedDirection * bounceSpeed;
        }
    }
}