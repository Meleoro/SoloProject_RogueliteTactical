using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public PlayerInput _playerInput;

    public static Vector2 moveDir;
    public static Vector2 mouseDelta;
    public static Vector2 mousePosition;
    public static float mouseScroll;
    public static bool wantsToJump;
    public static bool isHoldingJump;
    public static bool wantsToInteract;
    public static bool wantsToInventory;
    public static bool wantsToHeroInfo;
    public static bool wantsToSkillTree;
    public static bool wantsToSkills;
    public static bool wantsToRotateLeft;
    public static bool wantsToRotateRight;
    public static bool wantsToReturn;
    public static bool wantsToRightClick;
    public static bool wantsToDrag;

    private void Update()
    {
        moveDir = _playerInput.actions["Move"].ReadValue<Vector2>();
        mouseDelta = _playerInput.actions["MouseDelta"].ReadValue<Vector2>();
        mousePosition = _playerInput.actions["MousePosition"].ReadValue<Vector2>();
        mouseScroll = _playerInput.actions["MouseScroll"].ReadValue<float>();
        wantsToJump = _playerInput.actions["Jump"].WasPressedThisFrame();
        isHoldingJump = _playerInput.actions["Jump"].IsPressed();
        wantsToInteract = _playerInput.actions["Interact"].WasPressedThisFrame();
        wantsToInventory = _playerInput.actions["Inventory"].WasPressedThisFrame();
        wantsToHeroInfo = _playerInput.actions["HeroInfo"].WasPressedThisFrame();
        wantsToSkillTree = _playerInput.actions["SkillTrees"].WasPressedThisFrame();
        wantsToSkills = _playerInput.actions["Skills"].WasPressedThisFrame();
        wantsToRotateLeft = _playerInput.actions["RotateLeft"].WasPressedThisFrame();
        wantsToRotateRight = _playerInput.actions["RotateRight"].WasPressedThisFrame();
        wantsToReturn = _playerInput.actions["Return"].WasPressedThisFrame();
        wantsToRightClick = _playerInput.actions["RightClick"].IsPressed();
        wantsToDrag = _playerInput.actions["Drag"].IsPressed();
    }
}
