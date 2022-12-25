using MyEcs;
using EcsStructs;
using UnityEngine;

namespace EcsSystems
{
    public class PlayerAnimationSystem : IUpd
    {
        public const string
       ANIMATOR_FRONT = "Front",
       ANIMATOR_SIDE = "Side";

        Filter<PlayerData, NetData> playerFilter = null;
        public void Upd()
        {
            foreach (var i in playerFilter)
            {
                ref var playerData = ref playerFilter.Get1(i);
                ref var netData = ref playerFilter.Get2(i);

                var moveDir = playerData.velocity.GetXZ();
                var aimDir = (playerData.rotation * Vector3.forward).GetXZ();
                var front = Vector2.Dot(aimDir, moveDir);
                var side = Vector2.Dot(aimDir, Vector2.Perpendicular(moveDir));

                playerData.animator.SetFloat(ANIMATOR_FRONT, front, 0.2f, Time.deltaTime);
                playerData.animator.SetFloat(ANIMATOR_SIDE, side, 0.2f, Time.deltaTime);

                netData.animatorProvider.RpcSetAnimatorFloatProperty(ANIMATOR_FRONT, front);
                netData.animatorProvider.RpcSetAnimatorFloatProperty(ANIMATOR_SIDE, side);
            }
        }
    }
}