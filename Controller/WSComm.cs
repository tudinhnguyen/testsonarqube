using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Teechip.Controller
{
    class WSComm
    {
        static WSComm _instance = null;
        public static WSComm Instance()
        {
            if (_instance == null)
            {
                _instance = new WSComm();
            }

            return _instance;
        }

        WSComm()
        {
        }
    }
}
