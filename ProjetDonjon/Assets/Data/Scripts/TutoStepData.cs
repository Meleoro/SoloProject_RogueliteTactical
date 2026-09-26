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
    WaitDuration,
    Jump
}


public enum TutoType
{
    ClassicPanel,
    SmallPanel,
    ScreenPanel
}

public enum LockInputType
{
    DontLock,
    LockHeroInputs,
    LockAllInputs,
    LockAllExceptEquipment,
    LockAllExceptSkills,
    LockAllExceptSkillTrees
}


[CreateAssetMenu(fileName = "TutoStepData", menuName = "Scriptable Objects/TutoStepData")]
public class TutoStepData : ScriptableObject
{
    public string stepName;
    [TextArea] public string stepDescription;
    public TutoHighlightType highlightType;
    public TutoEndCondition endCondition;
    public TutoType tutoType;
    public LockInputType lockInputType;
    public bool leftSidePanel;
    public bool playNextStepOnFinished;
    public bool lockPlayerInputs;
    public float playNextStepDelay = 0.2f;
    public float waitDuration = 0f;
}
