using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class CharController : MonoBehaviour
{
    [SerializeField, Range(0.1f, 1)] private float maxSpeed;
    [SerializeField, Range(0.1f, 1)] private float maxAngularSpeed;
    [SerializeField] private float angularAcceleration = 1;
    [SerializeField] private float acceleration = 1;
    [SerializeField] private float decelerationMultiplier;
    [SerializeField] private Animator _animator;
    private float _speed;
    private float _angularSpeed;
    private bool _running;
    private int _currentPointIdx;
    private List<Vector2> _points;

    private static readonly int ForwardSpeed = Animator.StringToHash("ForwardSpeed");
    private static readonly int TurningSpeed = Animator.StringToHash("TurningSpeed");
    private Vector2 currentPoint => _points[_currentPointIdx];

    private Vector2 nextPoint => _currentPointIdx + 1 < _points.Count
        ? _points[_currentPointIdx + 1]
        : _points[_currentPointIdx];


    public void FixedUpdate()
    {
        if (!_running) return;
        AdjustCourse();
        SetAnimatorParameters();
    }

    private void SetAnimatorParameters()
    {
        _animator.SetFloat(ForwardSpeed, _speed);
        _animator.SetFloat(TurningSpeed, _angularSpeed);
    }

    private void AdjustCourse()
    {
        var curPoint = currentPoint;

        var pos3D = transform.position;
        var pos = new Vector2(pos3D.x, pos3D.z);
        if (_currentPointIdx + 1 < _points.Count && (pos - curPoint).magnitude < 1)
        {
            _currentPointIdx++;
            curPoint = currentPoint;
        }

        var forward3D = transform.forward;
        var forward = new Vector2(forward3D.x, forward3D.z);
        var left = new Vector2(forward3D.z, -forward3D.x);

        var curDir = curPoint - pos;
        var anticipateDir = nextPoint - curPoint;
        var shift = Vector2.Dot(curDir.normalized, left.normalized);
        var alignment = Vector2.Dot(curDir.normalized, forward.normalized);
        var anticipationAlignment = Vector2.Dot(anticipateDir.normalized, curDir.normalized);
        var delta = (Mathf.Max(0, alignment) - 0.5f + Mathf.Max(-1, anticipationAlignment - 1f) * 0.4f +
                     (Mathf.Clamp01(curDir.magnitude) - 1f) * 0.4f * _speed);
        delta = Mathf.Clamp(delta, -1, 1);
        var deltaAngle =
            (shift + (alignment < 0 ? Mathf.Sign(shift) : 0) * 0.5f + (Mathf.Abs(shift) - 1 * _angularSpeed) * 0.4f);
        deltaAngle = Mathf.Clamp(deltaAngle, -1, 1);
        _speed = Mathf.Clamp(_speed + delta * (delta > 0 ? 1 : decelerationMultiplier) *
            angularAcceleration *
            Time.fixedDeltaTime, 0, maxSpeed);

        _angularSpeed = Mathf.Clamp(_angularSpeed + deltaAngle *
            acceleration * Time.fixedDeltaTime, -maxAngularSpeed, maxAngularSpeed);
    }

    public void RunPath(List<Vector2> points)
    {
        _speed = 0;
        _angularSpeed = 0;
        transform.position = new Vector3(points[0].x, 0.05f, points[0].y);
        _animator.enabled = true;
        _currentPointIdx = 0;
        _running = true;
        _points = points;
        gameObject.SetActive(true);
    }

    public void Stop()
    {
        _animator.enabled = false;
        _running = false;
        _points = null;
        gameObject.SetActive(false);
    }
}