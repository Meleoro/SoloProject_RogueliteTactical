using UnityEngine;
using UnityEngine.UI;

public class ShopRow : MonoBehaviour
{
    [Header("Public Infos")]
    public ShopItem[] RowItems { get { return _rowItems; } }

    [Header("References")]
    [SerializeField] private ShopItem[] _rowItems;
    [SerializeField] private Image _lockedRowImage;


    public void InitialiseRow(bool isLocked = true)
    {
        if(isLocked) _lockedRowImage.gameObject.SetActive(true);
        else _lockedRowImage.gameObject.SetActive(false);
    }
}
