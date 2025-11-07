using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LegAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] legFrames;
    [SerializeField] private float frameRate = 12f; 

    private SpriteRenderer legRenderer;
    private float frameTimer;
    private int currentFrameIndex;
    
    private bool isMoving = false; 

    void Awake()
    {
        legRenderer = GetComponent<SpriteRenderer>();
        if (legFrames.Length > 0)
        {
            legRenderer.sprite = legFrames[0];
        }
    }

    void Update()
    {
        if (isMoving && legFrames.Length > 1)
        {
            frameTimer += Time.deltaTime;

            if (frameTimer >= 1f / frameRate)
            {
                currentFrameIndex = (currentFrameIndex + 1) % legFrames.Length;
                legRenderer.sprite = legFrames[currentFrameIndex];
                frameTimer = 0f;
            }
        }
    }
    
    public void SetMovementState(float moveMagnitude)
    {
        isMoving = moveMagnitude > 0.05f; 
        
        if (!isMoving)
        {
            legRenderer.sprite = legFrames[0];
            currentFrameIndex = 0;
            frameTimer = 0f;
        }
    }
}