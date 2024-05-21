using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utils;


[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private EmptySpaceEventHandler _emptySpaceEventHandler;
    [SerializeField] private float minCamDist = 3;
    [SerializeField] private float maxCamDist = 10;
    [SerializeField] private float initialCamDistRatio = 0.5f;
    [SerializeField] private float distChangeSpeed = 1;
    [SerializeField] private Rect constraints;

    public float height => transform.position.y;
    public Camera camera => _cam;
    private float _distRatio;
    private Camera _cam;
    private Vector3 _lastMove;
    private bool _moving;

    private void Start()
    {
        _distRatio = initialCamDistRatio;
        _cam = GetComponent<Camera>();
        Init();
    }

    public void Set(float newMinCamDist, float newMaxCamDist, Rect newConstraints)
    {
        minCamDist = newMinCamDist;
        maxCamDist = newMaxCamDist;
        constraints = newConstraints;

        Init();
    }

    private void Init()
    {
        if (_cam.orthographic)
            _cam.orthographicSize = Mathf.Lerp(minCamDist, maxCamDist, _distRatio);
        var pos2D = (constraints.min + constraints.max) / 2;
        var transform1 = _cam.transform;
        transform1.position = new Vector3(pos2D.x, _cam.orthographic ? 20 : maxCamDist, pos2D.y);
    }

    private void Rotate(float delta)
    {
        var rotationRatio = 720 * delta / Screen.width;
        transform.RotateAround(transform.position, Vector3.up, rotationRatio);
    }

    private void OnEnable()
    {
        _emptySpaceEventHandler.DeltaDrag.AddListener(MoveCamera);
        _emptySpaceEventHandler.DeltaScroll.AddListener(ChangeDist);
        _emptySpaceEventHandler.EndDrag.AddListener(EndMove);
        _emptySpaceEventHandler.StartDrag.AddListener(StartMove);
        _emptySpaceEventHandler.WheelDrag.AddListener(Rotate);
    }

    private void OnDisable()
    {
        _emptySpaceEventHandler.DeltaDrag.RemoveListener(MoveCamera);
        _emptySpaceEventHandler.DeltaScroll.RemoveListener(ChangeDist);
        _emptySpaceEventHandler.EndDrag.RemoveListener(EndMove);
        _emptySpaceEventHandler.StartDrag.RemoveListener(StartMove);
        _emptySpaceEventHandler.WheelDrag.RemoveListener(Rotate);
    }

    public enum CamMode
    {
        Perspective,
        Orthographic
    }

    public void SetMode(CamMode mode)
    {
        _cam.orthographic = mode == CamMode.Orthographic;
        transform.rotation = Quaternion.Euler(mode == CamMode.Orthographic ? 90 : 60, 0, 0);
    }

    private void ChangeDist(float deltaScroll)
    {
        _distRatio = Mathf.Clamp(_distRatio - deltaScroll * (_distRatio + 0.01f) * distChangeSpeed, 0, 1);
        if (_cam.orthographic)
            _cam.orthographicSize = Mathf.Lerp(minCamDist, maxCamDist, _distRatio);
        else
        {
            var position = _cam.transform.position;
            position = new Vector3(position.x,
                Mathf.Lerp(minCamDist, maxCamDist, _distRatio), position.z);
            _cam.transform.position = position;
        }

        if (!_moving) return;
        _emptySpaceEventHandler.UpdateCamToWorld();
    }

    private void StartMove()
    {
        _moving = true;
    }

    private void EndMove()
    {
        _moving = false;
    }

    private void MoveCamera(Vector2 deltaMove)
    {
        _lastMove = deltaMove;
        var newPosition = transform.position - new Vector3(deltaMove.x, 0, deltaMove.y);
        newPosition = constraints.Clamp(newPosition);
        transform.position = newPosition;
    }
}