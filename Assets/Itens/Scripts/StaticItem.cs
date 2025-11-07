using UnityEngine;

[CreateAssetMenu(fileName = "NewStaticItem", menuName = "Item/StaticItem")]
public class StaticItem : Item
{
    [Space(10)]
    public int scoreValue;
    public int evilness;
    public float wheight;
}