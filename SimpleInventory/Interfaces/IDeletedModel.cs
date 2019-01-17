using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Interfaces
{
    interface IDeletedModel<T>
    {
        void ModelWasDeleted(T model);
    }
}
