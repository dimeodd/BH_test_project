using MyEcs;
using EcsStructs;

namespace EcsSystems
{
    public class MoveSystem : IUpd
    {
        Filter<PlayerData, RigidbodyData> moveFilter = null;

        public void Upd()
        {
            foreach (var i in moveFilter)
            {
                ref var playerData = ref moveFilter.Get1(i);
                ref var rbData = ref moveFilter.Get2(i);

                rbData.rigidbody.velocity = playerData.velocity;
            }
        }
    }
}