using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utilities;


public class Timeline : MonoBehaviour
{
    [Serializable]
    public struct TimelineUnit
    {
        public int progress;
        public Unit unit;
    }

    [Header("Parameters")]
    [SerializeField] private TimelineSlot slotPrefab;
    [SerializeField] private int slotsAmount;
    [SerializeField] private float spaceBetweenSlots;

    [Header("Private Infos")]
    private List<TimelineSlot> slots = new List<TimelineSlot>();
    public List<TimelineUnit> timelineUnits;
    public List<TimelineUnit> currentTimelineUnits;

    [Header("Public Infos")]
    public List<TimelineSlot> Slots { get { return slots; } }
    public RectTransform[] SlotsPositions { get { return _slotsPositions; } }

    [Header("References")]
    [SerializeField] private RectTransform _leftLimitPosition;
    [SerializeField] private RectTransform _mainTr;
    [SerializeField] private RectTransform _hiddenTrPos;
    [SerializeField] private RectTransform _shownTrPos;
    [SerializeField] private RectTransform[] _slotsPositions;



    #region Public Functions

    public void InitialiseTimeline(List<Unit> units)
    {
        if (units.Count == 0) return;

        slots = new List<TimelineSlot>();
        timelineUnits = new List<TimelineUnit>();

        for (int i = 0; i < units.Count; i++)
        {
            TimelineUnit newTilemineUnit = new TimelineUnit();

            newTilemineUnit.unit = units[i];
            newTilemineUnit.progress = units[i].GetType() == typeof(Hero) ? 40 : 0;   // Heroes start with an advantage

            timelineUnits.Add(newTilemineUnit);
            currentTimelineUnits.Add(newTilemineUnit);
        }

        for(int i = 0; i < slotsAmount; i++)
        {
            AddOneUnitToTimeline();
        }

        Appear();
    }

    public void AddUnit(Unit unit)
    {
        TimelineUnit newTilemineUnit = new TimelineUnit();

        newTilemineUnit.unit = unit;
        newTilemineUnit.progress = 100;

        timelineUnits.Add(newTilemineUnit);
        currentTimelineUnits.Add(newTilemineUnit);

        //RecalculateTimeline();
    }


    public void RemoveUnit(Unit unit)
    {
        for (int j = timelineUnits.Count - 1; j >= 0; j--)
        {
            if (timelineUnits[j].unit != unit) continue;

            timelineUnits.RemoveAt(j);
            currentTimelineUnits.RemoveAt(j);
        }

        for (int i = slots.Count - 1; i > 0; i--)
        {
            if (slots[i].Unit == unit)
            {
                slots[i].DestroySlot();
                slots.RemoveAt(i);

                for (int j = slots.Count - 1; j >= i; j--)
                {
                    slots[j].Advance(0.5f, _slotsPositions);
                }
            }
        }

        while(slots.Count < slotsAmount)
        {
            AddOneUnitToTimeline();
        }
    }


    public void NextTurn()
    {
        slots[0].DestroySlot();
        slots.RemoveAt(0);

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Advance(0.5f, _slotsPositions);
        }

        ProgressCurrentTimelineUnits();
        AddOneUnitToTimeline();
    }


    public void RecalculateTimeline()
    {
        timelineUnits.Clear();

        for (int i = 0; i < currentTimelineUnits.Count; i++)
        {
            TimelineUnit newTilemineUnit = new TimelineUnit();

            newTilemineUnit.unit = currentTimelineUnits[i].unit;
            newTilemineUnit.progress = currentTimelineUnits[i].progress;

            timelineUnits.Add(newTilemineUnit);
        }

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            slots[i].DestroySlot();
            slots.RemoveAt(i);
        }

        for (int i = 0; i < slotsAmount; i++)
        {
            AddOneUnitToTimeline();
        }
    }


    public void Disappear()
    {
        _mainTr.UChangeLocalPosition(0.5f, _hiddenTrPos.localPosition, CurveType.EaseOutCubic);

        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i]) continue;
            slots[i].DestroySlot();
        }
    }

    #endregion


    #region Private Functions

    private void Appear()
    {
        _mainTr.UChangeLocalPosition(0.5f, _shownTrPos.localPosition, CurveType.EaseOutCubic);
    }


    private void ProgressCurrentTimelineUnits() 
    {
        TimelineUnit timelineUnit = DetermineWhichUnitOnSlot(currentTimelineUnits);
        for (int i = 0; i < currentTimelineUnits.Count; i++)
        {
            if (currentTimelineUnits[i].unit == timelineUnit.unit)
            {
                TimelineUnit current = currentTimelineUnits[i];
                current.progress -= 100;
                currentTimelineUnits[i] = current;
            }
        }
    }


    private void AddOneUnitToTimeline()
    {
        TimelineUnit timelineUnit = DetermineWhichUnitOnSlot(timelineUnits);
         
        TimelineSlot newSlot = Instantiate(slotPrefab, _slotsPositions[0].parent);
        newSlot._rectTr.localPosition = _leftLimitPosition.localPosition + new Vector3(spaceBetweenSlots * (slots.Count + 1), 0, 0);
        newSlot.SetupSlot(timelineUnit.unit.UnitData.unitImage, timelineUnit.unit, slots.Count + 1);
        newSlot.Advance(0.5f, _slotsPositions);
        slots.Add(newSlot);

        for(int i = 0; i < timelineUnits.Count; i++)
        {
            if(timelineUnits[i].unit == timelineUnit.unit)
            {
                TimelineUnit current = timelineUnits[i];
                current.progress -= 100;
                timelineUnits[i] = current;
            }
        }
    }


    private TimelineUnit DetermineWhichUnitOnSlot(List<TimelineUnit> timelineUnits)
    {
        List<TimelineUnit> validUnits = new List<TimelineUnit>();

        for (int i = 0; i < timelineUnits.Count; i++)
        {
            if (timelineUnits[i].progress >= 100)
            {
                validUnits.Add(timelineUnits[i]);
            }
        }

        // We pick the unit whith the higher progress 
        if (validUnits.Count == 1) return validUnits[0];
        else if(validUnits.Count > 0)
        {
            TimelineUnit best = validUnits[0];
            int maxProgress = 0;

            for(int i = 0; i < validUnits.Count; i++)
            {
                if(validUnits[i].progress >= maxProgress)
                {
                    best = validUnits[i];
                    maxProgress = validUnits[i].progress;
                }
            }

            return best;
        }

        int antiCrashCounter = 0;
        while (antiCrashCounter++ < 1000)
        {
            for (int i = 0; i < timelineUnits.Count; i++)
            {
                TimelineUnit current = timelineUnits[i];
                current.progress += timelineUnits[i].unit.CurrentSpeed + 5;
                timelineUnits[i] = current;

                if (timelineUnits[i].progress >= 100)
                {
                    validUnits.Add(timelineUnits[i]);
                }
            }

            // We pick the unit whith the higher progress 
            if (validUnits.Count == 1) return validUnits[0];
            else if (validUnits.Count > 0)
            {
                TimelineUnit best = validUnits[0];
                int maxProgress = 0;

                for (int i = 0; i < validUnits.Count; i++)
                {
                    if (validUnits[i].progress >= maxProgress)
                    {
                        best = validUnits[i];
                        maxProgress = validUnits[i].progress;
                    }
                }

                return best;
            }
        }

        return new TimelineUnit();
    }

    #endregion
}
