using UnityEngine;

public enum AlterationType
{
    Weakened,
    Strength,
    Vulnerable,
    Provocked,
    Hindered,
    Shield,
    Thorn,
    Lucky,
    Unlucky,
    Stunned,
    Sturdy,
    Poisoned,
    Slowed,
    Skulled,
    Parry,
    Fear
}

[CreateAssetMenu(fileName = "AlterationData", menuName = "Scriptable Objects/AlterationData")]
public class AlterationData : ScriptableObject
{
    public string alterationName;
    public string alterationDescription;
    public Sprite alterationIcon;

    public AlterationType alterationType;

    public bool isPositive;
    public bool isInfinite;
    public bool isStackable;
    public bool decrementOnTurnStart;
    public int duration;
    public float strength;
}
