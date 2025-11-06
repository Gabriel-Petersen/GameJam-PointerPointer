using UnityEngine;
using UnityEngine.InputSystem;

public class DebugItemConfig : MonoBehaviour
{
    [SerializeField] private bool draw;
    enum ItemType { THROWABLE, STUN }
    [SerializeField] private ItemType itemType;

    [Header("Stun Item Settings")]
    [Space(5)]
    [SerializeField] private float actionRadius;
    [SerializeField] private float offset;

    [Header("Throwable Item Settings")]
    [Space(5)]
    [SerializeField] private float throwDistance;

    public void OnDrawGizmos()
    {
        if (!draw || Camera.main == null || Mouse.current == null) return;
        
        Vector3 origin = transform.position;
        
        Vector3 mpos = Input.mousePosition;        
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mpos);
        worldPoint.z = 0f; 
        Vector3 mouseDir = (worldPoint - origin).normalized;
        
        switch (itemType)
        {
            case ItemType.STUN:
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(origin + mouseDir * offset, actionRadius);
                break;
                
            case ItemType.THROWABLE:
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, origin + mouseDir * throwDistance);
                break;
        }
    }

}