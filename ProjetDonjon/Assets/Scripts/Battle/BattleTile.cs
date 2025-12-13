using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;



public enum BattleTileState
{
    None,
    Move,
    Attack,
    Danger
}

public class BattleTile : MonoBehaviour
{
    [Header("Base Parameters")]
    [SerializeField] private Color baseTileColorOutline;
    [SerializeField] private Color moveTileColorOutline;
    [SerializeField] private Color attackTileColorOutline;
    [SerializeField] private Color dangerTileColorOutline;
    [SerializeField] private Color baseTileColorBack;
    [SerializeField] private Color moveTileColorBack;
    [SerializeField] private Color attackTileColorBack;
    [SerializeField] private Color dangerTileColorBack;

    [Header("Overlay Parameters")]
    [SerializeField] private float overlayEffectDuration;
    [SerializeField] private Color addedColorOverlayOutline;
    [SerializeField] private Color addedColorOverlayBack;

    [Header("Actions")]
    public Action<int> OnUnitEnterTile; 

    [Header("Private Infos")]
    private Vector2Int tileCoordinates;
    private Unit unitOnTile;
    private Color currentColorOutline;
    private Color currentColorBack;
    private List<BattleTile> tileNeighbors = new List<BattleTile>();
    private BattleTileState currentTileState;
    private BattleTileState saveTileState;
    private Vector3 savePos;
    private Coroutine highlightCoroutine;
    private Coroutine changeStateEffectCoroutine;
    private BattleTile[] highlightedTiles;
    private bool isHole;
    private bool cantStopHere;
    private bool isHovered;

    [Header("Public Infos")]
    public Unit UnitOnTile { get { return unitOnTile; } }
    public List<BattleTile> TileNeighbors { get { return tileNeighbors; } }
    public Vector2Int TileCoordinates { get { return tileCoordinates; } }
    public BattleTileState CurrentTileState { get { return currentTileState; } }
    public bool IsHole { get { return isHole; } }
    public bool CantStopHere { get { return cantStopHere; } }
    public bool IsHovered { get { return isHovered; } }

    [Header("References")]
    [SerializeField] private SpriteRenderer _mainSpriteRenderer;
    [SerializeField] private SpriteRenderer _backSpriteRenderer;
    [SerializeField] private Button _tileButton;
    [SerializeField] private Canvas _buttonCanvas;



    #region Setup

    public void SetupBattleTile(Vector2Int tileCoordinates, bool isHole)
    {
        this.tileCoordinates = tileCoordinates;

        _mainSpriteRenderer.color = baseTileColorOutline;
        _backSpriteRenderer.color = baseTileColorBack;

        currentColorOutline = baseTileColorOutline;
        currentColorBack = baseTileColorBack;

        savePos = transform.position;
        highlightedTiles = new BattleTile[0];

        tileNeighbors = new List<BattleTile>();

        this.isHole = isHole;
    }

    public void AddNeighbor(BattleTile tile)
    {
        tileNeighbors.Add(tile);
    }

    #endregion

    private void LateUpdate()
    {
        saveTileState = BattleTileState.None;
    }


    #region Hide / Show

    public IEnumerator HideTileCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        _mainSpriteRenderer.enabled = false;
        _backSpriteRenderer.enabled = false;
    }

    public IEnumerator ShowTileCoroutine(float delay)
    {
        if (isHole) yield break;

        yield return new WaitForSeconds(delay);

        _mainSpriteRenderer.enabled = true;
        _backSpriteRenderer.enabled = true;
    }

    #endregion


    #region Tile States 

    public void DisplayMoveTile()
    {
        if (currentTileState == BattleTileState.Move) return;
        currentTileState = BattleTileState.Move;

        if (changeStateEffectCoroutine is not null) StopCoroutine(changeStateEffectCoroutine);
        changeStateEffectCoroutine = StartCoroutine(ChangeStateEffect(moveTileColorOutline, moveTileColorBack, true, saveTileState == BattleTileState.Move));
    }

    public void DisplayPossibleAttackTile(bool doBounce)
    {
        if (currentTileState == BattleTileState.Attack) return;
        currentTileState = BattleTileState.Attack;

        if (changeStateEffectCoroutine is not null) StopCoroutine(changeStateEffectCoroutine);
        changeStateEffectCoroutine = StartCoroutine(ChangeStateEffect(attackTileColorOutline, attackTileColorBack, doBounce, saveTileState == BattleTileState.Attack));
    }

    public void DisplayDangerTile()
    {
        if (currentTileState == BattleTileState.Danger) return;
        if (unitOnTile is not null) unitOnTile.DisplaySkillOutline(false);
        currentTileState = BattleTileState.Danger;

        if (changeStateEffectCoroutine is not null) StopCoroutine(changeStateEffectCoroutine);
        changeStateEffectCoroutine = StartCoroutine(ChangeStateEffect(dangerTileColorOutline, dangerTileColorBack, false, saveTileState == BattleTileState.Danger));
    }

    public void DisplayNormalTile()
    {
        if (currentTileState == BattleTileState.None) return;
        if (currentTileState == BattleTileState.Danger && unitOnTile is not null) unitOnTile.HideOutline();
        saveTileState = currentTileState;
        currentTileState = BattleTileState.None;

        if(changeStateEffectCoroutine is not null) StopCoroutine(changeStateEffectCoroutine);
        changeStateEffectCoroutine = StartCoroutine(ChangeStateEffect(baseTileColorOutline, baseTileColorBack, false, true));
    }

    private IEnumerator ChangeStateEffect(Color outlineColor, Color backColor, bool doBounce = true, bool doInstant = false)
    {
        currentColorOutline = outlineColor;
        currentColorBack = backColor;

        _mainSpriteRenderer.DOComplete();
        _backSpriteRenderer.DOComplete();
        transform.DOComplete();

        StopHighlight();

        if (doInstant)
        {
            _mainSpriteRenderer.color = outlineColor;
            _backSpriteRenderer.color = backColor;

            transform.position = savePos;
            transform.localScale = Vector3.one;

            yield break;
        }

        if (doBounce)
        {
            //yield return new WaitForSeconds(Random.Range(0, 0.05f));

            float randomDelay = 0.1f + Random.Range(-0.03f, 0.03f);
            randomDelay = 0.1f;

            _mainSpriteRenderer.DOColor(outlineColor + Color.white * 0.25f, randomDelay).SetEase(Ease.InOutCubic);
            _backSpriteRenderer.DOColor(backColor + Color.white * 0.25f, randomDelay).SetEase(Ease.InOutCubic);

            transform.DOMove(savePos + new Vector3(0, 0.1f, 0), randomDelay).SetEase(Ease.InOutCubic);
            transform.DOScale(new Vector3(1, Random.Range(1f, 1.15f), 1), randomDelay).SetEase(Ease.InOutCubic);

            yield return new WaitForSeconds(randomDelay + 0.01f);

            transform.DOMove(savePos, randomDelay).SetEase(Ease.InOutCubic);
            transform.DOScale(Vector3.one, randomDelay).SetEase(Ease.InOutCubic);

            _mainSpriteRenderer.DOColor(outlineColor, randomDelay).SetEase(Ease.InOutCubic);
            _backSpriteRenderer.DOColor(backColor, randomDelay).SetEase(Ease.InOutCubic);
        }
        else
        {
            _mainSpriteRenderer.DOColor(outlineColor, 0.15f).SetEase(Ease.InOutCubic);
            _backSpriteRenderer.DOColor(backColor, 0.15f).SetEase(Ease.InOutCubic);

            transform.DOMove(savePos, 0.2f).SetEase(Ease.InOutCubic);
            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutCubic);
        }
    }

    #endregion


    #region Tile Content 

    public void UnitEnterTile(Unit unit, bool isTall)
    {
        unitOnTile = unit;
        OnUnitEnterTile?.Invoke(1);    // For tutorial

        // We prevent unit stoping on the top neighbor if the unit is tall
        if (!isTall) return;
        foreach(BattleTile neighbor in tileNeighbors)
        {
            if (neighbor.TileCoordinates.y <= tileCoordinates.y) continue;

            neighbor.cantStopHere = true;
        }    
    }

    public void UnitLeaveTile()
    {
        unitOnTile = null;

        foreach (BattleTile neighbor in tileNeighbors)
        {
            if (neighbor.TileCoordinates.y <= tileCoordinates.y) continue;

            neighbor.cantStopHere = false;
        }
    }

    #endregion


    #region Mouse Input Functions

    public void HoverTile()
    {
        isHovered = true;

        OverlayTile();
    }

    public void OverlayTile()
    {
        if (currentTileState == BattleTileState.Attack && BattleManager.Instance.CurrentActionType == MenuType.LaunchSkill)
        {
            BattleManager.Instance.TilesManager.DisplayDangerTiles(this, null);
        }
        else 
        {
            // Displays a preview of the possible movement of the tile's current unit if it's possible
            if (unitOnTile is not null && BattleManager.Instance.CurrentUnit is not null && BattleManager.Instance.CurrentActionType != MenuType.LaunchSkill)
            {
                unitOnTile.DisplayOverlayOutline();
                if (unitOnTile.GetType() != typeof(Hero) && BattleManager.Instance.CurrentUnit.GetType() == typeof(Hero))
                {
                    BattleManager.Instance.TilesManager.DisplayPossibleTiles(unitOnTile as AIUnit);
                }
            }
        }

        _mainSpriteRenderer.color = currentColorOutline + addedColorOverlayOutline;
        _backSpriteRenderer.color = currentColorBack + addedColorOverlayBack;

        // We display the path to reach this tile
        if(currentTileState == BattleTileState.Move)
        {
            if (BattleManager.Instance.CurrentUnit.CurrentTile.TileCoordinates == TileCoordinates) return;

            BattleManager.Instance.PathCalculator.ActualisePathCalculatorTiles(BattleManager.Instance.BattleRoom.PlacedBattleTiles);
            Vector2Int[] path = BattleManager.Instance.PathCalculator.GetPath(BattleManager.Instance.CurrentUnit.CurrentTile.TileCoordinates, TileCoordinates, false).ToArray();
            if (path.Length <= 1) return;
            highlightedTiles = new BattleTile[path.Length - 1];

            for(int i = 0; i < path.Length - 1; i++)
            {
                BattleManager.Instance.BattleRoom.PlacedBattleTiles[path[i].x, path[i].y].HighlightMovePathTile();
                highlightedTiles[i] = BattleManager.Instance.BattleRoom.PlacedBattleTiles[path[i].x, path[i].y];
            }
        }
    }

    public void UnhoverTile()
    {
        isHovered = false;

        StartCoroutine(VerifyQuitOverlayTile());
    }

    private IEnumerator VerifyQuitOverlayTile()
    {
        yield return new WaitForEndOfFrame();

        if(unitOnTile && unitOnTile.IsHovered) yield break;

        QuitOverlayTile();
    }

    public void QuitOverlayTile()
    {
        if (currentTileState == BattleTileState.Danger)
        {
            BattleManager.Instance.TilesManager.DisplayPossibleSkillTiles(null, null, false);
        }

        else if (BattleManager.Instance.CurrentActionType != MenuType.LaunchSkill)
        {
            // Hides the preview of the possible movement of the tile's current unit if it's possible
            if (unitOnTile is not null && BattleManager.Instance.CurrentUnit is not null)
            {
                unitOnTile.HideOutline();
                if (unitOnTile.GetType() != typeof(Hero) && BattleManager.Instance.CurrentUnit.GetType() == typeof(Hero))
                {
                    BattleManager.Instance.TilesManager.StopDisplayPossibleMoveTiles();
                }
            }
        }

        if(highlightedTiles.Length > 0)
        {
            for (int i = 0; i < highlightedTiles.Length; i++)
            {
                highlightedTiles[i].StopHighlight();
            }

            highlightedTiles = new BattleTile[0];
        }

        _mainSpriteRenderer.color = currentColorOutline;
        _backSpriteRenderer.color = currentColorBack;
    }

    public async void ClickTile()
    {
        await Task.Delay((int)(Time.deltaTime * 1000));

        if (InputManager.wantsToRightClick) return;

        switch (currentTileState)
        {
            case BattleTileState.Move:
                if (BattleManager.Instance.CurrentActionType != MenuType.Move) break;
                StartCoroutine(BattleManager.Instance.MoveUnitCoroutine(this, false));
                break;

            case BattleTileState.Danger:
                if (BattleManager.Instance.CurrentActionType != MenuType.LaunchSkill) break;
                StartCoroutine(BattleManager.Instance.UseSkillCoroutine(null));
                break;
        }
    }

    #endregion


    #region Other Effects

    int[] originalLayers;
    public void SetToFirstLayer(int newLayer)
    {
        originalLayers = new int[3];
        originalLayers[0] = _mainSpriteRenderer.sortingOrder;
        originalLayers[1] = _backSpriteRenderer.sortingOrder;
        originalLayers[2] = _buttonCanvas.sortingOrder;

        _buttonCanvas.sortingOrder = newLayer;

        _mainSpriteRenderer.sortingOrder = newLayer;
        _backSpriteRenderer.sortingOrder = newLayer;
    }

    public void SetToNormalLayer()
    {
        _mainSpriteRenderer.sortingOrder = originalLayers[0];
        _backSpriteRenderer.sortingOrder = originalLayers[1];
        _buttonCanvas.sortingOrder = originalLayers[2];
    }


    public void HighlightMovePathTile()
    {
        StopHighlight();

        highlightCoroutine = StartCoroutine(HighlightTile(moveTileColorOutline, moveTileColorBack, 0.3f));
    }


    public void HighlightSkillTile()
    {
        StopHighlight();

        highlightCoroutine = StartCoroutine(HighlightTile(attackTileColorOutline, attackTileColorBack, 0.3f));
    }


    public void StopHighlight()
    {
        if (highlightCoroutine is null) return;
        StopCoroutine(highlightCoroutine);

        _mainSpriteRenderer.DOComplete();
        _backSpriteRenderer.DOComplete();

        _mainSpriteRenderer.color = currentColorOutline;
        _backSpriteRenderer.color = currentColorBack;
    }


    private IEnumerator HighlightTile(Color outlineColor, Color backColor, float duration)
    {
        while (true)
        {
            _mainSpriteRenderer.DOColor(outlineColor + new Color(0.02f, 0.02f, 0.02f, 0.2f), duration * 0.98f).SetEase(Ease.InOutCubic);
            _backSpriteRenderer.DOColor(backColor + new Color(0.005f, 0.005f, 0.005f, 0.2f), duration * 0.98f).SetEase(Ease.InOutCubic);

            yield return new WaitForSeconds(duration);

            _mainSpriteRenderer.DOColor(outlineColor + new Color(0.01f, 0.01f, 0.01f, 0.1f), duration * 0.98f).SetEase(Ease.InOutCubic);
            _backSpriteRenderer.DOColor(backColor + new Color(0.005f, 0.005f, 0.005f, 0.1f), duration * 0.98f).SetEase(Ease.InOutCubic);

            yield return new WaitForSeconds(duration);
        }
    }

    #endregion
}
