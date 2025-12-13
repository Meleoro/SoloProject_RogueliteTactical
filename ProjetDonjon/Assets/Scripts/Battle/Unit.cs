using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;


public struct AlterationStruct
{
    public AlterationStruct(AlterationData alteration, int duration, float strength, Unit origin)
    {
        this.alteration = alteration;
        this.duration = duration;
        this.currentStrength = strength;
        this.origin = origin;
    }

    public AlterationData alteration;
    public int duration;
    public float currentStrength;
    public Unit origin;
}

public class Unit : MonoBehaviour
{
    [Header("Main parameters")]
    [SerializeField] protected bool isEnemy = true;
    [SerializeField] protected bool isTall;
    [SerializeField][ColorUsage(true, true)] protected Color damageColor;
    [SerializeField][ColorUsage(true, true)] protected Color healColor;
    [SerializeField][ColorUsage(true, true)] protected Color poisonColor;
    [SerializeField][ColorUsage(true, true)] protected Color skullColor;
    [SerializeField][ColorUsage(true, true)] protected Color shieldColor;

    [Header("Actions")]
    public Action OnHeroInfosChange;
    public Action<int> OnAlterationAdded;    // For tuto 

    [Header("Alteration VFXs")]
    [SerializeField] private GameObject buffVFX;
    [SerializeField] private GameObject debuffVFX;
    [SerializeField] private GameObject shieldVFX;
    [SerializeField] private GameObject hinderVFX;
    [SerializeField] private GameObject healVFX;
    [SerializeField] private GameObject stunVFX;
    [SerializeField] private GameObject skullVFX;

    [Header("Outline Color")]
    [SerializeField] protected Color overlayColor = Color.white;
    [SerializeField] protected Color positiveColor = Color.green;
    [SerializeField] protected Color negativeColor = Color.red;
    [SerializeField] protected Color unitsTurnColor = Color.yellow; 
 
    [Header("Private Infos")]
    protected int currentHealth;
    protected int currentMaxHealth;
    protected int currentStrength;
    protected int currentSpeed;
    protected int currentLuck;
    protected int currentMovePoints;
    protected int currentShield;
    protected bool restartTurnOutlineNext;
    protected bool isHovered;

    [Header("Private Stats Modificators")]
    private int strengthModificatorAdditive;
    private int speedModificatorAdditive;
    private int luckModificatorAdditive;
    private int movePointsModificatorAdditive;
    private float critModificatorAdditive;
    private List<AlterationStruct> currentAlterations = new List<AlterationStruct>(); 

    [Header("Protected Other Infos")]
    protected BattleTile currentTile;
    protected BattleTile previousTile;
    protected bool isUnitsTurn;
    protected UnitData unitData;
    protected Unit provocationTarget;
    protected Coroutine squishCoroutine;
    protected Coroutine turnOutlineCoroutine;
    protected PassiveData[] equippedPassives = new PassiveData[3];

    [Header("Public Infos")]
    public int CurrentHealth { get { return currentHealth; } set { currentHealth = value; 
            _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations); 
            OnHeroInfosChange?.Invoke(); } }
    public int CurrentShield { get { return currentShield; } set { currentShield = value; } }
    public int CurrentMaxHealth { get { return currentMaxHealth; } }
    public int CurrentStrength { get { return currentStrength + strengthModificatorAdditive; } }
    public int CurrentSpeed { get { return currentSpeed + speedModificatorAdditive; } }
    public int CurrentLuck { get { return currentLuck + luckModificatorAdditive; } }
    public int CurrentMovePoints { get { return currentMovePoints + movePointsModificatorAdditive; } }
    public float CurrentCritMultiplier { get { return 2 + critModificatorAdditive; } }
    public UnitData UnitData { get { return unitData; } }
    public BattleTile CurrentTile { get { return currentTile; } }
    public PassiveData[] EquippedPassives { get { return equippedPassives; } }
    public bool IsEnemy { get { return isEnemy; } }
    public bool IsHindered { get { return VerifyHasAlteration(AlterationType.Hindered) != null; } }
    public bool IsStunned { get { return VerifyHasAlteration(AlterationType.Stunned) != null; } }
    public bool IsSkulled { get { return VerifyHasAlteration(AlterationType.Skulled) != null; } }
    public bool IsHovered { get { return isHovered; } }

    [Header("References")]
    [SerializeField] protected UnitUI _ui;
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] protected StunnedVFX _stunnedVFX;
    [SerializeField] protected PoisonedVFX _poisonedVFX;
    [SerializeField] protected ShieldVFX _shieldVFX;
    public Animator _animator;
    public UnitAnimsInfos _unitAnimsInfos;



    #region Unit Infos Functions

    public void InitialiseUnitInfos(int maxHealth, int strength, int speed, int luck, int movePoints)
    {
        currentHealth = maxHealth;
        currentMaxHealth = maxHealth;
        currentStrength = strength;
        currentSpeed = speed;
        currentLuck = luck;

        currentMovePoints = movePoints;

        _ui.HoverAction += HoverUnit;
        _ui.UnHoverAction += UnHoverUnit;
        _ui.ClickAction += ClickUnit;

        _ui.ActualiseUI(1, currentHealth, currentAlterations);

        StartCoroutine(StartAlterationsDelayCoroutine());

        _spriteRenderer.material.SetVector("_TextureSize", new Vector2(_spriteRenderer.sprite.texture.width, _spriteRenderer.sprite.texture.height));
    }

    private IEnumerator StartAlterationsDelayCoroutine()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < unitData.startAlterations.Length; i++)
        {
            AddAlteration(unitData.startAlterations[i], this);
        }

        _ui.ActualiseUI(1, currentHealth, currentAlterations);
    }

    public virtual void ActualiseUnitInfos(int addedMaxHealth, int addedStrength, int addedSpeed, int addedLuck, int addedMovePoints, int addedSP)
    {
        int currentDamages = currentMaxHealth - currentHealth;

        currentMaxHealth = unitData.baseHealth + addedMaxHealth;
        currentHealth = currentMaxHealth - currentDamages;
        currentStrength = unitData.baseStrength + addedStrength;
        currentSpeed = unitData.baseSpeed + addedSpeed;
        currentLuck = unitData.baseLuck + addedLuck;

        PassiveData[] passives = BattleManager.Instance.PassivesManager.GetTriggeredPassives(PassiveTriggerType.Always, this).ToArray();
        BattleManager.Instance.PassivesManager.ApplyPassives(passives, this, null);

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations);
    }

    public virtual void AddStatsModificators(int addedMaxHealth, int addedStrength, int addedSpeed, int addedLuck, int addedMovePoints, int addedSP)
    {
        int currentDamages = currentMaxHealth - currentHealth;

        currentMaxHealth += addedMaxHealth;
        currentHealth = currentMaxHealth - currentDamages;
        currentStrength += addedStrength;
        currentSpeed += addedSpeed;
        currentLuck += addedLuck;

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations);
    }

    #endregion 


    #region Move Functions

    public IEnumerator MoveUnitCoroutine(BattleTile[] pathTiles)
    {
        for(int i = 0; i < pathTiles.Length; i++)
        {
            MoveUnit(pathTiles[i], true);

            if (pathTiles[i].IsHole)
            {
                StartCoroutine(FallCoroutine());

                yield break;
            }

            yield return new WaitForSeconds(0.15f);
        }

        BattleManager.Instance.PathCalculator.ActualisePathCalculatorTiles(BattleManager.Instance.BattleRoom.PlacedBattleTiles);
    }

    private IEnumerator FallCoroutine()
    {
        _spriteRenderer.transform.UChangeScale(0.5f, Vector3.zero, CurveType.EaseInOutSin);

        yield return new WaitForSeconds(0.5f);

        _spriteRenderer.transform.UStopChangeScale();
        MoveUnit(previousTile);
        _spriteRenderer.transform.localScale = Vector3.one;

        HeroesManager.Instance.TakeDamage(1);
        BattleManager.Instance.PathCalculator.ActualisePathCalculatorTiles(BattleManager.Instance.BattleRoom.PlacedBattleTiles);
    }

    public void MoveUnit(BattleTile tile, bool doSquish = false)
    {
        if(currentTile != null)
        {
            currentTile.UnitLeaveTile();
            previousTile = currentTile;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        tile.UnitEnterTile(this, isTall);
        currentTile = tile;

        transform.position = tile.transform.position;

        if (doSquish)
        {
            if(squishCoroutine != null)
                StopCoroutine(squishCoroutine);

            squishCoroutine = StartCoroutine(SquishCoroutine(0.125f));
        }
    }

    protected IEnumerator SquishCoroutine(float duration)
    {
        transform.UChangeScale(duration * 0.24f, new Vector3(1.25f, 0.85f, 1f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.2f);

        transform.UChangeScale(duration * 0.48f, new Vector3(0.85f, 1.15f, 1f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.5f);

        transform.UChangeScale(duration * 0.24f, new Vector3(1f, 1f, 1f), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(duration * 0.25f);
    }

    public void PushUnit(Vector2Int direction, int strength)
    {
        List<BattleTile> crossedTiles = new List<BattleTile>();

        for(int i = 0; i < strength; i++)
        {
            Vector2Int currentPos = currentTile.TileCoordinates + direction * (i + 1);

            if (BattleManager.Instance.BattleRoom.PlacedBattleTiles[currentPos.x, currentPos.y] is null) break;
            if (BattleManager.Instance.BattleRoom.PlacedBattleTiles[currentPos.x, currentPos.y].UnitOnTile is not null) break;

            crossedTiles.Add(BattleManager.Instance.BattleRoom.PlacedBattleTiles[currentPos.x, currentPos.y]);
        }

        StartCoroutine(MoveUnitCoroutine(crossedTiles.ToArray()));
    }

    #endregion


    #region Enter / Exit Functions

    public virtual void EnterBattle(BattleTile startTile)
    {
        _ui.ShowUnitUI();
        currentTile = startTile;
    }

    public virtual void ExitBattle(Hero currentHero)
    {
        _ui.HideUnitUI();
    }

    #endregion


    #region Use / Apply Skills

    public bool VerifyCrit(Unit[] attackedUnits, int addedProba)
    {
        if(attackedUnits.Length == 0) return false;
        if (TutoManager.Instance.IsInTuto) return false;

        int averageLuck = 0;
        for(int i = 0; i < attackedUnits.Length; i++)
        {
            averageLuck += attackedUnits[i].CurrentLuck;
        }
        averageLuck /= attackedUnits.Length;

        int critProba = ((CurrentLuck - averageLuck) + 3) * 3;
        int pickedProba = Random.Range(0, 100);

        return pickedProba < critProba + addedProba;
    }

    public virtual void TakeDamage(int damageAmount, Unit originUnit)
    {
        PassiveData[] triggeredPassives = BattleManager.Instance.PassivesManager.GetTriggeredPassives(PassiveTriggerType.OnDamageReceived, this).ToArray();
        for(int i = 0; i <= triggeredPassives.Length; i++)
        {
            BattleManager.Instance.PassivesManager.ApplyPassives(triggeredPassives, this, originUnit);
        }

        _animator.SetTrigger("Damage");
        CameraManager.Instance.DoCameraShake(0.2f, Mathf.Lerp(0.5f, 1f, damageAmount / 15f), 50);

        // For no damages when the hero explores in the tuto
        if (!BattleManager.Instance.IsInBattle && TutoManager.Instance.IsInTuto)
        {
            StartCoroutine(DoColorEffectCoroutine(damageColor));
            return;
        }

        // Thorn
        if (originUnit)
        {
            AlterationData thornAlt = VerifyHasAlteration(AlterationType.Thorn);
            if (thornAlt) originUnit.TakeDamage((int)thornAlt.strength, null);
        }

        // Skulled
        if (IsSkulled)
        {
            RemoveAlteration(AlterationType.Skulled);
            StartCoroutine(DoColorEffectCoroutine(skullColor));
            return;
        }

        // Vulnerable
        AlterationData vulnerableAlt = VerifyHasAlteration(AlterationType.Vulnerable);
        if (vulnerableAlt) damageAmount = (int)(damageAmount * vulnerableAlt.strength);

        // Sturdy
        AlterationData sturdyAlt = VerifyHasAlteration(AlterationType.Sturdy);
        if (sturdyAlt) damageAmount = (int)(damageAmount * sturdyAlt.strength);

        int shieldDamages = Mathf.Clamp(damageAmount, 0, CurrentShield);
        int healthDamages = damageAmount - shieldDamages;

        if (shieldDamages == CurrentShield && CurrentShield > 0) RemoveAlteration(AlterationType.Shield);

        CurrentShield -= shieldDamages;
        CurrentHealth -= healthDamages;

        StartCoroutine(DoColorEffectCoroutine(damageColor));

        // Shield
        if(shieldDamages != 0)
        {
            if (CurrentShield > 0) _shieldVFX.PlayBlockAnim();
            else _shieldVFX.PlayBreakAnim();
        }

        if (CurrentHealth <= 0)
            Die();
    }


    protected virtual void Die()
    {
        currentTile.UnitLeaveTile();
        BattleManager.Instance.RemoveUnit(this);
        Destroy(gameObject);
    }


    public virtual void Heal(int healedAmount)
    {
        if (healedAmount != -1)
            CurrentHealth = Mathf.Clamp(CurrentHealth + healedAmount, 0, CurrentMaxHealth);
        else
            CurrentHealth = CurrentMaxHealth;

        AudioManager.Instance.PlaySoundOneShot(2, 11);

        StartCoroutine(DoColorEffectCoroutine(healColor));
    }


    private IEnumerator DoColorEffectCoroutine(Color color)
    {
        _spriteRenderer.material.DOVector(color, "_AddedColor", 0.1f);

        yield return new WaitForSeconds(0.15f);

        _spriteRenderer.material.DOVector(Color.black, "_AddedColor", 0.25f);
    }

    #endregion


    #region Alterations

    public void AddAlteration(AlterationData alteration, Unit origin)
    {
        bool alreadyApplied = false;

        OnAlterationAdded?.Invoke(1);

        if (BattleManager.Instance.PassivesManager.VerifyAlterationImmunities(alteration.alterationType, this)) return;
        int passiveModificator = BattleManager.Instance.PassivesManager.GetGivePassiveAlterationUpgrade(origin, alteration);
        passiveModificator += BattleManager.Instance.PassivesManager.GetReceivePassiveAlterationUpgrade(this, alteration);

        for (int i = 0; i < currentAlterations.Count; i++)
        {
            if (currentAlterations[i].alteration.alterationType == alteration.alterationType)
            {
                alreadyApplied = true;

                AlterationStruct current = currentAlterations[i];

                if (current.alteration.alterationType == AlterationType.Shield)
                {
                    current.currentStrength = alteration.strength > currentAlterations[i].currentStrength ?
                        alteration.strength : currentAlterations[i].currentStrength;
                }
                else
                {
                    current.duration = alteration.duration > currentAlterations[i].duration ?
                        alteration.duration : currentAlterations[i].duration;
                }

                if (alteration.isStackable)
                {
                    current.currentStrength += alteration.strength;
                }

                current.currentStrength += passiveModificator;
                currentAlterations[i] = current;

                break;
            }
        }

        if (!alreadyApplied)
        {
            currentAlterations.Add(new AlterationStruct(alteration, alteration.duration, alteration.strength + passiveModificator, origin));
        }

        if(alteration.alterationType == AlterationType.Shield)
        {
            CurrentShield = Mathf.Max(CurrentShield, (int)alteration.strength + passiveModificator);
        }

        PlayAlterationVFX(alteration.alterationType, alteration.isPositive);
        ActualiseAlterations(false);
    }

    public void AddAlterationStrength(AlterationType type, float addedStrength)
    {
        for (int i = 0; i < currentAlterations.Count; i++)
        {
            if (currentAlterations[i].alteration.alterationType == type)
            {
                AlterationStruct current = currentAlterations[i];

                current.currentStrength += addedStrength;

                currentAlterations[i] = current;
            }
        }
    }


    public AlterationData VerifyHasAlteration(AlterationType type)
    {
        foreach (AlterationStruct altStruct in currentAlterations)
        {
            if (altStruct.alteration.alterationType == type)
            {
                return altStruct.alteration;
            }
        }
        return null;
    }

    private void PlayAlterationVFX(AlterationType type, bool isBuff)
    {
        switch (type)
        {
            case AlterationType.Shield:
                _shieldVFX.PlayEquipAnim();
                return;

            case AlterationType.Stunned:
                Destroy(Instantiate(stunVFX, transform.position, Quaternion.Euler(0, 0, 0)), 1f);
                _stunnedVFX.Appear();
                return;

            case AlterationType.Poisoned:
                _poisonedVFX.PlayStartPoisonEffect();
                return;

            case AlterationType.Hindered:
                Destroy(Instantiate(hinderVFX, transform.position, Quaternion.Euler(0, 0, 0)), 1f);
                return;

            case AlterationType.Skulled:
                Destroy(Instantiate(skullVFX, transform.position, Quaternion.Euler(0,0, 0)), 1f);
                return;
        }

        if (isBuff)
        {
            AudioManager.Instance.PlaySoundOneShot(2, 8);
            Destroy(Instantiate(buffVFX, transform.position, Quaternion.Euler(0, 0, 0)), 1f);
        }
        else
        {
            AudioManager.Instance.PlaySoundOneShot(2, 7);
            Destroy(Instantiate(debuffVFX, transform.position, Quaternion.Euler(0, 0, 0)), 1f);
        }
    }

    public void RemoveAllAlterations()
    {
        for (int i = currentAlterations.Count - 1; i >= 0; i--)
        {
            RemoveAlteration(i);
        }

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations);
    }

    public void RemoveAllNegativeAlterations()
    {
        for (int i = currentAlterations.Count - 1; i >= 0; i--)
        {
            if (currentAlterations[i].alteration.isPositive) continue;
            RemoveAlteration(i);
        }

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations);
    }

    private void RemoveAlteration(int index)
    {
        if (currentAlterations[index].alteration.alterationType == AlterationType.Stunned)
        {
            _stunnedVFX.Hide();
        }

        currentAlterations.RemoveAt(index);
    }

    private void RemoveAlteration(AlterationType type)
    {
        for (int i = currentAlterations.Count - 1; i >= 0; i--)
        {
            if (currentAlterations[i].alteration.alterationType == type)
            {
                RemoveAlteration(i);
                break;
            }
        }

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations);
    }


    private void ActualiseAlterations(bool endTurn)
    {
        if (endTurn)
        {
            for (int i = currentAlterations.Count - 1; i >= 0; i--)
            {
                AlterationStruct current = currentAlterations[i];

                if (!current.alteration.isInfinite)
                    current.duration--;

                currentAlterations[i] = current;

                if (currentAlterations[i].duration <= 0 && !currentAlterations[i].alteration.isInfinite)
                {
                    RemoveAlteration(i);
                }
            }
        }

        movePointsModificatorAdditive = 0;
        strengthModificatorAdditive = 0;
        speedModificatorAdditive = 0;
        luckModificatorAdditive = 0;
        provocationTarget = null;

        for (int i = 0; i < currentAlterations.Count; i++)
        {
            switch (currentAlterations[i].alteration.alterationType)
            {
                case AlterationType.Strength:
                    strengthModificatorAdditive += (int)(currentStrength * currentAlterations[i].currentStrength);
                    break;

                case AlterationType.Weakened:
                    strengthModificatorAdditive -= (int)(currentStrength * currentAlterations[i].currentStrength);
                    break;

                case AlterationType.Provocked:
                    provocationTarget = currentAlterations[i].origin;
                    break;

                case AlterationType.Lucky:
                    luckModificatorAdditive += (int)(currentLuck * currentAlterations[i].currentStrength);
                    break;

                case AlterationType.Unlucky:
                    luckModificatorAdditive -= (int)(currentLuck * currentAlterations[i].currentStrength);
                    break;
            }
        }

        _ui.ActualiseUI((float)currentHealth / currentMaxHealth, currentHealth, currentAlterations);
    }

    #endregion


    #region Mouse Inputs

    private void HoverUnit()
    {
        isHovered = true;

        CurrentTile?.OverlayTile();
    }

    private void UnHoverUnit()
    {
        isHovered = false;

        StartCoroutine(VerifyQuitOverlayTile());
    }

    private IEnumerator VerifyQuitOverlayTile()
    {
        yield return new WaitForEndOfFrame();

        if (!CurrentTile || CurrentTile.IsHovered) yield break;

        CurrentTile?.QuitOverlayTile();
    }

    protected virtual async void ClickUnit()
    {
        await Task.Delay((int)(Time.deltaTime * 1000));

        if (InputManager.wantsToRightClick) return;

        StartCoroutine(SquishCoroutine(0.15f));
        CurrentTile?.ClickTile();
    }

    #endregion


    #region Outline Functions

    public void HideOutline()
    {
        if (restartTurnOutlineNext)
        {
            restartTurnOutlineNext = false;
            StartTurnOutline();
        }

        _spriteRenderer.material.ULerpMaterialColor(0.15f, new Color(0, 0, 0, 0), "_OutlineColor");
    }

    protected void DisplayOutline(Color color)
    {
        if(turnOutlineCoroutine != null)
        {
            restartTurnOutlineNext = true;
            EndTurnOutline();
        }

        _spriteRenderer.material.ULerpMaterialColor(0.15f, color, "_OutlineColor");
    }

    public void DisplayOverlayOutline()
    {
        DisplayOutline(overlayColor);
    }

    public void DisplaySkillOutline(bool isPositive)
    {
        if (isPositive)
        {
            DisplayOutline(positiveColor);
        }
        else
        {
            DisplayOutline(negativeColor);
        }
    }

    protected void StartTurnOutline()
    {
        if(turnOutlineCoroutine != null)
        {
            StopCoroutine(turnOutlineCoroutine);
        }

        if (!isUnitsTurn) return;

        turnOutlineCoroutine = StartCoroutine(TurnOutlineCoroutine());
    }

    protected void EndTurnOutline()
    {
        if (turnOutlineCoroutine != null)
        {
            StopCoroutine(turnOutlineCoroutine);
        }
    }

    private IEnumerator TurnOutlineCoroutine()
    {
        while (true)
        {
            _spriteRenderer.material.ULerpMaterialColor(0.7f, unitsTurnColor, "_OutlineColor");

            yield return new WaitForSeconds(0.9f);

            _spriteRenderer.material.ULerpMaterialColor(0.7f, new Color(0, 0, 0, 0), "_OutlineColor");

            yield return new WaitForSeconds(0.9f);
        }
    }

    #endregion


    protected IEnumerator DisappearCoroutine(float duration)
    {
        BattleManager.Instance.RemoveUnit(this);
        _spriteRenderer.material.ULerpMaterialFloat(duration, -0.5f, "_DitherProgress");

        yield return new WaitForSeconds(duration);

        Destroy(gameObject);
    }


    public void UseItem(LootData itemData)
    {
        switch (itemData.consumableType)
        {
            case ConsumableType.Heal:
                Heal(itemData.consumablePower);
                break;

            case ConsumableType.Focus:
                (this as Hero).AddSkillPoints(itemData.consumablePower);
                break;

            case ConsumableType.Action:
                (this as Hero).AddActionPoints(itemData.consumablePower);
                break;
        }
    }


    public virtual void StartTurn()
    {
        isUnitsTurn = true;

        // Poison
        if (VerifyHasAlteration(AlterationType.Poisoned))
        {
            TakeDamage(1, null);
            _poisonedVFX.PlayApplyPoisonEffect();
        }

        StartTurnOutline();
    }


    public virtual void EndTurn(float delay)
    {
        isUnitsTurn = false;
        restartTurnOutlineNext = false;

        EndTurnOutline();
        HideOutline();

        ActualiseAlterations(true);

        StartCoroutine(BattleManager.Instance.NextTurnCoroutine(delay));
    }
}
