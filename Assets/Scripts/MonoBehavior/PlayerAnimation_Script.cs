using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation_Script : MonoBehaviour
{
    public const string
        ANIMATOR_FRONT = "Front",
        ANIMATOR_SIDE = "Side";

    PlayerProvider _provider;
    Transform _locTransform;
    Vector2 _lastPos, _moveDelta;

    void Start()
    {
        _provider = GetComponent<PlayerProvider>();
        _locTransform = transform;
        _lastPos = _locTransform.position.GetXZ();
    }

    void FixedUpdate()
    {
        var currPos = _locTransform.position.GetXZ();
        _moveDelta = Vector2.Lerp(_moveDelta, (currPos - _lastPos).normalized, 0.8f);

        var aimDir = (_provider.horizontalTransform.rotation * Vector3.forward).GetXZ();

        var front = Vector2.Dot(aimDir, _moveDelta);
        var side = Vector2.Dot(aimDir, Vector2.Perpendicular(_moveDelta));
        _provider.animator.SetFloat(ANIMATOR_FRONT, front, 0.1f, Time.fixedDeltaTime);
        _provider.animator.SetFloat(ANIMATOR_SIDE, side, 0.1f, Time.fixedDeltaTime);

        _lastPos = currPos;
    }
}