using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Alteration : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color buffColor;
    [SerializeField] private Color debuffColor;

    [Header("Private Infos")]
    private AlterationStruct currentData;
    private Unit attachedUnit;
    private bool isDisplayed;

    [Header("References")]
    [SerializeField] private Image _image;


    private void Setup(AlterationStruct info, Unit unit)
    {
        currentData = info;
        attachedUnit = unit;

        _image.sprite = info.alteration.alterationIcon;

        if (info.alteration.isPositive)
        {
            _image.color = buffColor;
        }
        else
        {
            _image.color = debuffColor;
        }
    }


    #region Appear / Disappear

    public void Appear(AlterationStruct info, Unit unit)
    {
        Setup(info, unit);
        _image.enabled = true;

        if (!isDisplayed)
        {
            isDisplayed = true;
        }
    }

    public void Disappear()
    {
        _image.enabled = false;

        if (isDisplayed)
        {
            isDisplayed = false;

        }
    }

    #endregion


    #region Mouse Inputs
    
    public void OverlayAlteration()
    {
        UIMetaManager.Instance.GenericDetailsPanel.LoadDetails(currentData, transform.position, attachedUnit);
    }

    public void QuitOverlayAlteration()
    {
        UIMetaManager.Instance.GenericDetailsPanel.HideDetails();
    }

    #endregion
}
