using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEcs
{
    public interface ISystem { }

    public interface IInit : ISystem
    {
        void Init();
    }

    public interface IUpd : ISystem
    {
        void Upd();
    }

    public interface IDispose : ISystem, IDisposable { }
}

