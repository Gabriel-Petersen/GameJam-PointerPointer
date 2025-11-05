using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    private Rigidbody2D rig;

    private Vector2 moveDir;
    private float originalScaleX;

    [SerializeField] private float baseMoveSpeed;
    [HideInInspector] public float speedBonus;
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
    }

    void OnEnable()
    {
        input.actions["Move"].performed += x => moveDir = x.ReadValue<Vector2>();
        input.actions["Move"].canceled += x => moveDir = Vector2.zero;
        input.actions.Enable();
    }

    void OnDisable()
    {
        input.actions["Move"].performed -= x => moveDir = x.ReadValue<Vector2>();
        input.actions["Move"].canceled -= x => moveDir = Vector2.zero;
        input.actions.Disable();
    }

    Vector2 GetMouseDir()
    {
        var mpos = Mouse.current.position.ReadValue();
        var world = Camera.main.ScreenToWorldPoint(mpos);
        return world - transform.position;
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
}
