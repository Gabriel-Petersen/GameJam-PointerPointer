using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    private LegAnimator legs;
    [HideInInspector] public PlayerInventory inventory;
    private Rigidbody2D rig;

    private Vector2 moveDir;

    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private float interactionRadius;
    [SerializeField] private float interactionOffset;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private int maxEvilQuantity;
    [HideInInspector] public float speedBonus;

    public bool inInventory;
    public int totalEvilness;
    public int MaxEvil { get; private set; }
    public int totalScore;
    public float MoveSpeed
    {
        get
        {
            return Mathf.Max(0, baseMoveSpeed + speedBonus);
        }
    }
    void Awake()
    {
        input = GetComponent<PlayerInput>();
        rig = GetComponent<Rigidbody2D>();
        inventory = GetComponent<PlayerInventory>();
        inventory.Controller = this;
        totalEvilness = totalScore = 0;
        speedBonus = 0;
        MaxEvil = maxEvilQuantity;

        legs = GetComponentInChildren<LegAnimator>();
        if (legs == null)
        {
            Debug.LogError("Player não tem pernas");
        }
    }

    void OnEnable()
    {
        input.actions["Move"].performed += x => moveDir = x.ReadValue<Vector2>();
        input.actions["Move"].canceled += x => moveDir = Vector2.zero;
        input.actions["Fire"].performed += _ => inventory.UseSelectedItem();
        input.actions["Interact"].performed += _ => Interact();
        input.actions.Enable();
    }

    void OnDisable()
    {
        input.actions["Move"].performed -= x => moveDir = x.ReadValue<Vector2>();
        input.actions["Move"].canceled -= x => moveDir = Vector2.zero;
        input.actions["Fire"].performed -= _ => inventory.UseSelectedItem();
        input.actions["Interact"].performed -= _ => Interact();
        input.actions.Disable();
    }

    private void Interact ()
    {
        if (inInventory) return;
        Vector2 interactionPoint = (Vector2)transform.position + GetMouseDir().normalized * interactionOffset;
        var hit = Physics2D.OverlapCircleAll(interactionPoint, interactionRadius, interactionLayer);
        foreach (var obj in hit)
        {
            if (obj.TryGetComponent(out IInteractable component))
            {
                component.Interact(this);
            }
            else
            {
                Debug.LogWarning($"Objeto {obj.gameObject.name} está na layer interagível mas não possui script de interação");
                continue;
            }
        }
    }

    public Vector2 GetMouseDir()
    {
        Vector3 mpos = Input.mousePosition;
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mpos);
        worldPoint.z = 0f;
        return worldPoint - transform.position;
    }

    public void StartSpeedBonus(float bonus, float buffTime)
    {
        speedBonus += bonus;
        Debug.Log("Speed bonus aplicado! Velocidade atual igual a " + MoveSpeed);
        StartCoroutine(SpeedBonusLoop(bonus, buffTime));
    }

    private IEnumerator SpeedBonusLoop(float bonus, float buffTime)
    {
        yield return new WaitForSeconds(buffTime);
        speedBonus -= bonus;
        Debug.Log("Speed bonus encerrado. Velocidade atual igual a " + MoveSpeed);
    }

    void Update()
    {
        if (inInventory)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                inventory.Close();
                inInventory = false;
            }
            return;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            inventory.Open();
            inInventory = true;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (inventory == null) return; 

        if (scroll > 0f)
        {
            inventory.ScrollItem(-1);
        }
        else if (scroll < 0f)
        {
            inventory.ScrollItem(1);
        }
    }

    void FixedUpdate()
    {
        if (inInventory) return;
        rig.linearVelocity = MoveSpeed * moveDir;
        Vector2 mouseDir = GetMouseDir();
        
        if (legs != null)
        {
            legs.SetMovementState(rig.linearVelocity.magnitude);
        }
        legs.SetMovementState(rig.linearVelocity.magnitude); 

        float angle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        float rotationSpeed = 15f;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)transform.position + GetMouseDir().normalized * interactionOffset, interactionRadius);
    }
}
