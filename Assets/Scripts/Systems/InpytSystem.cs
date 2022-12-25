using MyEcs;
using EcsStructs;
using UnityEngine;

namespace EcsSystems
{
    public class InpytSystem : IUpd
    {
        Filter<InputData, PlayerData, RigidbodyData, TransformData> inputFilter = null;

        StaticData _stData = null;

        public void Upd()
        {
            foreach (var i in inputFilter)
            {
                ref var input = ref inputFilter.Get1(i);
                ref var playerData = ref inputFilter.Get2(i);
                ref var rbData = ref inputFilter.Get3(i);
                ref var tfData = ref inputFilter.Get4(i);

                #region Move

                input.move.Normalize();
                var inputFront = input.move.x;
                var inputSide = input.move.y;
                var speed = _stData.playerSpeed;

                //смещение игрока относительно взгляда
                var moveDir = new Vector2(inputFront, inputSide).normalized;
                var aimDir = (rbData.rigidbody.rotation * Vector3.forward).GetXZ();
                var front = Vector2.Dot(aimDir, moveDir);
                var side = Vector2.Dot(aimDir, Vector2.Perpendicular(moveDir));

                playerData.velocity = new Vector3(front, 0, side) * speed;

                #endregion

                #region Look

                tfData.transform.rotation = Quaternion.Euler(0, input.HorizontalRotation, 0);

                var verticalAngle = input.VerticalRotation;
                if (verticalAngle < 180 && verticalAngle > _stData.upMaxAngle ||
                    verticalAngle > 180 && verticalAngle < 360 - _stData.downMaxAngle)
                {
                    return;
                }
                playerData.followTarget.rotation = Quaternion.Euler(input.VerticalRotation, input.HorizontalRotation, 0);

                #endregion
            }

        }
    }
}