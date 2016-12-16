using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection
{
    public interface IPooledObjSupporter : IDisposable
    {
        void Reset(); //恢复对象为初始状态，当IObjectPool.GiveBackObject时调用
    }
}
