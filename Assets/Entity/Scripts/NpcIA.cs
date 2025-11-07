using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum NpcState
{
    PATROL,
    ATTENTION,
    FOLLOW,
    CAPTURING,
    STUNNED
}

[RequireComponent(typeof(Rigidbody2D))]
public class NpcIA : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_1 = new(0.1f);
    private Rigidbody2D rig;
    private PlayerController player;
    private Vector2 lookDirection;
    private TextMeshProUGUI stateTxt;
    private Coroutine attentionRoutine;
    private Vector3 distractionTarget;
    private LegAnimator legAnimator;
    private bool isDistractionFollow = false;

    [Header("General settings")]
    [SerializeField] private NpcState currentState;
    [SerializeField] private LayerMask playerLayer;
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

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        player = FindFirstObjectByType<PlayerController>();
        legAnimator = GetComponentInChildren<LegAnimator>();
        stateTxt = GetComponentInChildren<TextMeshProUGUI>();
        if (legAnimator == null)
        {
            Debug.LogError($"Npc {name} não possui pernas");
        }
        currentState = NpcState.PATROL;
        timer = 0;
    }

    void Start()
    {
        for (int c = 0; c < patrolPointParent.childCount; c++)
            patrolPoints.Add(patrolPointParent.GetChild(c));
    }

    void FixedUpdate()
    {
        Vector2 currentMoveDirection = rig.linearVelocity.normalized;
        legAnimator.SetMovementState(rig.linearVelocity.magnitude);
        stateTxt.text = currentState.ToString();
        switch (currentState)
        {
            case NpcState.PATROL:
                HandlePatrol();
                break;
            case NpcState.ATTENTION:
                HandleAtention();
                break;
            case NpcState.FOLLOW:
                HandleFollow();
                break;
            case NpcState.CAPTURING:
                Debug.Log("Capturou o jogador!");
                player.Die();
                currentState = NpcState.PATROL;
                break;
            case NpcState.STUNNED:
                break;
            default:
                Debug.LogWarning($"Estado não suportado: {currentState}");
                break;
        }

        if (rig.linearVelocity.sqrMagnitude > 0.01f) 
        {
            Vector2 targetDirection = lookDirection.normalized;
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            float rotationSpeed = 10f;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void HandlePatrol()
    {
        if (CanSeePlayer())
        {
            currentState = NpcState.ATTENTION;
            isDistractionFollow = false; 

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
        
        if (CanSeePlayer())
            currentState = NpcState.FOLLOW;
        else
            currentState = NpcState.PATROL;
        
        attentionRoutine = null;
    }

    private void HandleFollow()
    {
        var hit = Physics2D.OverlapCircleAll((Vector2)transform.position + 0.3f*lookDirection.normalized, 0.3f, playerLayer);
        if (hit != null)
        {
            currentState = NpcState.CAPTURING;
            return;
        }
        if (CanSeePlayer())
        {
            isDistractionFollow = false;
        }

        Vector3 currentTarget;
        if (isDistractionFollow)
        {
            currentTarget = distractionTarget;
        }
        else
        {
            if (Vector3.Distance(transform.position, player.transform.position) < destinationTolerance * 2f)
            {
                currentState = NpcState.CAPTURING;
                rig.linearVelocity = Vector2.zero;
                return;
            }
            currentTarget = player.transform.position;
        }
        
        if (isDistractionFollow && Vector3.Distance(transform.position, currentTarget) < 3*destinationTolerance)
        {
            currentState = NpcState.ATTENTION;
            rig.linearVelocity = Vector2.zero;
            return;
        }

        
        if (!isDistractionFollow) 
        {            
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
        }
        else
        {
            if (currentPath == null)
            {
                TryRecalculatePath(distractionTarget);
            }
        }
        
        FollowCurrentPath(followMoveSpeed);
    }

    public void BecomeStunned(float stunTime)
    {
        currentState = NpcState.STUNNED;
        StopAllCoroutines();
        StartCoroutine(StunCoroutine(stunTime));
    }
    
    private IEnumerator StunCoroutine(float stunTime)
    {
        float counter = 0;
        while (counter <= stunTime)
        {
            rig.linearVelocity = Vector2.zero;
            counter += 0.1f;
            yield return _waitForSeconds0_1;
        }
        currentState = NpcState.PATROL;
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
    
    public void HearDistraction(Vector3 soundPosition)
    {
        if (currentState == NpcState.STUNNED || currentState == NpcState.CAPTURING || currentState == NpcState.FOLLOW)
        {
            return;
        }
        Debug.Log($"NPC {gameObject.name} ouviu uma distração em " + soundPosition);
        distractionTarget = soundPosition;
        isDistractionFollow = true; 
        
        currentState = NpcState.FOLLOW;
        rig.linearVelocity = Vector2.zero;

        if (attentionRoutine != null)
        {
            StopCoroutine(attentionRoutine);
            attentionRoutine = null;
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
    
    private void OnDrawGizmosSelected()
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
