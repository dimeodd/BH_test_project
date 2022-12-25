using UnityEngine;

namespace MyEcs.GoPool
{
    public class MyEcsPool
    {
        GameObject _prefab;
        MyList<GameObject> _pool;
        internal EcsWorld _world;
        int count;

        public MyEcsPool(GameObject prefab, int count, EcsWorld world)
        {
            _world = world;
            _prefab = prefab;
            _pool = new MyList<GameObject>(8);

            for (int i = 0; i < count; i++)
            {
                Prepare();
            }
        }
        void Prepare()
        {
            var go = MonoBehaviour.Instantiate(_prefab);
            go.name += count++;
            go.SetActive(false);
            _pool.Add(go);
        }

        public GameObject Create(Vector3 pos, Entity ownerEntity)
        {
            if (_pool.Count <= 0)
            {
                Prepare();
            }
            var go = _pool._data[0];
            _pool.DeleteReplaced(0);

            go.transform.position = pos;
            go.SetActive(true);

            ref var entProv = ref Helper.LinkObjects(ownerEntity, go);
            entProv.pool = this;

            return go;
        }

        public void Storage(GameObject target)
        {
            target.SetActive(false);

            if (_pool.Contain(target)) throw new System.Exception("уже есть в пуле: " + target.name);

            _pool.Add(target);
        }
    }
}