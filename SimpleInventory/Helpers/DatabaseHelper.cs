using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimpleInventory.Helpers
{
    class DatabaseHelper
    {
        bool DoesDatabaseExist()
        {
            return File.Exists("data/inventory.sidb");
        }
    }
}
