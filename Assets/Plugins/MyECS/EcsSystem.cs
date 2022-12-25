using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MyEcs
{
    public sealed class EcsSystem : IInit, IUpd, IDispose
    {
        const int defSize = 16;

        readonly EcsWorld _world;
        readonly string _name;

        Dictionary<Type, object> _dictonary = new Dictionary<Type, object>();
        Dictionary<string, int> _sysByName = new Dictionary<string, int>();

        MyList<ISystem> _allSubSys = new MyList<ISystem>(defSize);
        MyList<bool> _sysEnable = new MyList<bool>(defSize);

        MyList<IUpd> _updSubSys = new MyList<IUpd>(defSize);

        public EcsSystem(EcsWorld world, string name = null)
        {
            _world = world;
            _name = name;
        }

        public EcsSystem Add(ISystem subSys)
        {
            var id = _allSubSys.Add(subSys);
            _sysEnable.Add(true);

            if (subSys is EcsSystem sys && sys._name != null)
            {
                if (_sysByName.ContainsKey(sys._name)) throw new Exception($"system name \"{sys._name}\" is bisy");
                _sysByName.Add(sys._name, id);
            }

            if (subSys is IUpd upd)
                _updSubSys.Add(upd);

            return this;
        }

        public EcsSystem OneFrame<T>() where T : struct
        {
            return this.Add(new OneFrameClass<T>());
        }

        public EcsSystem Inject(object data)
        {
            if (data == null) { throw new Exception("Null data"); }

            var type = data.GetType();

            if (!_dictonary.ContainsKey(type))
            {
                _dictonary.Add(type, data);
            }
            else
            {
                _dictonary[type] = data;
            }

            return this;
        }

        public int GetSystemByName(string name)
        {
            if (_sysByName.TryGetValue(name, out var id))
                return id;

            throw new Exception($"System \"{name}\" not exist");
        }
        public void UnsafeSetSystem(int i, bool status)
        {
            _sysEnable._data[i] = status;
        }

        void RealInject()
        {
            FiltersInject();

            foreach (var type in _dictonary.Keys)
            {
                var data = _dictonary[type];
                for (int i = 0, iMax = _allSubSys.Count; i < iMax; i++)
                {
                    if (_allSubSys._data[i] is EcsSystem sys)
                    {
                        sys.Inject(data);
                    }

                    var fields = _allSubSys._data[i].GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                    for (int j = 0, jMax = fields.Length; j < jMax; j++)
                    {
                        if (fields[j].FieldType == type)
                        {
                            fields[j].SetValue(_allSubSys._data[i], data);
                        }
                    }
                }
            }
        }

        void FiltersInject()
        {
            Dictionary<Type, Filter> pull = new Dictionary<Type, Filter>();

            //Наполнение фильтров
            for (int i = 0, iMax = _allSubSys.Count; i < iMax; i++)
            {
                var fields = _allSubSys._data[i].GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                for (int j = 0, jMax = fields.Length; j < jMax; j++)
                {
                    var field = fields[j];

                    if (field.FieldType.IsSubclassOf(typeof(Filter)))
                    {
                        if (pull.TryGetValue(field.FieldType, out var obj))
                        {
                            field.SetValue(_allSubSys._data[i], obj);
                        }
                        else
                        {
                            Filter f = (Filter)Activator.CreateInstance(field.FieldType);
                            pull.Add(field.FieldType, f);

                            f._world = _world;
                            f.Init();

                            field.SetValue(_allSubSys._data[i], f);
                        }
                    }
                }
            }

            pull.Clear();
        }

        public void Init()
        {
            Inject(_world);
            RealInject();

            for (int i = 0, iMax = _allSubSys.Count; i < iMax; i++)
            {
                if (_sysEnable._data[i] && _allSubSys._data[i] is IInit init)
                    init.Init();
            }
        }

        public void Upd()
        {
            if (_updSubSys.Count < 1) return;

            for (int i = 0, iMax = _updSubSys.Count; i < iMax; i++)
            {
                // try
                // {
                    if (_sysEnable._data[i])
                        _updSubSys._data[i].Upd();
                // }
                // catch (Exception ex)
                // {
                    //TODO
                //     UnityEngine.Debug.LogError(_updSubSys._data[i].GetType().Name + ex.HelpLink +
                //     ex.Source);
                //     throw ex;
                // }
            }
        }

        public void Dispose()
        {
            for (int i = 0, iMax = _allSubSys.Count; i < iMax; i++)
            {
                if (_allSubSys._data[i] is IDispose init)
                    init.Dispose();
            }
        }

        class OneFrameClass<T> : IUpd where T : struct
        {
            Filter<T> filter = null;

            public void Upd()
            {
                foreach (var i in filter)
                {
                    filter.GetEntity(i).Del<T>();
                }
            }

        }
    }
}
