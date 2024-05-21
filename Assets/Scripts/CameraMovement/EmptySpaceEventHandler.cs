using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class EmptySpaceEventHandler : MonoBehaviour, IPointerClickHandler
{
    public CameraController cameraController;
    public UnityEvent<float> DeltaScroll { get; } = new();
    public UnityEvent<Vector2> DeltaDrag { get; } = new();
    public UnityEvent StartDrag { get; } = new();
    public UnityEvent EndDrag { get; } = new();

    public UnityEvent<float> WheelDrag { get; } = new();
    public UnityEvent<PointerEventData.InputButton> OnClick { get; } = new();

    private bool _dragged;
    private bool _wheelDragged;
    private float _deltaScroll;

    private void Update()
    {
        _deltaScroll += Input.mouseScrollDelta.y;
        if (Input.GetMouseButtonDown(1) && !_wheelDragged)
        {
            OnBeginDrag(Input.mousePosition);
        }

        if (Input.GetMouseButtonDown(2) && !_dragged)
        {
            OnWheelBeginDrag(Input.mousePosition);
        }

        if (_dragged)
        {
            OnDrag(Input.mousePosition);
        }

        if (_wheelDragged)
        {
            OnWheelDrag(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(1))
        {
            OnEndDrag();
        }

        if (Input.GetMouseButtonUp(2))
        {
            OnWheelEndDrag();
        }
    }


    private void FixedUpdate()
    {
        if (_deltaScroll == 0) return;
        DeltaScroll.Invoke(_deltaScroll);
        _deltaScroll = 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick.Invoke(eventData.button);
    }


    private Vector2 _dragBeginPos;
    private Vector2 _dragCurrentScreenPos;

    private Vector2 CurDeltaDrag
    {
        get
        {
            var pos = cameraController.camera.ScreenToWorldPoint(new Vector3(_dragCurrentScreenPos.x,
                _dragCurrentScreenPos.y, cameraController.height));
            return new Vector2(pos.x, pos.z) - _dragBeginPos;
        }
    }

    public void OnBeginDrag(Vector2 screenPos)
    {
        var pos = cameraController.camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y,
            cameraController.height));
        _dragBeginPos = new Vector2(pos.x, pos.z);
        _dragged = true;
        StartDrag.Invoke();
    }

    public void OnEndDrag()
    {
        _dragged = false;
        EndDrag.Invoke();
    }

    public void OnDrag(Vector2 screenPos)
    {
        if ((screenPos - _dragCurrentScreenPos).sqrMagnitude < 2) return;
        _dragCurrentScreenPos = screenPos;
        DeltaDrag.Invoke(CurDeltaDrag);
    }

    public void OnWheelBeginDrag(Vector2 screenPos)
    {
        var pos = cameraController.camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y,
            cameraController.height));
        _dragBeginPos = new Vector2(pos.x, pos.z);
        _dragCurrentScreenPos = screenPos;
        _wheelDragged = true;
    }

    public void OnWheelEndDrag()
    {
        _wheelDragged = false;
    }

    public void OnWheelDrag(Vector2 screenPos)
    {
        if ((screenPos - _dragCurrentScreenPos).sqrMagnitude < 2) return;

        WheelDrag.Invoke(_dragCurrentScreenPos.x - screenPos.x);
        _dragCurrentScreenPos = screenPos;
    }

    public void UpdateCamToWorld()
    {
        if (!_dragged) return;
        DeltaDrag.Invoke(CurDeltaDrag);
    }
}