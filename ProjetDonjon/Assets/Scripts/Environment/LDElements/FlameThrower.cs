using UnityEngine;

public class FlameThrower : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float waypointWaitDuration = 1.0f;
    [SerializeField] private bool turnOffAtWaypoint = true;


    [Header("References")]
    [SerializeField] private Transform[] _waypoints;  // If no waypoints, stays stationary
}
