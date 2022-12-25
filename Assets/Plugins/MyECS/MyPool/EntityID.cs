using UnityEngine;

namespace MyEcs.GoPool
{

    [SelectionBase]
    public class EntityID : MonoBehaviour
    {
        internal Entity entity;

        private void OnDestroy()
        {
            if (!entity.IsDestroyed())
            {
                entity.Destroy();
            }
        }

        internal void Clean()
        {
            entity = Entity.NULL;
        }

        public Entity GetEntity()
        {
            if (entity.IsDestroyed()) throw new System.Exception("Null Entity");

            return entity;
        }

    }
}

namespace EcsStructs
{
    using MyEcs.GoPool;

    public struct GoEntityProvider
    {
        public EntityID provider;
        public MyEcsPool pool;

        public void Recycle()
        {
            provider.Clean();
            provider.gameObject.SetActive(false);

            if (pool != null)
            {
                pool.Storage(provider.gameObject);
            }
            else
            {
                MonoBehaviour.Destroy(provider.gameObject, 0.01f);
            }
        }
    }
}