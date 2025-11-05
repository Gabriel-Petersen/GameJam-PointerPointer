using UnityEngine;

[CreateAssetMenu(fileName = "NewStaticItem", menuName = "Item/StaticItem")]
public class StaticItem : Item
{
    public float scoreValue;
    public float wheight;

    public override void PlaySound()
    {
        PlayAllertSound(this);
    }
}