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
    void OnDisable()
    {
        item = null;
    }

    public void SetupEntry(InventoryManager manager, ItemStack itemStack, Button btt)
    {
        item = itemStack.item;
        inventoryManager = manager;
        icon.sprite = itemStack.item.icon;
        itemName.text = itemStack.item.itemName;
        amountTxt.text = itemStack.amount + "x";
        btt.onClick.RemoveAllListeners();
        btt.onClick.AddListener(Discard);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryManager.UpdateDescriptor(item);
    }
    
    public void Discard ()
    {
        if (item != null)
            inventoryManager.Discard(item);
    }
}
