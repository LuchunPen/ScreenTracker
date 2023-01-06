//Copyright(c) Luchunpen, 2023.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Luch.UI
{
    public class ScreenTrackersController : MonoBehaviour
    {
        [Flags]
        public enum RenderMode
        {
            None = 0,
            OnScreen = 1,
            OffScreen = 2
        }


        [SerializeField] private Camera _activeCamera;
        [SerializeField] private RectTransform _trackersRoot;
        [SerializeField] private ScreenTracker _template;
        [SerializeField] private Vector2 _offset;
        [SerializeField] private bool _rounded = false;
        [SerializeField] private bool _rotate = true;
        [SerializeField] private RenderMode _render;

        [Space(10)]
        [Header("Tracker transparency")]
        [SerializeField] private Transform _trackerCenter;
        [SerializeField] private float _distanceStart;
        [SerializeField] private float _distanceEnd;
        [SerializeField] private AnimationCurve _transparencyValue;

        private Dictionary<int, ScreenTracker> _activeTrackers = new Dictionary<int, ScreenTracker>();
        public ScreenTracker[] ActiveTrackers
        {
            get 
            {
                ScreenTracker[] result = new ScreenTracker[_activeTrackers.Count];
                _activeTrackers.Values.CopyTo(result, 0);
                return result;
            }
        }

        public bool ContainsTracker(int id)
        {
            return _activeTrackers.ContainsKey(id);
        }

        public int Count
        {
            get { return _activeTrackers.Count; }
        }

        void Start()
        {
            if (_activeCamera == null) { _activeCamera = Camera.main; }
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            UpdateTrackers();
        }

        public void UpdateTrackers()
        {
            foreach(ScreenTracker tracker in _activeTrackers.Values)
            {
                UpdateTrackerPosition(tracker);
                UpdateTrackersTransparency(tracker);
            }
        }

        public ScreenTracker GetTracker(int id)
        {
            ScreenTracker result;
            _activeTrackers.TryGetValue(id, out result);

            return result;
        }
        private void UpdateTrackerPosition(ScreenTracker tracker)
        {
            if (tracker == null) { return; }
            if (tracker.Target == null) { return; }

            Vector2 localPoint;
            Vector3 trackerPoint = tracker.Target.position;
            bool isOffscreen = false;
            
            /*
            Vector3 center = _trackerCenter.position;
            Vector3 trackeredPosition = tracker.Target.position;
            Vector3 direction = (trackeredPosition - center).normalized;
            float dist = Vector3.Distance(trackeredPosition, center);
            trackerPoint = center + direction * dist;
            */

            Vector3 screenPoint = _activeCamera.WorldToScreenPoint(trackerPoint);

            if (screenPoint.z < 0) { screenPoint *= -1; }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_trackersRoot, screenPoint, null, out localPoint);

            Rect r = _trackersRoot.rect;
            float angl = Mathf.Atan2(localPoint.y, localPoint.x) - (90 * Mathf.Deg2Rad);
            localPoint += _offset;

            if (localPoint.x < r.xMin || localPoint.x > r.xMax || localPoint.y < r.yMin || localPoint.y > r.yMax)
            {               
                float cos = Mathf.Cos(angl);
                float sin = -Mathf.Sin(angl);
                float m = cos / sin;

                Vector2 screenBound = new Vector2(r.width / 2, r.height / 2);

                if (cos > 0) { localPoint = new Vector2(screenBound.y / m, screenBound.y); }
                else { localPoint = new Vector2(-screenBound.y / m, -screenBound.y); }
                if (localPoint.x > screenBound.x) { localPoint = new Vector2(screenBound.x, screenBound.x * m); }
                else if (localPoint.x < -screenBound.x) { localPoint = new Vector2(-screenBound.x, -screenBound.x * m); }

                isOffscreen = true;
            }

            if (_rounded)
            {
                float radius = Mathf.Min(r.width, r.height) / 2;
                Vector2 circlePos = Vector2.ClampMagnitude(localPoint, radius);
                localPoint = circlePos;
            }

            if (_rotate) {
                tracker.transform.localRotation = Quaternion.Euler(0, 0, angl * Mathf.Rad2Deg);
            }

            tracker.transform.localPosition = localPoint;

            if (isOffscreen)
            {
                bool show = (_render & RenderMode.OffScreen) != 0;
                tracker.gameObject.SetActive(show);
            }
            else
            {
                bool show = (_render & RenderMode.OnScreen) != 0;
                tracker.gameObject.SetActive(show);
            }
        }

        private void UpdateTrackersTransparency(ScreenTracker tracker)
        {
            if (_trackerCenter == null) { return; }
            if (tracker == null) { return; }
            if (tracker.Target == null) { return; }

            float distance = Vector3.Distance(tracker.Target.position, _trackerCenter.position);

            float eValue = Mathf.Clamp01((distance - _distanceStart) / (_distanceEnd - _distanceStart));
            float alpha = _transparencyValue.Evaluate(eValue);

            if (tracker.Icon != null)
            {
                Color c = tracker.Icon.color;
                c.a = alpha;
                tracker.Icon.color = c;
            }
        }

        public ScreenTracker CreateTracker(Transform target, int id)
        {
            if (target == null) { return null; }
            if (_template == null) { return null; }

            ScreenTracker tr = Instantiate(_template, _trackersRoot);
            tr.transform.localScale = Vector3.one;
            tr.transform.localRotation = Quaternion.identity;
            tr.transform.localPosition = Vector3.zero;

            tr.Target = target;
            tr.TrackerID = id;
            if (_activeTrackers.ContainsKey(id)) { RemoveTracker(id); }
            _activeTrackers[id] = tr;

            UpdateTrackerPosition(tr);
            return tr;
        }

        public void RemoveTracker(int id)
        {
            ScreenTracker sc;
            _activeTrackers.TryGetValue(id, out sc);
            if (sc != null) 
            { 
                _activeTrackers.Remove(id);
                Destroy(sc.gameObject);
            }
        }

        public void Clear()
        {
            if (_activeTrackers.Count == 0) { return; }

            ScreenTracker[] tracks = new ScreenTracker[_activeTrackers.Count];
            _activeTrackers.Values.CopyTo(tracks, 0);
            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] == null) { continue; }
                Destroy(tracks[i].gameObject);
            }
            _activeTrackers.Clear();
        }
    }
}
