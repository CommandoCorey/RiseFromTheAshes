using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitStats : MonoBehaviour
{
    public UnitController[] unitPrefabs;

    public TextMeshProUGUI title;
    public Image tankImage;
    public float maxBarWidth = 500;

    [Header("Time and cost")]
    public TextMeshProUGUI costValue;
    public TextMeshProUGUI trainTimeValue;

    [Header("UnitStats")]
    public RectTransform maxHPBar;
    public TextMeshProUGUI maxHPValue;
    public RectTransform attackRateBar;
    public TextMeshProUGUI attackRateValue;
    public RectTransform damageBar;
    public TextMeshProUGUI damageValue;
    public RectTransform speedBar;
    public TextMeshProUGUI speedValue;
    public RectTransform rangeBar;
    public TextMeshProUGUI rangeValue;

    [Header("Ranges")]
    [SerializeField] float maxHP;
    [SerializeField] float maxAttackRate;
    [SerializeField] float maxDamage;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxRange;

    private int unitNumber = 0;

    // Start is called before the first frame update
    void Start()
    {
        UpdateStats(unitPrefabs[unitNumber]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateStats(UnitController unit)
    {
        title.text = unit.Name;
        trainTimeValue.text = unit.TimeToTrain.ToString();

        tankImage.sprite = unit.GuiIcon;
        maxHPValue.text = unit.MaxHealth.ToString();
        attackRateValue.text = unit.AttackRate.ToString();
        damageValue.text = unit.DamagePerHit.ToString();
        rangeValue.text = unit.AttackRange.ToString();
        speedValue.text = unit.Speed.ToString();

        // update bars
        maxHPBar.sizeDelta = new Vector2(maxBarWidth / maxHP * unit.MaxHealth, maxHPBar.sizeDelta.y);
        attackRateBar.sizeDelta = new Vector2(maxBarWidth / maxAttackRate * unit.AttackRate, attackRateBar.sizeDelta.y);
        damageBar.sizeDelta = new Vector2(maxBarWidth / maxDamage * unit.DamagePerHit, damageBar.sizeDelta.y);
        speedBar.sizeDelta = new Vector2(maxBarWidth / maxSpeed * unit.Speed, speedBar.sizeDelta.y);
        rangeBar.sizeDelta = new Vector2(maxBarWidth / maxRange * unit.AttackRange, rangeBar.sizeDelta.y);
    }

    public void NextVehicle()
    {
        unitNumber++;

        if(unitNumber >= unitPrefabs.Length)
            unitNumber = 0;

        UpdateStats(unitPrefabs[unitNumber]);
    }

    public void PreviousVehicle()
    {
        unitNumber--;

        if (unitNumber < 0)
            unitNumber = unitPrefabs.Length - 1;

        UpdateStats(unitPrefabs[unitNumber]);
    }

}
