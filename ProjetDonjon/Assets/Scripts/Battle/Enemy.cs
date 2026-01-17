using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;

public enum PreviewType
{
    Move,
    Attack,
    MoveAndAttack
}

public class AIUnit : Unit
{
    [Header("Parameters")]
    [SerializeField] private AIData aiData;
    [SerializeField] private AIData aiEliteData;
    [SerializeField] private Loot lootPrefab;
    [SerializeField] private Coin coinPrefab;
    [SerializeField] private bool isBoss;

    [Header("Actions")]
    public Action<int> OnDamageTaken;   // For Tuto

    [Header("Private Infos")]
    private BattleTile[] aimedTiles;
    private BattleTile[] avoidedTiles;
    private SkillData currentSkillData;
    private PreviewType currentPreviewType;
    private int currentSkillIndex;
    private AIData currentData;
    private bool isElite;

    [Header("Public Infos")]
    public AIData AIData { get { return isElite ? aiEliteData : aiData; } }
    public SkillData CurrentSkillData { get { return currentSkillData; } }
    public PreviewType CurrentPreviewType { get { return currentPreviewType; } }
    public bool IsBoss { get { return isBoss; } }

    [Header("Enemy References")]
    [SerializeField] private ParticleSystem _eliteVFX;



    #region Setup

    public void Initialise(bool isElite)
    {
        this.isElite = isElite;
        if (isElite)
        {
            currentData = aiEliteData;
            _eliteVFX.Play();

            _spriteRenderer.material.SetInt("_DisplayEliteEffect", 1);
        }
        else
        {
            currentData = aiData;
        }

        unitData = currentData;
        currentSkillData = currentData.skills[0];
        currentSkillIndex = 0;

        currentPreviewType = PreviewType.Move;

        InitialiseUnitInfos(currentData.baseHealth, currentData.baseStrength, currentData.baseSpeed, currentData.baseLuck, 0);
        StartCoroutine(AppearCoroutine(1f));
    }


    private void SetupAimedTiles()
    {
        if(provocationTarget is not null)
        {
            aimedTiles = new BattleTile[1];
            aimedTiles[0] = provocationTarget.CurrentTile;

            return;
        }

        if (isEnemy)
        {
            if (currentSkillData.skillEffects[0].skillEffectTargetType == SkillEffectTargetType.Allies)
            {
                aimedTiles = new BattleTile[BattleManager.Instance.CurrentEnemies.Count];
                for (int i = 0; i < BattleManager.Instance.CurrentEnemies.Count; i++) {
                    if (BattleManager.Instance.CurrentEnemies[i].CurrentTile == currentTile) continue;
                    aimedTiles[i] = BattleManager.Instance.CurrentEnemies[i].CurrentTile;
                }
            }

            else if (currentSkillData.skillEffects[0].skillEffectTargetType == SkillEffectTargetType.Empty)
            {
                List<BattleTile> temp = new List<BattleTile>();

                foreach(BattleTile tile in BattleManager.Instance.BattleRoom.BattleTiles)
                {
                    if (tile.UnitOnTile is not null) continue;
                    if (tile.IsHole) continue;

                    temp.Add(tile);
                }

                aimedTiles = temp.ToArray();
            }

            else
            {
                aimedTiles = new BattleTile[BattleManager.Instance.CurrentHeroes.Count];
                for (int i = 0; i < BattleManager.Instance.CurrentHeroes.Count; i++) {
                    aimedTiles[i] = BattleManager.Instance.CurrentHeroes[i].CurrentTile;
                }
            }

            avoidedTiles = new BattleTile[BattleManager.Instance.CurrentHeroes.Count];
            for (int i = 0; i < BattleManager.Instance.CurrentHeroes.Count; i++) {
                avoidedTiles[i] = BattleManager.Instance.CurrentHeroes[i].CurrentTile;
            }
        }

        else
        {
            if (currentSkillData.skillEffects[0].skillEffectTargetType == SkillEffectTargetType.Allies)
            {
                aimedTiles = new BattleTile[BattleManager.Instance.CurrentHeroes.Count];
                for (int i = 0; i < BattleManager.Instance.CurrentHeroes.Count; i++) {
                    aimedTiles[i] = BattleManager.Instance.CurrentHeroes[i].CurrentTile;
                }
            }

            else if(currentSkillData.skillEffects[0].skillEffectTargetType == SkillEffectTargetType.Empty)
            {
                aimedTiles = BattleManager.Instance.BattleRoom.BattleTiles.ToArray();
            }

            else
            {
                aimedTiles = new BattleTile[BattleManager.Instance.CurrentEnemies.Count];
                for (int i = 0; i < BattleManager.Instance.CurrentEnemies.Count; i++) {
                    if (BattleManager.Instance.CurrentEnemies[i].CurrentTile == currentTile) continue;
                    aimedTiles[i] = BattleManager.Instance.CurrentEnemies[i].CurrentTile;
                }
            }

            avoidedTiles = new BattleTile[BattleManager.Instance.CurrentEnemies.Count];
            for (int i = 0; i < BattleManager.Instance.CurrentEnemies.Count; i++) {
                avoidedTiles[i] = BattleManager.Instance.CurrentEnemies[i].CurrentTile;
            }
        }
    }

    #endregion


    #region Appear / Disappear

    private IEnumerator AppearCoroutine(float duration)
    {
        _spriteRenderer.material.SetFloat("_DitherProgress", -2);

        _spriteRenderer.material.DOFloat(3.5f, "_DitherProgress", duration).SetEase(Ease.OutFlash);

        yield return new WaitForSeconds(duration);

        _ui.ShowUnitUI();
    }

    private IEnumerator LastEnemyDisappearCoroutine(float duration)
    {
        CameraManager.Instance.FocusOnTransform(transform, 3f);
        transform.UShakePosition(duration * 0.75f, 0.2f, 0.03f);

        BattleManager.Instance.StartBattleEndCutscene();

        yield return new WaitForSeconds(duration * 0.75f);

        BattleManager.Instance.EndBattle();
        BattleManager.Instance.BattleEndRewards(this);

        StartCoroutine(DisappearCoroutine(duration * 0.25f));
    }

    #endregion


    #region AI Functions

    public IEnumerator PlayEnemyTurnCoroutine()
    {
        if (IsStunned)
        {
            EndTurn(0.5f);
            yield break;
        }

        SetupAimedTiles();

        yield return new WaitForSeconds(0.5f);

        BattleTile moveTile = null;
        BattleTile skillTile = null;
        (moveTile, skillTile) = GetBestMove(currentTile);

        // If we can't use any skills on this turn, we look for the next turns where it will be possible
        if(skillTile is null)
        {
            (moveTile, skillTile) = GetBestMove(currentTile, 0, 1);

            if (skillTile is null) {
                (moveTile, skillTile) = GetBestMove(currentTile, 0, 2);
            }

            skillTile = null;
        }


        StartCoroutine(BattleManager.Instance.MoveUnitCoroutine(moveTile, true));

        yield return new WaitForSeconds(0.55f);

        if(skillTile is not null)
        {
            BattleManager.Instance.TilesManager.DisplayDangerTiles(skillTile, currentSkillData);

            yield return new WaitForSeconds(0.5f);

            StartCoroutine(BattleManager.Instance.UseSkillCoroutine(currentSkillData));

            yield return new WaitForSeconds(0.75f * currentSkillData.attackCount);

            if(currentData.skills.Length > 1)
            {
                StartCoroutine(_ui.DoChangePaternEffectCoroutine(1.5f));
                currentSkillIndex = (currentSkillIndex + 1) % currentData.skills.Length;
                currentSkillData = currentData.skills[currentSkillIndex];

                yield return new WaitForSeconds(1.5f);
            }

            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            EndTurn(0f);
        }
    }
    
    private (BattleTile, BattleTile) GetBestMove(BattleTile currentTile, int depth = 0, int maxDepth = 0)
    {
        List<BattleTile> possibleMoves = 
            BattleManager.Instance.TilesManager.GetPaternTiles(currentTile.TileCoordinates, AIData.movePatern, 
                (int)Mathf.Sqrt(AIData.movePatern.Length), true, Enums.ObstacleType.All, null, true);
        possibleMoves.Add(currentTile);

        int bestGrade = -1000;
        BattleTile pickedMoveTile = currentTile;
        BattleTile pickedSkillTile = null;

        if (IsHindered) 
        { 
            possibleMoves.Clear(); 
            possibleMoves.Add(currentTile);
        }

        for (int i = 0; i < possibleMoves.Count; i++)
        {
            if (possibleMoves[i].UnitOnTile is not null)
            {
                if (possibleMoves[i].UnitOnTile != this) continue;
                if (possibleMoves[i].CantStopHere) continue;
            }

            // If we can move
            if(depth < maxDepth)
            {
                BattleTile currentMoveTile = null;
                BattleTile currentSkillTile = null;

                (currentMoveTile, currentSkillTile) = GetBestMove(possibleMoves[i], depth+1, maxDepth);

                // If we didn't find any usable skill tile we skip
                if (currentSkillTile is null) continue;
                if (currentSkillTile.UnitOnTile is null && currentSkillData.skillEffects[0].skillEffectTargetType != SkillEffectTargetType.Empty) continue;
                if (currentSkillTile.UnitOnTile == this && currentSkillData.skillType != SkillType.SkillArea) continue;
                if (!aimedTiles.Contains(currentSkillTile) && currentSkillData.skillType != SkillType.SkillArea) continue;

                // If we can hit a target, we verify if the move has a good enough grade to be selected
                BattleTile[] dangerTiles = GetDangerTiles(currentMoveTile.TileCoordinates, currentSkillTile.TileCoordinates);
                int moveGrade = GetMoveGrade(currentMoveTile.TileCoordinates, dangerTiles, depth);
                if (moveGrade < bestGrade) continue;

                bestGrade = moveGrade;
                pickedMoveTile = possibleMoves[i];
                pickedSkillTile = currentSkillTile;
            }

            // If we need to test skills (max depth)
            else 
            {
                BattleTile[] possibleSkillTiles;

                if (currentSkillData.skillType == SkillType.SkillArea) possibleSkillTiles = new BattleTile[1] { possibleMoves[i] };
                else possibleSkillTiles = BattleManager.Instance.TilesManager.GetPaternTiles(possibleMoves[i].TileCoordinates, currentSkillData.skillPatern,
                        (int)Mathf.Sqrt(currentSkillData.skillPatern.Length), true, Enums.ObstacleType.UnitsIncluded, currentTile).ToArray();

                for (int j = 0; j < possibleSkillTiles.Length; j++)
                {
                    BattleTile[] dangerTiles = GetDangerTiles(possibleMoves[i].TileCoordinates, possibleSkillTiles[j].TileCoordinates);
                    int moveGrade = GetMoveGrade(possibleMoves[i].TileCoordinates, dangerTiles, depth);

                    if (moveGrade <= bestGrade) continue;
                    
                    bestGrade = moveGrade;
                    pickedMoveTile = possibleMoves[i];

                    foreach (var dangerTile in dangerTiles)
                    {
                        if (dangerTile.UnitOnTile is null && pickedMoveTile != dangerTile 
                            && currentSkillData.skillEffects[0].skillEffectTargetType != SkillEffectTargetType.Empty) continue;
                        if (!aimedTiles.Contains(dangerTile) && (pickedMoveTile != dangerTile 
                            || currentSkillData.skillEffects[0].skillEffectTargetType != SkillEffectTargetType.Allies)) continue;

                        pickedSkillTile = possibleSkillTiles[j];
                        break;
                    }
                    
                }
            }
        }

        return (pickedMoveTile, pickedSkillTile);
    }

    private BattleTile[] GetDangerTiles(Vector2Int movePos, Vector2Int skillPos)
    {
        // Skill Grade
        BattleTile[] skillDangerTiles;
        if (currentSkillData.skillType == SkillType.SkillArea)
        {
            skillDangerTiles = BattleManager.Instance.TilesManager.GetPaternTiles(movePos,
                currentSkillData.skillPatern, (int)Mathf.Sqrt(currentSkillData.skillPatern.Length), true, Enums.ObstacleType.Nothing).ToArray();
        }
        else if (currentSkillData.useOrientatedAOE)
        {
            Vector2Int coordinateDif = skillPos - CurrentTile.TileCoordinates;

            if (coordinateDif.y > 0)
                skillDangerTiles = BattleManager.Instance.TilesManager.GetPaternTiles(skillPos, currentSkillData.skillAOEPaternUp,
                    (int)Mathf.Sqrt(currentSkillData.skillAOEPaternUp.Length), false, Enums.ObstacleType.Nothing, null, true).ToArray();

            else if(coordinateDif.y < 0)
                skillDangerTiles = BattleManager.Instance.TilesManager.GetPaternTiles(skillPos, currentSkillData.skillAOEPaternDown,
                    (int)Mathf.Sqrt(currentSkillData.skillAOEPaternDown.Length), false, Enums.ObstacleType.Nothing, null, true).ToArray();

            else if (coordinateDif.x > 0)
                skillDangerTiles = BattleManager.Instance.TilesManager.GetPaternTiles(skillPos, currentSkillData.skillAOEPaternRight,
                    (int)Mathf.Sqrt(currentSkillData.skillAOEPaternRight.Length), false, Enums.ObstacleType.Nothing, null, true).ToArray();

            else
                skillDangerTiles = BattleManager.Instance.TilesManager.GetPaternTiles(skillPos, currentSkillData.skillAOEPaternLeft,
                    (int)Mathf.Sqrt(currentSkillData.skillAOEPaternLeft.Length), false, Enums.ObstacleType.Nothing, null, true).ToArray();
        }
        else
        {
            skillDangerTiles = BattleManager.Instance.TilesManager.GetPaternTiles(skillPos, currentSkillData.skillAOEPatern, 
                (int)Mathf.Sqrt(currentSkillData.skillAOEPatern.Length), true, Enums.ObstacleType.UnitsIncluded).ToArray();
        }

        return skillDangerTiles;
    }

    private int GetMoveGrade(Vector2Int testedMovePos, BattleTile[] testedDangerTiles, int depth)
    {
        int finalGrade = -5 * depth;

        // Move grade
        for (int i = 0; i < avoidedTiles.Length; i++)
        {
            int currentDist = (int)Vector2Int.Distance(testedMovePos, avoidedTiles[i].TileCoordinates);

            switch (AIData.AI)
            {
                case AIType.Classic:
                    finalGrade -= currentDist;
                    break;

                case AIType.Shy:
                    finalGrade += currentDist;
                    break;
            }
        }

        // Skill Grade
        for (int i = 0; i < testedDangerTiles.Length; i++)
        {
            if (!aimedTiles.Contains(testedDangerTiles[i])) continue;

            finalGrade += 10;
        }

        return finalGrade;
    }

    #endregion


    #region Overrides

    protected override async void ClickUnit()
    {
        await Task.Delay((int)(Time.deltaTime * 1000));
        if (InputManager.wantsToRightClick) return;
        if (BattleManager.Instance.IsEnemyTurn || BattleManager.Instance.NoMouseControls) return;

        int newPreviewIndex = ((int)currentPreviewType + 1) % 3;
        currentPreviewType = (PreviewType)newPreviewIndex;

        CurrentTile.ClickTile();
        CurrentTile.OverlayTile();

        StartCoroutine(SquishCoroutine(0.15f));

        OnClickUnit?.Invoke(0);
    }

    protected override void Die()
    {
        currentTile.UnitLeaveTile();
        UnHoverUnit();

        AudioManager.Instance.PlaySoundOneShot(2, 10);
        if(isEnemy) HeroesManager.Instance.SpawnXP(AIData.minXpDrop, transform.position);

        BattleManager.Instance.RemoveUnit(this);

        if ((BattleManager.Instance.CurrentEnemies.Count == 0 || isBoss) && isEnemy)
        {
            StartCoroutine(LastEnemyDisappearCoroutine(2f));
        }
        else
        {
            StartCoroutine(DisappearCoroutine(1f));
        }
    }

    public override void TakeDamage(int damageAmount, Unit originUnit)
    {
        AudioManager.Instance.PlaySoundOneShot(2, 1);

        OnDamageTaken?.Invoke(1);

        base.TakeDamage(damageAmount, originUnit);
    }


    public override void RotateTowardTarget(Transform aimedTr)
    {
        base.RotateTowardTarget(aimedTr);

        if (transform.position.x + 0.2f > aimedTr.position.x) return;

        _spriteRenderer.transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    public override void RotateBackToNormal()
    {
        base.RotateBackToNormal();

        _spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    #endregion
}
