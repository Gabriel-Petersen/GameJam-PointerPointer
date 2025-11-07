using System.Collections.Generic;
using UnityEngine;

public class HotBarController : MonoBehaviour
{
    [SerializeField] private HotBarItemEntry prefab;
    [SerializeField] private Transform highlight;
    private PlayerInventory inventory;
    private readonly List<HotBarItemEntry> hotBarItems = new();

    void Start()
    {
        inventory = FindFirstObjectByType<PlayerController>().inventory;
    }

    public void UpdateCurrentItem () // chama toda vez que movo o scroll
    {
        if (hotBarItems.Count == 0) return;
        highlight.SetParent(hotBarItems[inventory.hotBarIndex].transform);
        highlight.localPosition = Vector3.zero;
    }

    public void UpdateHotBar() // não muito eficiente, mas vai servir
    {
        highlight.gameObject.SetActive(true);
        highlight.SetParent(transform);
        foreach (var o in hotBarItems)
        {
            Destroy(o.gameObject);
        }
        hotBarItems.Clear();
        foreach (var item in inventory.hotBar)
        {
            var newItem = Instantiate(prefab, transform);
            newItem.Setup(inventory.inventory[item]);
            hotBarItems.Add(newItem);
        }
        if (hotBarItems.Count > 0)
            UpdateCurrentItem();
        else
            highlight.gameObject.SetActive(false);
    }
}
