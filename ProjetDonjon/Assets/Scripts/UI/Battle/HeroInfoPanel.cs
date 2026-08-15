using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroInfoPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image[] _heroImages;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private SkillPoint[] _skillPoints;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _xpText;
    [SerializeField] private Image _healthFillableImage;
    [SerializeField] private Image _xpFillableImage;


    public void InitialisePanel(Hero hero)
    {
        
        for(int i = 0; i < _heroImages.Length; i++)
        {
            _heroImages[i].enabled = i == hero.HeroIndex;
        }

        //_nameText.text = hero.HeroData.unitName;

        ActualisePanel(hero);
    }

    public void ActualisePanel(Hero hero)
    {
        _levelText.text = "LEVEL " + hero.CurrentLevel;

        for(int i = 0; i < hero.CurrentSkillPoints; i++)
        {
            _skillPoints[i].gameObject.SetActive(true);
            _skillPoints[i].ActivateSkillPoint();
        }

        for (int i = hero.CurrentSkillPoints; i < hero.CurrentMaxSkillPoints; i++)
        {
            _skillPoints[i].gameObject.SetActive(true);
            _skillPoints[i].DeactivateSkillPoint();
        }

        for(int i = hero.CurrentMaxSkillPoints; i < _skillPoints.Length; i++)
        {
            _skillPoints[i].gameObject.SetActive(false);
        }

        _healthText.text = hero.CurrentHealth + "/" + hero.CurrentMaxHealth;
        _healthFillableImage.fillAmount = (float)hero.CurrentHealth / hero.CurrentMaxHealth;

        if (hero.CurrentXPToReach == 0) return;

        _xpText.text = hero.CurrentXP + "/" + hero.CurrentXPToReach;
        _xpFillableImage.fillAmount = (float)hero.CurrentXP / hero.CurrentXPToReach;
    }
}
