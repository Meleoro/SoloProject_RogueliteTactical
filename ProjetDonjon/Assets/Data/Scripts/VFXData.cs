using UnityEngine;

[CreateAssetMenu(fileName = "VFXData", menuName = "Scriptable Objects/VFXData")]
public class VFXData : ScriptableObject
{
    public enum VFXAttackType
    {
        OnTargetsVFX,
        OnTilesVFX,
        OnAverageTilesPosVFX
    }

    [Header("VFX Parameters")]
    public VFXAttackType vfxType;
    public Vector2 offset;
    public bool rotateVFX;
    public bool mirrorHorizontalVFX;
    public bool mirrorVerticalVFX;

    [Header("VFX References")]
    public GameObject VFX;
    public GameObject throwedObject;
    public GameObject leftVFX;
    public GameObject rightVFX;
    public GameObject upVFX;
    public GameObject downVFX;
}
