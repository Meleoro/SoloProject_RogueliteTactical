using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Utilities;

public class SkillTreeManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private SkillTreeNode nodePrefab;

    [Header("Private Infos")]
    private SkillTreeNode[] currentNodes;
    private SkillTreeNode[,] organisedCurrentNodes;
    private Hero currentHero;
    private float maxDistX, maxDistY;
    private bool isOpeningOrClosing;
    private int currentIndex;

    [Header("Actions")]
    public Action OnShow;
    public Action OnHide;

    [Header("References")]
    [SerializeField] private RectTransform _nodesBottomLeftTrRef;
    [SerializeField] private RectTransform _nodesUpRightTrRef;
    [SerializeField] private RectTransform _nodesParent;
    [SerializeField] private RectTransform _mainRectTr; 
    [SerializeField] private RectTransform _leftArrowRectTr;
    [SerializeField] private RectTransform _leftArrowPosRef;
    [SerializeField] private RectTransform _rightArrowRectTr;
    [SerializeField] private RectTransform _rightArrowPosRef;
    [SerializeField] private TextMeshProUGUI _heroNameText;
    [SerializeField] private TextMeshProUGUI _heroAvailablePointsText;


    private void Start()
    {
        currentNodes = new SkillTreeNode[15];
        for(int i = 0; i < currentNodes.Length; i++)
        {
            currentNodes[i] = Instantiate(nodePrefab, transform.position, Quaternion.Euler(0, 0, 0), _nodesParent);
            currentNodes[i].OnHover += HoverNode;
            currentNodes[i].OnUnhover += UnhoverNode;
            currentNodes[i].OnClickValid += ClickNode;
        }
    }


    #region Generate Skill Tree

    private void LoadHero(Hero hero)
    {
        currentHero = hero;

        GenerateSkillTree(hero);
        SetupNodes(hero);

        _heroNameText.text = hero.HeroData.unitName;
        _heroAvailablePointsText.text = "AVAILABLE POINTS : " + hero.CurrentSkillTreePoints;
    }


    public void GenerateSkillTree(Hero hero)
    {
        maxDistX = _nodesUpRightTrRef.position.x - _nodesBottomLeftTrRef.position.x;
        maxDistY = _nodesUpRightTrRef.position.y - _nodesBottomLeftTrRef.position.y;

        int lengthX = hero.SkillTreeData.skillTreeRows.Length;
        float distBetweenCols = maxDistX / lengthX;
        int currentNodeIndex = 0;
        organisedCurrentNodes = new SkillTreeNode[lengthX, 5];

        for (int x = 0; x < lengthX; x++)
        {
            int lengthY = hero.SkillTreeData.skillTreeRows[x].rowNodes.Length;
            float distBetweenRows = maxDistY / 3.5f;
            Vector3 middlePos = _nodesBottomLeftTrRef.position + new Vector3(distBetweenCols * (x + 0.5f), 
                maxDistY * 0.5f, 0);

            for (int y = 0; y < lengthY; y++)
            {
                Vector3 finalPos = middlePos + new Vector3(0, distBetweenRows * (y + 0.5f - (lengthY * 0.5f)), 0);
                currentNodes[currentNodeIndex].SetupNode(finalPos, hero.SkillTreeData.skillTreeRows[x].rowNodes[y], hero);

                organisedCurrentNodes[x, y] = currentNodes[currentNodeIndex];

                currentNodeIndex++;
            }
        }

        currentNodeIndex = 0;
        for (int x = 0; x < lengthX; x++)
        {
            int lengthY = hero.SkillTreeData.skillTreeRows[x].rowNodes.Length;

            for (int y = 0; y < lengthY; y++)
            {
                SkillTreeNode[] connectedNodes = 
                    GetConnectedNodes(hero.SkillTreeData.skillTreeRows[x].rowNodes[y].connectedNextNodeIndexes, new Vector2Int(x, y));

                currentNodes[currentNodeIndex].SetupLinks(connectedNodes);

                currentNodeIndex++;
            }
        }
    }


    private SkillTreeNode[] GetConnectedNodes(int[] connectedIndexes, Vector2Int coord)
    {
        SkillTreeNode[] result = new SkillTreeNode[connectedIndexes.Length];   

        for(int i = 0; i < connectedIndexes.Length; i++)
        {
            result[i] = organisedCurrentNodes[coord.x + 1, connectedIndexes[i]];
        }

        return result;
    }

    #endregion


    #region Open / Close / Change Effects

    public void Show()
    {
        if (isOpeningOrClosing) return;

        currentIndex = HeroesManager.Instance.CurrentHeroIndex;
        Hero hero = HeroesManager.Instance.Heroes[currentIndex];

        LoadHero(hero);

        HeroesManager.Instance.StopControl();
        OnShow.Invoke();
        isOpeningOrClosing = true;

        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        _mainRectTr.UChangeLocalPosition(0.3f, new Vector3(0, -30, 0), CurveType.EaseOutCubic);
        _leftArrowRectTr.UChangeLocalPosition(0.3f, _leftArrowPosRef.localPosition + new Vector3(10, 0, 0), CurveType.EaseOutCubic);
        _rightArrowRectTr.UChangeLocalPosition(0.3f, _rightArrowPosRef.localPosition + new Vector3(-10, 0, 0), CurveType.EaseOutCubic);

        yield return new WaitForSeconds(0.3f);

        _mainRectTr.UChangeLocalPosition(0.1f, new Vector3(0, 0, 0), CurveType.EaseInOutCubic);
        _leftArrowRectTr.UChangeLocalPosition(0.1f, _leftArrowPosRef.localPosition, CurveType.EaseInOutCubic);
        _rightArrowRectTr.UChangeLocalPosition(0.1f, _rightArrowPosRef.localPosition, CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(0.1f);

        isOpeningOrClosing = false;
    }

    public void Hide()
    {
        if (isOpeningOrClosing) return;

        HeroesManager.Instance.RestartControl();
        OnHide.Invoke();
        isOpeningOrClosing = true;

        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        _mainRectTr.UChangeLocalPosition(0.1f, new Vector3(0, -30, 0), CurveType.EaseOutCubic);
        _leftArrowRectTr.UChangeLocalPosition(0.1f, _leftArrowPosRef.localPosition + new Vector3(10, 0, 0), CurveType.EaseOutCubic);
        _rightArrowRectTr.UChangeLocalPosition(0.1f, _rightArrowPosRef.localPosition + new Vector3(-10, 0, 0), CurveType.EaseOutCubic);

        yield return new WaitForSeconds(0.1f);

        _mainRectTr.UChangeLocalPosition(0.3f, new Vector3(0, 600, 0), CurveType.EaseInOutCubic);
        _leftArrowRectTr.UChangeLocalPosition(0.3f, _leftArrowPosRef.localPosition + new Vector3(-100, 0, 0), CurveType.EaseInOutCubic);
        _rightArrowRectTr.UChangeLocalPosition(0.3f, _rightArrowPosRef.localPosition + new Vector3(100, 0, 0), CurveType.EaseInOutCubic);

        yield return new WaitForSeconds(0.3f);

        isOpeningOrClosing = false;
    }

    public IEnumerator ChangeHeroCoroutine(bool goLeft)
    {
        isOpeningOrClosing = true;

        Vector3 pos1 = goLeft ? new Vector3(-800, 0, 0) : new Vector3(800, 0, 0);
        Quaternion rot1 = goLeft ? Quaternion.Euler(0, 0, 15) : Quaternion.Euler(0, 0, -15);
        _mainRectTr.UChangeLocalPosition(0.2f, pos1, CurveType.EaseInCubic);
        _mainRectTr.UChangeLocalRotation(0.2f, rot1, CurveType.EaseInCubic);

        yield return new WaitForSeconds(0.2f);

        _mainRectTr.localRotation = Quaternion.Euler(0, 0, 0);

        currentIndex += goLeft ? -1 : 1;
        if (currentIndex < 0) currentIndex += HeroesManager.Instance.Heroes.Length;
        currentIndex = currentIndex % HeroesManager.Instance.Heroes.Length;
        Hero hero = HeroesManager.Instance.Heroes[currentIndex];
        LoadHero(hero);

        _mainRectTr.localRotation = rot1;

        _mainRectTr.localPosition = -pos1;
        _mainRectTr.UChangeLocalPosition(0.2f, new Vector3(0, 0, 0), CurveType.EaseOutCubic);
        _mainRectTr.UChangeLocalRotation(0.2f, Quaternion.Euler(0, 0, 0), CurveType.EaseOutCubic);

        yield return new WaitForSeconds(0.2f);

        isOpeningOrClosing = false;
    }

    #endregion


    #region Node Functions

    private void HoverNode(SkillTreeNode node)
    {
        UIMetaManager.Instance.GenericDetailsPanel.LoadDetails(node.Data, node.transform.position, node.RectTr.localPosition.x < 0);
    }

    private void UnhoverNode(SkillTreeNode node)
    {
        UIMetaManager.Instance.GenericDetailsPanel.HideDetails();
    }

    private void ClickNode(SkillTreeNode node)
    {
        node.ActualiseNodeState(true, true);

        bool[] possessedNodes = new bool[currentNodes.Length];
        for(int i = 0; i < possessedNodes.Length; i++)
        {
            possessedNodes[i] = currentNodes[i].IsPossessed;
        }

        currentHero.ActualiseSkillTreeUnlockedNodes(possessedNodes);
        _heroAvailablePointsText.text = "AVAILABLE POINTS : " + currentHero.CurrentSkillTreePoints;

        ActualiseNodes();
    }


    #endregion


    #region Others

    public void HoverArrow(bool isLeft)
    {
        if (isLeft)
        {
            _leftArrowRectTr.UChangeScale(0.2f, Vector3.one * 1.2f, CurveType.EaseOutSin);
        }
        else
        {
            _rightArrowRectTr.UChangeScale(0.2f, Vector3.one * 1.2f, CurveType.EaseOutSin);
        }
    }

    public void UnhoverArrow(bool isLeft)
    {
        if (isLeft)
        {
            _leftArrowRectTr.UChangeScale(0.2f, Vector3.one * 1f, CurveType.EaseOutSin);
        }
        else
        {
            _rightArrowRectTr.UChangeScale(0.2f, Vector3.one * 1f, CurveType.EaseOutSin);
        }
    }

    public void ClickArrow(bool isLeft)
    {
        if (isOpeningOrClosing) return;

        StartCoroutine(ChangeHeroCoroutine(isLeft));
    }


    // Called at the end of the generation of a skill tree to setup all nodes
    private void SetupNodes(Hero hero)
    {
        for (int i = 0; i < currentNodes.Length; i++)
        {
            if (currentNodes[i] is null) return;

            currentNodes[i].ActualiseNodeState(hero.SkillTreeUnlockedNodes[i], false);
        }

        ActualiseNodes();
    }

    // Called to verify if some nodes became reachable
    private void ActualiseNodes()
    {
        for(int y = 0; y < organisedCurrentNodes.GetLength(1); y++)
        {
            if (organisedCurrentNodes[0, y] is null) return;

            organisedCurrentNodes[0, y].VerifyNodeState(true);
        }
    }

    #endregion
}
