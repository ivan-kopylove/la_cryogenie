using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace La_cryogenie
{
    public static class Globals
    {
        public static DataTable listOfActiveNotices { get; set; }
        public static bool isBotPaused { get; set; }
    }
}
