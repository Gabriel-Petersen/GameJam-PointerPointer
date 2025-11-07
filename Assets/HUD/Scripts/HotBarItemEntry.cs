using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotBarItemEntry : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI qtd;
    public void Setup (ItemStack item)
    {
        image.sprite = item.item.icon;
        qtd.text = $"{item.amount}x";
    }
}
