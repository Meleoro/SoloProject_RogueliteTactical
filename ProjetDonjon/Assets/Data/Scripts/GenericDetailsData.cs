using UnityEngine;

[CreateAssetMenu(fileName = "GenericDetailsData", menuName = "Scriptable Objects/GenericDetailsData")]
public class GenericDetailsData : ScriptableObject
{
    public string name;
    [TextArea] public string description;
}
