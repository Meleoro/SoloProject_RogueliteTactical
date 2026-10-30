using UnityEngine;

public class Enums 
{


    public enum ObstacleType
    {
        Nothing,
        Holes,
        Enemies,       // Only blocked by obstacles and units considered as enemies for the unit
        Units,         // Blocked by all units and obstacles, allies or enemies
        UnitsIncluded,      // Units and obstacles block but are included on the valid tiles
        All
    }
}
