using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    Transform _tf = null;
    Rigidbody _rb = null;
    StaticData _stData = null;
    PlayerProvider _provider = null;

    Vector2 _moveDir;
    Quaternion _verticalRotation = Quaternion.Euler(0, 0, 0);
    Quaternion _horizontalRotation = Quaternion.Euler(0, 0, 0);

    void Start()
    {
        _tf = transform;
        _rb = GetComponent<Rigidbody>();
        _provider = GetComponent<PlayerProvider>();

        _stData = World.Singleton.StaticData;
    }

    public void SetMoveDirection(Vector2 newDirection)
    {
        _moveDir = newDirection.normalized;
    }
    public Vector2 GetMoveDir() => _moveDir;
    public void SetLookOffset(Vector2 lookOffset)
    {
        var horizontalRotationOffset = new Quaternion(0, lookOffset.x, 0, 1);
        var verticalRotationOffset = new Quaternion(lookOffset.y, 0, 0, 1);

        _provider.horizontalTransform.rotation *= horizontalRotationOffset;

        var newVertRotation = _provider.followTarget.rotation * verticalRotationOffset;
        var verticalAngle = newVertRotation.eulerAngles.x;
        if (verticalAngle < 180 && verticalAngle > _stData.upMaxAngle ||
            verticalAngle > 180 && verticalAngle < 360 - _stData.downMaxAngle)
        {
            return;
        }
        _provider.followTarget.rotation = newVertRotation;
    }

    void Update()
    {

    }
    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        var speed = _stData.playerSpeed;

        //смещение игрока относительно взгляда
        var moveDir = _moveDir;
        var aimDir = (_provider.horizontalTransform.rotation * Vector3.forward).GetXZ_v2();
        var front = Vector2.Dot(aimDir, moveDir);
        var side = Vector2.Dot(aimDir, Vector2.Perpendicular(moveDir));

        _rb.velocity = new Vector3(front, 0, side) * speed;
    }
}
