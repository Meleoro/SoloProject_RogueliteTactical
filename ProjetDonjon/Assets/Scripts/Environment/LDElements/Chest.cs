using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utilities;

public class Chest : MonoBehaviour, IInteractible
{
    enum ChestType
    {
        Normal, 
        Challenge, 
        Trial
    }

    [Header("Parameters")]
    [SerializeField] private Loot lootPrefab;
    [SerializeField] private Coin coinPrefab;
    [SerializeField] private Relic relicPrefab;
    [SerializeField] private ChestType chestType;
    [SerializeField] private float displayedDitherValue;
    [SerializeField] private float hiddenDitherValue;
    [SerializeField] private bool activatesChallenge;

    [Header("Private Infos")]
    private bool isOpened;
    private bool isActivated;
    private PossibleLootData[] possibleLoots;

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _shadowSpriteRenderer;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Light2D _chestLight;
    [SerializeField] private Room _activatedRoom;
    [SerializeField] private Trial _activatedTrial;
    [SerializeField] private ParticleSystem _chestVFX;


    private void Start()
    {
        _spriteRenderer.material.SetVector("_TextureSize", new Vector2(_spriteRenderer.sprite.texture.width, _spriteRenderer.sprite.texture.height));
        possibleLoots = ProceduralGenerationManager.Instance.EnviroData.lootPerFloors[ProceduralGenerationManager.Instance.CurrentFloor].chestPossibleLoots;
    }


    private void GenerateLoot()
    {
        switch(chestType)
        {
            case ChestType.Normal:
                LootManager.Instance.SpawnLootChest(transform.position);
                break;

            case ChestType.Challenge:
                LootManager.Instance.SpawnLootChallengeEnd(transform.position);
                break;

            case ChestType.Trial:
                LootManager.Instance.SpawnLootTrialEnd(transform.position);
                break;
        }
    }


    private IEnumerator InteractCoroutine(float openDuration)
    {
        CameraManager.Instance.FocusOnTransform(transform, 3f);
        GameManager.Instance.OpenChest();

        transform.UShakePosition(openDuration * 0.3f, 0.2f, 0.04f);

        yield return new WaitForSeconds(openDuration * 0.3f);

        _animator.SetTrigger("Open");

        yield return new WaitForSeconds(openDuration * 0.25f);

        // Relic Spawn
        RelicData relicData = RelicsManager.Instance.TryRelicSpawn(RelicSpawnType.NormalChestSpawn,
            ProceduralGenerationManager.Instance.CurrentFloor, 0);
        if (relicData != null)
        {
            Relic newRelic = Instantiate(relicPrefab, transform.position, Quaternion.Euler(0, 0, 0));
            newRelic.Initialise(relicData);
        }

        GenerateLoot();
        _chestLight.ULerpIntensity(openDuration * 0.03f, 3f);

        yield return new WaitForSeconds(openDuration * 0.05f);

        _chestLight.ULerpIntensity(openDuration * 0.05f, 0f);
        _spriteRenderer.material.DOColor(new Color(1, 1, 1, 0), "_Color", 0.2f);

        yield return new WaitForSeconds(0.2f);

        Destroy(gameObject);
    }

    public void Show()
    {
        _spriteRenderer.material.ULerpMaterialFloat(0.5f, displayedDitherValue, "_DitherProgress");

        _shadowSpriteRenderer.enabled = true;

        if(chestType == ChestType.Challenge)
            BattleManager.Instance.OnBattleEnd -= Show;

        StartCoroutine(InteractCoroutine(1.5f));
    }


    public void Hide()
    {
        _spriteRenderer.material.ULerpMaterialFloat(0.5f, hiddenDitherValue, "_DitherProgress");
        _spriteRenderer.material.SetFloat("_OutlineSize", 0.2f);

        if(_chestVFX) _chestVFX.Stop();
        _shadowSpriteRenderer.enabled = false;
    }


    #region Interface Functions

    public void CanBePicked()
    {
        if (isOpened) return;
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 1f, "_OutlineSize");
    }

    public void CannotBePicked()
    {
        if (isOpened) return;
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact()
    {
        if (isOpened || isActivated) return;

        if (chestType == ChestType.Challenge)
        {
            isActivated = true;

            _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
            _collider.enabled = false;

            _activatedRoom.StartBattle(true);
            BattleManager.Instance.OnBattleEnd += Show;

            Hide();
        }

        else if(chestType == ChestType.Trial)
        {
            isActivated = true;

            _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
            _collider.enabled = false;

            _activatedTrial.StartTrial();
            _activatedTrial.OnTrialEnd += Show;

            Hide();
        }

        else
        {
            isOpened = true;
            _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
            _collider.enabled = false;

            StartCoroutine(InteractCoroutine(1.5f));
        }
    }

    #endregion
}
