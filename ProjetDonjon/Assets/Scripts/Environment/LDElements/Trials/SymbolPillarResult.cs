using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SymbolPillarResult : MonoBehaviour
{
    private struct SymbolInfos
    {
        public SymbolInfos(int categoryIndex, int symbolIndex, SpriteRenderer linkedSpriteRenderer)
        {
            this.categoryIndex = categoryIndex;
            this.symbolIndex = symbolIndex;
            this.linkedSpriteRenderer = linkedSpriteRenderer;
        }

        public int categoryIndex;
        public int symbolIndex;
        public SpriteRenderer linkedSpriteRenderer;
    }

    [Header("Parameters")]
    [SerializeField] private Color validColor;

    [Header("Private Infos")]
    private List<SymbolInfos> currentSymbolInfos = new List<SymbolInfos>();

    [Header("References")]
    [SerializeField] private SpriteRenderer[] _symbolSpriteRenderers;


    public void AddSymbol(int categoryIndex, int symbolIndex, Sprite symbolSprite)
    {
        int spriteRendererIndex = currentSymbolInfos.Count;

        currentSymbolInfos.Add(new SymbolInfos(categoryIndex, symbolIndex, _symbolSpriteRenderers[spriteRendererIndex]));

        _symbolSpriteRenderers[spriteRendererIndex].enabled = true;
        _symbolSpriteRenderers[spriteRendererIndex].sprite = symbolSprite;
    }

    public void ValidateSymbol(int categoryIndex, int symbolIndex)
    {
        for(int i = 0; i < currentSymbolInfos.Count; i++)
        {
            if (currentSymbolInfos[i].categoryIndex != categoryIndex || currentSymbolInfos[i].symbolIndex != symbolIndex)
            {
                continue;
            }

            currentSymbolInfos[i].linkedSpriteRenderer.color = validColor;
            break;
        }
    }
}
