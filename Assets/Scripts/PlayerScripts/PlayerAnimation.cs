using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    PlayerProvider _provider;
    Transform _locTransform;
    Vector2 _lastPos, _moveDelta;

    void Start()
    {
        _provider = GetComponent<PlayerProvider>();
        _locTransform = transform;
        _lastPos = _locTransform.position.GetXZ_v2();
    }

    void FixedUpdate()
    {
        var currPos = _locTransform.position.GetXZ_v2();
        _moveDelta = Vector2.Lerp(_moveDelta, (currPos - _lastPos).normalized, 0.8f);

        var aimDir = (_provider.horizontalTransform.rotation * Vector3.forward).GetXZ_v2();

        var front = Vector2.Dot(aimDir, _moveDelta);
        var side = Vector2.Dot(aimDir, Vector2.Perpendicular(_moveDelta));

        _provider.animator.SetFloat("Front", front, 0.1f, Time.fixedDeltaTime);
        _provider.animator.SetFloat("Side", side, 0.1f, Time.fixedDeltaTime);

        _lastPos = currPos;
    }
}