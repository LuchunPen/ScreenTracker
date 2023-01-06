//Copyright(c) Luchunpen, 2023.

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Luch.UI
{
    public class ScreenTracker : MonoBehaviour
    {
        private int _trackerID;
        public int TrackerID 
        { 
            get { return _trackerID; } 
            set { _trackerID = value; } 
        }

        [SerializeField] private Image _icon;
        public Image Icon { get { return _icon; } }

        [SerializeField] private TextMeshProUGUI _info;
        public TextMeshProUGUI Info { get { return _info; } }

        [SerializeField] private Transform _target;

        public Transform Target
        {
            get { return _target; }
            set { _target = value; }
        }
    }
}
