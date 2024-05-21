using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathUtils;
using TMPro;
using UnityEngine;


namespace Grid
{
    public enum EventMode
    {
        SetTarget,
        SetSource,
        RemoveAdd,
        Add,
        Remove
    }

    public class GridManager : MonoBehaviour
    {
        [SerializeField] private PlaneEventsHandler _planeEventsHandler;
        [SerializeField] private PathfindingManager _pathfindingManager;
        [SerializeField] private TMP_Dropdown _toolSelection;
        [SerializeField] private MapManager _mapManager;
        [SerializeField] private EventMode _mode;


        private void OnEnable()
        {
            _toolSelection.ClearOptions();
            _toolSelection.AddOptions(Enum.GetNames(typeof(EventMode)).ToList());
            _toolSelection.onValueChanged.AddListener(SetTool);

            _planeEventsHandler.OnTileInteract.AddListener(ProcessGridEvent);
        }


        private void SetTool(int tool)
        {
            _mode = (EventMode)tool;
        }


        private void OnDisable()
        {
            _toolSelection.onValueChanged.RemoveListener(SetTool);


            _planeEventsHandler.OnTileInteract.RemoveListener(ProcessGridEvent);
        }


        private void ProcessGridEvent(Vector2Int point)
        {
            if (!_mapManager.hasLoadedMap) return;
            if (!_mapManager.size.Contains(point))
            {
                Debug.LogWarning($"Point {point} is out of bounds {_mapManager.size}");
                return;
            }

            _pathfindingManager.StopAndClear();
            bool success = _mode switch
            {
                EventMode.SetTarget => _pathfindingManager.TrySetTarget(point),
                EventMode.SetSource => _pathfindingManager.TrySetSource(point),
                EventMode.RemoveAdd => _mapManager.TryAddRemoveObstacle(point),
                EventMode.Add => _mapManager.TryAddObstacle(point),
                EventMode.Remove => _mapManager.TryRemoveObstacle(point),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (!success)
            {
                Debug.LogWarning($"Operation {_mode} is not successfull for point {point} ");
            }
        }


        public void Init(Vector2Int size)
        {
            _planeEventsHandler.Init(size);
        }
    }
}