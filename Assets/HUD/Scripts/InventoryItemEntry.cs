using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemEntry : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI amountTxt;
    private InventoryManager inventoryManager;
    private Item item;
    public void SetupEntry(InventoryManager manager, ItemStack itemStack)
    {
        item = itemStack.item;
        inventoryManager = manager;
        icon.sprite = itemStack.item.icon;
        itemName.text = itemStack.item.itemName;
        amountTxt.text = itemStack.amount + "x";
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryManager.UpdateDescriptor(item);
    }
}
