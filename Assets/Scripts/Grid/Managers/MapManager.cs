using System;
using System.Collections.Generic;
using PathUtils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Grid
{
    public class MapManager:MonoBehaviour
    {
        private IMapData _mapData;
     
        [SerializeField] private Vector2Int _size = new(5, 5);
        [SerializeField] private TMP_Dropdown _mapOptions;
        [SerializeField] private TMP_Dropdown _mapTypeOptions;
        [SerializeField] private GridDisplay _display;
        [SerializeField] private TMP_InputField _xSizeInput;
        [SerializeField] private TMP_InputField _ySizeInput;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private Toggle _cameraMode;
        [SerializeField] private Button _generateMap;
        [SerializeField] private PathfindingManager _pathfindingManager;
        [SerializeField] private GridManager _gridManager;
        private bool _hasLoadedMap;
    //    private MapLoader _mapLoader;
        public IMapData map => _mapData;
        public bool hasLoadedMap => _hasLoadedMap;
        public Vector2Int size => _size;

        private void Awake()
        {
          //  _mapLoader = new MapLoader();
        }

        private void OnEnable()
        {
            _mapTypeOptions.ClearOptions();
            //_mapTypeOptions.AddOptions(_mapLoader.mapTypes);
            UpdateMapOptions(_mapTypeOptions.value);
            _mapTypeOptions.onValueChanged.AddListener(UpdateMapOptions);
            _generateMap.onClick.AddListener(LoadEmptyMap);
            _cameraMode.onValueChanged.AddListener(SetCamMode);
        }
        
        private void UpdateMapOptions(int type)
        {
            _mapOptions.onValueChanged.RemoveListener(LoadMap);
            _mapOptions.ClearOptions();
            _mapOptions.AddOptions(noneOption);
            //_mapOptions.AddOptions(_mapLoader.GetMapOptions(type));
            _mapOptions.value = 0;
            _mapOptions.onValueChanged.AddListener(LoadMap);
        }
        
        private static readonly List<String> noneOption = new List<string>() { "-" };

        private void LoadMap(int option)
        {
            // if (option != 0)
            //     SetUpMap(_mapLoader.GetMap(_mapTypeOptions.value, option - 1));
        }
        private void SetCamMode(bool isPerspective)
        {
            _cameraController.SetMode(isPerspective
                ? CameraController.CamMode.Perspective
                : CameraController.CamMode.Orthographic);
        }

        private void Clear()
        {
            for (int i = 0; i < _mapData.size.x; i++)
            {
                for (int j = 0; j < _mapData.size.y; j++)
                {
                    if (!_mapData[new Vector2Int(i, j)])
                        _display.Clear(new Vector2Int(i, j));
                }
            }
        }

        private void SetUpMap(IMapData mapData)
        {

            if (_hasLoadedMap)
            {
                Clear();
                _pathfindingManager.ClearAll();
            }
            
            _mapData = mapData;
            _hasLoadedMap = true;
            _size = _mapData.size;
            _pathfindingManager.Init(_size);
            _gridManager.Init(_size);
            _display.Init(_size, _mapData);
            _cameraController.Set(3, Mathf.Max(_size.x, _size.y), new Rect(0, 0, _size.x, _size.y));
        }

        private void OnDisable()
        {
            _mapOptions.onValueChanged.RemoveListener(LoadMap);
            _generateMap.onClick.RemoveListener(LoadEmptyMap);
            _mapTypeOptions.onValueChanged.RemoveListener(UpdateMapOptions);
            _cameraMode.onValueChanged.RemoveListener(SetCamMode);
        }

        private void LoadEmptyMap()
        {
            if (!int.TryParse(_xSizeInput.text, out var xSize)) return;
            if (!int.TryParse(_ySizeInput.text, out var ySize)) return;
            if (ySize == 0 || xSize == 0) return;
            var mapData = new MapData(new Vector2Int(xSize, ySize));
            SetUpMap(mapData);
        }

        public bool TryRemoveObstacle(Vector2Int point)
        {
            if (_mapData[point]) return false;
            _mapData[point] = true;
            _display.Clear(point);
            return true;
        }

        public bool TryAddObstacle(Vector2Int point)
        {
            if (!_mapData[point]) return false;

            _pathfindingManager.ClearPoint(point);
            _display.Display(point, DisplayObject.Obstacle);
            _mapData[point] = false;
            return true;
        }

        public bool TryAddRemoveObstacle(Vector2Int point)
        {
            return _mapData[point] ? TryAddObstacle(point) : TryRemoveObstacle(point);
        }

    }
}