using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NpcState
{
    PATROL,
    ATTENTION,
    FOLLOW,
    CAPTURING
}

[RequireComponent(typeof(Rigidbody2D))]
public class NpcIA : MonoBehaviour
{
    private Rigidbody2D rig;
    private PlayerController player;
    private Vector2 lookDirection;
    private Coroutine attentionRoutine;

    [Header("General settings")]
    [SerializeField] private NpcState currentState;
    [SerializeField] private float attentionTime;


    [Space(10)]
    [Header("Field of view settings")]
    
    [SerializeField] private float visionRadius;
    [SerializeField] private float visionAngle;
    [SerializeField] private bool seeVision;
    [SerializeField] private LayerMask obstacleMask;

    [Space(10)]
    [Header("Moviment settings")]
    [SerializeField] private float patrolMoveSpeed;
    [SerializeField] private float followMoveSpeed;
    [SerializeField] private Transform patrolPointParent;
    [SerializeField] private float destinationTolerance;

    [Space(5)]
    [Header("Recalculation Settings (FOLLOW)")]
    [SerializeField] private float recalculateTime = 0.65f;
    [SerializeField] private float recalculateAngle = 25f;
    
    private float timer; 
    private Vector2 lastPlayerPosition;
    private readonly List<Transform> patrolPoints = new();
    private List<Vector3> currentPath;
    private int currentPathIndex;
    private int currentPatrolPointIndex;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rig = GetComponent<Rigidbody2D>();
        player = FindFirstObjectByType<PlayerController>();
        currentState = NpcState.PATROL;
        timer = 0;
    }

    void Start()
    {
        for (int c = 0; c < patrolPointParent.childCount; c++)
            patrolPoints.Add(patrolPointParent.GetChild(c));
    }

    void Reset()
    {
        player.enabled = true;
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case NpcState.PATROL:
                sr.color = Color.green;
                HandlePatrol();
                break;
            case NpcState.ATTENTION:
                sr.color = Color.yellow;
                HandleAtention();
                break;
            case NpcState.FOLLOW:
                sr.color = Color.red;
                HandleFollow();
                break;
            case NpcState.CAPTURING:
                Debug.Log("Capturou o jogador!");
                player.enabled = false;
                Invoke(nameof(Reset), 5f);
                break;
            default:
                Debug.LogWarning($"Estado não suportado: {currentState}");
                break;
        }
    }

    private void HandlePatrol()
    {
        if (CanSeePlayer())
        {
            currentState = NpcState.ATTENTION;
            attentionRoutine = StartCoroutine(AttentionTimer());
            rig.linearVelocity = Vector2.zero;
            return;
        }

        if (currentPath == null)
        {
            if (patrolPoints.Count == 0) return;
            Vector3 targetWaypoint = patrolPoints[currentPatrolPointIndex].position;
            if (!TryRecalculatePath(targetWaypoint))
            {
                currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Count;
                return;
            }
        }
        
        FollowCurrentPath(patrolMoveSpeed);
        if (currentPath == null)
            currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Count;
    }

    private void HandleAtention()
    {
        if (!CanSeePlayer())
        {
            if (attentionRoutine != null)
            {
                StopCoroutine(attentionRoutine);
                attentionRoutine = null;
            }
            currentState = NpcState.PATROL;
        }
    }

    private IEnumerator AttentionTimer()
    {
        yield return new WaitForSeconds(attentionTime);
        currentState = CanSeePlayer() ? NpcState.FOLLOW : NpcState.PATROL;
        attentionRoutine = null;
    }

    private void HandleFollow()
    {
        if (!CanSeePlayer())
        {
            currentState = NpcState.ATTENTION;
            rig.linearVelocity = Vector2.zero;
            return;
        }

        if (Vector3.Distance(transform.position, player.transform.position) < destinationTolerance * 2f)
        {
            currentState = NpcState.CAPTURING;
            rig.linearVelocity = Vector2.zero;
            return;
        }

        timer += Time.fixedDeltaTime;

        float playerMovementSqr = (player.transform.position - (Vector3)lastPlayerPosition).sqrMagnitude;
        bool playerMovedSignificantly = playerMovementSqr > 0.1f;

        bool angleIsTooWide = false;
        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            Vector2 optimalDirection = GetPlayerVector().normalized;
            Vector2 pathDirection = (currentPath[currentPathIndex] - transform.position).normalized;

            float angleDifference = Vector2.Angle(optimalDirection, pathDirection);
            angleIsTooWide = angleDifference > recalculateAngle;
        }

        bool shouldRecalculate = (currentPath == null) || (timer >= recalculateTime) || (angleIsTooWide && playerMovedSignificantly);
        if (shouldRecalculate)
        {
            if (TryRecalculatePath(player.transform.position))
            {
                timer = 0f;
                lastPlayerPosition = player.transform.position;
            }
        }
        FollowCurrentPath(followMoveSpeed);
    }

    private bool TryRecalculatePath(Vector3 targetPosition)
    {
        currentPath = null;

        if (GridManager.instance.TryGetNode(transform.position, out Node start) &&
            GridManager.instance.TryGetNode(targetPosition, out Node target))
        {
            if (target.isWalkable && PathFinder.TryFindPath(start, target, out currentPath))
            {
                currentPathIndex = 0;
                return true;
            }
        }

        return false;
    }

    private void FollowCurrentPath(float moveSpeed)
    {
        if (currentPath == null || currentPathIndex >= currentPath.Count)
        {
            rig.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 targetPoint = currentPath[currentPathIndex];
        
        Vector2 direction = (targetPoint - transform.position).normalized;
        rig.linearVelocity = direction * moveSpeed;

        lookDirection = direction; 

        if (Vector3.Distance(transform.position, targetPoint) < destinationTolerance)
        {
            currentPathIndex++;
            
            if (currentPathIndex >= currentPath.Count)
            {
                currentPath = null;
                rig.linearVelocity = Vector2.zero;
            }
        }
    }

    private Vector2 GetPlayerVector()
    {
        return player.transform.position - transform.position;
    }

    private bool CanSeePlayer()
    {
        Vector2 playerVector = GetPlayerVector();
        if (playerVector.magnitude > visionRadius) return false;
        float angle = Mathf.Abs(Vector2.SignedAngle(playerVector, lookDirection));
        if (angle > visionAngle / 2) return false;

        Vector2 rayOrigin = (Vector2)transform.position + lookDirection * 0.15f;    
        var hit = Physics2D.Raycast(rayOrigin, playerVector, visionRadius, obstacleMask);
        
        if (hit.collider == null)
            return true; 
        
        if (hit.collider.gameObject == player.gameObject)
            return true;
        
        return false;
    }
    
    private void OnDrawGizmos()
    {
        if (!seeVision) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        float currentAngle = Vector2.SignedAngle(Vector2.right, lookDirection);

        float startAngle = currentAngle + visionAngle / 2f;
        float endAngle = currentAngle - visionAngle / 2f;

        Vector3 startDir = AngleToVector(startAngle);
        Vector3 endDir = AngleToVector(endAngle);

        Vector3 origin = transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + startDir * visionRadius);
        Gizmos.DrawLine(origin, origin + endDir * visionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + (Vector3)lookDirection * (visionRadius / 2f));
    }

    private Vector3 AngleToVector(float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }
}
