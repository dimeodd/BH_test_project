using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation_Script : MonoBehaviour
{
    public const string
        ANIMATOR_FRONT = "Front",
        ANIMATOR_SIDE = "Side";

    Animator _animator;
    Transform _locTransform;
    Vector2 _lastPos;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _locTransform = transform;
        _lastPos = _locTransform.position.GetXZ();
    }

    void Update()
    {
        var currPos = _locTransform.position.GetXZ();

        var moveDir = (currPos - _lastPos).normalized;
        var aimDir = (_locTransform.rotation * Vector3.forward).GetXZ();

        var front = Vector2.Dot(aimDir, moveDir);
        var side = Vector2.Dot(aimDir, Vector2.Perpendicular(moveDir));
        _animator.SetFloat(ANIMATOR_FRONT, front, 0.1f, Time.deltaTime);
        _animator.SetFloat(ANIMATOR_SIDE, side, 0.1f, Time.deltaTime);

        _lastPos = currPos;
    }
}
