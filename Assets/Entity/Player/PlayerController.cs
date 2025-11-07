using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    private Vector2 startPosition;
    public bool alive;
    [HideInInspector] public Animator animator;
    private LegAnimator legs;
    [HideInInspector] public PlayerInventory inventory;
    private Rigidbody2D rig;
    private WaitForSeconds decSecondWait;

    private Vector2 moveDir;

    [SerializeField] private TextMeshProUGUI timerTxt;
    [SerializeField] private float dieCooldown;
    [SerializeField] private float gameTime;
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
            return Mathf.Max(0.5f, baseMoveSpeed + speedBonus);
        }
    }

    [Space(10)]

    [SerializeField] private GameObject endGameObj;
    [SerializeField] private TextMeshProUGUI cashTxt;
    [SerializeField] private TextMeshProUGUI evilTxt;
    void Awake()
    {
        input = GetComponent<PlayerInput>();
        rig = GetComponent<Rigidbody2D>();
        inventory = GetComponent<PlayerInventory>();
        animator = GetComponent<Animator>();
        inventory.Controller = this;
        totalEvilness = totalScore = 0;
        speedBonus = 0;
        MaxEvil = maxEvilQuantity;
        startPosition = transform.position;
        alive = true;
        decSecondWait = new WaitForSeconds(0.1f);

        legs = GetComponentInChildren<LegAnimator>();
        if (legs == null)
        {
            Debug.LogError("Player não tem pernas");
        }
    }

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        float currentTime = gameTime;
        while (currentTime > 0)
        {
            timerTxt.text = currentTime.ToString("F2") + "s";
            currentTime -= 0.1f;
            yield return decSecondWait;
        }
        alive = false;

        inventory.hudController.gameObject.SetActive(false);
        inventory.hotBarController.gameObject.SetActive(false);
        inventory.inventoryController.gameObject.SetActive(false);
        endGameObj.SetActive(true);
        cashTxt.text = $"Dinheiro acumulado: {totalScore}";
        float maldade = 100f * totalEvilness / MaxEvil;
        evilTxt.text = $"Maldade total: {maldade}%";
    }

    public void ToMenu ()
    {
        SceneManager.LoadScene("Menu");
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

    private void Interact()
    {
        if (inInventory || !alive) return;
        animator.SetTrigger("catch");
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

    public void Die()
    {
        alive = false;
        transform.position = startPosition;
        rig.linearVelocity = Vector3.zero;
        Invoke(nameof(ResetPlayer), dieCooldown);
    }
    
    private void ResetPlayer()
    {
        alive = true;
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
        if (!alive) return;
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
        if (inInventory || !alive) return;
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
