using UnityEngine;

public class HeroStatSlot : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private GenericDetailsData statData;

    [Header("References")]
    [SerializeField] private RectTransform _iconRectTr;

    public void Hover()
    {
        UIMetaManager.Instance.GenericDetailsPanel.LoadDetails(statData, _iconRectTr.position + new Vector3(3, 0, 0), true);
    }

    public void Unhover()
    {
        UIMetaManager.Instance.GenericDetailsPanel.HideDetails();
    }
}
