using UnityEngine;

public enum TutoHighlightType
{
    None,
    HighlightActionPoints,
    HighlightSkillPoints,
    HighlightMoveButton,
    HighlightSkillButton,
    HighlightEnemy,
    HighlightSkullEnemy,
    HighlightChest,
    HighlightCollection,
    HighlightExpedition,
    HighlightCampLevel
}

public enum TutoEndCondition
{
    ClickContinue,
    MoveExplo,
    MoveBattle,
    Attack,
    Interact,
    ClickEnemy,
    AddShield
}


[CreateAssetMenu(fileName = "TutoStepData", menuName = "Scriptable Objects/TutoStepData")]
public class TutoStepData : ScriptableObject
{
    public string stepName;
    [TextArea] public string stepDescription;
    public TutoHighlightType highlightType;
    public TutoEndCondition endCondition;
    public bool leftSidePanel;
    public bool playNextStepOnFinished;
    public float playNextStepDelay = 0.2f;
}
