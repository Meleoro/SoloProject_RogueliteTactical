using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utilities;

public class Chest : MonoBehaviour, IInteractible
{
    [Header("Parameters")]
    [SerializeField] private Loot lootPrefab;
    [SerializeField] private Coin coinPrefab;
    [SerializeField] private Relic relicPrefab;

    [Header("Private Infos")]
    private bool isOpened;
    private PossibleLootData[] possibleLoots;

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Light2D _chestLight;


    private void Start()
    {
        _spriteRenderer.material.SetVector("_TextureSize", new Vector2(_spriteRenderer.sprite.texture.width, _spriteRenderer.sprite.texture.height));
        possibleLoots = ProceduralGenerationManager.Instance.EnviroData.lootPerFloors[ProceduralGenerationManager.Instance.CurrentFloor].chestPossibleLoots;
    }


    private void GenerateLoot()
    {
        LootManager.Instance.SpawnLootChest(transform.position);
        if(Random.Range(0, 2) == 0) LootManager.Instance.SpawnLootChest(transform.position);    // 50% chance to get second loot

        int pickedCoinsAmount = Random.Range(ProceduralGenerationManager.Instance.EnviroData.lootPerFloors[ProceduralGenerationManager.Instance.CurrentFloor].minChestCoins,
            ProceduralGenerationManager.Instance.EnviroData.lootPerFloors[ProceduralGenerationManager.Instance.CurrentFloor].maxChestCoins);

        for(int i = 0; i < pickedCoinsAmount; i++)
        {
            Coin coin = Instantiate(coinPrefab, transform.position, Quaternion.Euler(0, 0, 0), UIManager.Instance.CoinUI.transform);
            coin.transform.position = transform.position;
        }
    }


    private IEnumerator InteractCoroutine(float openDuration)
    {
        CameraManager.Instance.FocusOnTr(transform, 3f);

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

        yield return new WaitForSeconds(openDuration * 0.03f);

        _chestLight.ULerpIntensity(openDuration * 0.05f, 0f);
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
        if (isOpened) return;

        isOpened = true;
        _spriteRenderer.material.ULerpMaterialFloat(0.1f, 0f, "_OutlineSize");
        _collider.enabled = false;

        StartCoroutine(InteractCoroutine(1.5f));
    }

    #endregion
}
