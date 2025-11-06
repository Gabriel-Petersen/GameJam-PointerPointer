using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    [HideInInspector] public PlayerInventory inventory;
    private Rigidbody2D rig;

    private Vector2 moveDir;
    private float originalScaleX;

    [SerializeField] private float baseMoveSpeed;
    [SerializeField] private float interactionRadius;
    [SerializeField] private float interactionOffset;
    [SerializeField] private LayerMask interactionLayer;
    [HideInInspector] public float speedBonus;

    public int totalEvilness;
    public int totalScore;
    private float MoveSpeed
    {
        get
        {
            return Mathf.Max(0, baseMoveSpeed + speedBonus);
        }
    }
    void Awake()
    {
        originalScaleX = transform.localScale.x;
        input = GetComponent<PlayerInput>();
        rig = GetComponent<Rigidbody2D>();
        inventory = GetComponent<PlayerInventory>();
        inventory.Controller = this;
        totalEvilness = totalScore = 0;
        speedBonus = 0;
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
        Vector2 interactionPoint = (Vector2)transform.position + GetMouseDir().normalized * interactionOffset;
        var hit = Physics2D.OverlapCircleAll(interactionPoint, interactionRadius, interactionLayer);
        foreach (var obj in hit)
        {
            Debug.Log($"{obj.name} está na área de colisão de interação");
            if (obj.TryGetComponent(out IInteractable component))
            {
                Debug.Log("Interagindo com objeto: " + obj.name);
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

    private IEnumerator SpeedBonusLoop (float bonus, float buffTime)
    {
        yield return new WaitForSeconds(buffTime);
        speedBonus -= bonus;
        Debug.Log("Speed bonus encerrado. Velocidade atual igual a " + MoveSpeed);
    }

    void FixedUpdate()
    {
        rig.linearVelocity = MoveSpeed * moveDir;
        Vector2 mouseDir = GetMouseDir();

        if (mouseDir.x > 0)
            transform.localScale = new Vector3(originalScaleX, transform.localScale.y, transform.localScale.z);
        else if (mouseDir.x < 0)
            transform.localScale = new Vector3(-originalScaleX, transform.localScale.y, transform.localScale.z);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)transform.position + GetMouseDir().normalized * interactionOffset, interactionRadius);
    }
}
