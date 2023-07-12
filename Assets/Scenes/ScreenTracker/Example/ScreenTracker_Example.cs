using System;
using UnityEngine;
using Luch.UI;


public class ScreenTracker_Example : MonoBehaviour
{
    [SerializeField] private ScreenTrackersController _tracker;
    [SerializeField] private Transform[] _targets;

    void Start()
    {
        if (_targets != null)
        {
            foreach (Transform t in _targets)
            {
                _tracker.CreateTracker(t, t.GetInstanceID());
            }
        }
    }
}
