using UnityEngine;

namespace MyEcs.GoPool
{
    public static class Helper
    {
        /// <summary>
        /// Создаёт GoEntityProvider(ent); EntityID(go)
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="go"></param>
        public static ref EcsStructs.GoEntityProvider LinkObjects(Entity ent, GameObject go)
        {
            if (ent.IsDestroyed()) throw new System.Exception("Null Entity");

            if (!go.TryGetComponent<EntityID>(out var entityID))
            {
                entityID = go.AddComponent<EntityID>();
            }
            entityID.entity = ent;

            ref var entProv = ref ent.Get<EcsStructs.GoEntityProvider>();
            entProv.provider = entityID;
            return ref entProv;
        }

        public static void DestroyLinkedObject(EntityID prov)
        {
            var ent = prov.entity;
            DestroyLinkedObject(ent);
        }
        public static void DestroyLinkedObject(Entity ent)
        {
            if (ent.IsDestroyed()) throw new System.Exception("Null Entity");
            if (ent.Contain<EcsStructs.GoEntityProvider>())
            {
                ref var entProv = ref ent.Get<EcsStructs.GoEntityProvider>();
                entProv.Recycle();
            }

            ent.Destroy();
        }
    }

}