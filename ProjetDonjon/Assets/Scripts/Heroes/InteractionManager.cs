using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float yOffset;

    [Header("Actions")]
    public Action<int> OnInteraction;    // For Tuto

    [Header("Private Infos")]
    private List<IInteractible> interactiblesAtRange = new List<IInteractible>();
    private IInteractible closestInteractible;
    private bool isSetup;

    [Header("Referencess")]
    private Transform currentHeroTransform;
    

    private void Start()
    {
        closestInteractible = null;
    }

    public void ActualiseCurrentHeroTransform(Transform newTr)
    {
        isSetup = true; 
        currentHeroTransform = newTr;
    }


    #region Detect Interactibles

    private void Update()
    {
        if (!isSetup) return;

        transform.position = currentHeroTransform.position + new Vector3(0, yOffset, 0);

        if (closestInteractible is null) return;
        if (UIManager.Instance.CurrentUIState != UIState.Nothing) return;
        if (InputManager.wantsToInteract)
        {
            OnInteraction?.Invoke(0);
            closestInteractible.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!GameManager.Instance.IsInExplo) return;

        AddInteractibleAtRange(collision.GetComponent<IInteractible>());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!GameManager.Instance.IsInExplo) return;

        RemoveInteractibleAtRange(collision.GetComponent<IInteractible>());
    }

    #endregion


    #region Manage Interactibles

    public void AddInteractibleAtRange(IInteractible interactible)
    {
        interactiblesAtRange.Add(interactible);
        ActualiseClosestInteractible();
    }

    public void RemoveInteractibleAtRange(IInteractible interactible)
    {
        interactible.CannotBePicked();
        interactiblesAtRange.Remove(interactible);
        ActualiseClosestInteractible();
    }

    private void ActualiseClosestInteractible()
    {
        closestInteractible = null;
        if (interactiblesAtRange.Count == 0) return;

        int closestIndex = 0;
        float bestDist = Vector2.Distance(interactiblesAtRange[0].GetTransform().position, currentHeroTransform.position);

        for(int i = 1; i < interactiblesAtRange.Count; i++)
        {
            interactiblesAtRange[i].CannotBePicked();
            float dist = Vector2.Distance(interactiblesAtRange[i].GetTransform().position, currentHeroTransform.position);

            if(dist < bestDist)
            {
                bestDist = dist;
                closestIndex = i;
            }
        }

        interactiblesAtRange[closestIndex].CanBePicked();
        closestInteractible = interactiblesAtRange[closestIndex];
    }

    #endregion
}
