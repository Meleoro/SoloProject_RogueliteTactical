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
    HighlightCampLevel,
    HighlightReturnToCamp,
    HighlightEquipmentMenu,
    HighlightSkillTreeMenu,
    HighlightSkillsMenu
}

public enum TutoEndCondition
{
    ClickContinue,
    MoveExplo,
    MoveBattle,
    Attack,
    Interact,
    ClickEnemy,
    UseSecondSkill,
    ReturnToCamp,
    EquipEquiment,
    PlaceItemInInventory,
    OpenSkillTree,
    OpenSkills,
    WaitDuration
}


public enum TutoType
{
    ClassicPanel,
    SmallPanel
}


[CreateAssetMenu(fileName = "TutoStepData", menuName = "Scriptable Objects/TutoStepData")]
public class TutoStepData : ScriptableObject
{
    public string stepName;
    [TextArea] public string stepDescription;
    public TutoHighlightType highlightType;
    public TutoEndCondition endCondition;
    public TutoType tutoType;
    public bool leftSidePanel;
    public bool playNextStepOnFinished;
    public float playNextStepDelay = 0.2f;
    public float waitDuration = 0f;
}
