using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOA.connection
{
    public interface IDynamicObject
    {
        void Create(Object param);
        Object GetInnerObject();
        bool IsValidate();
        void Release();
    }
}
