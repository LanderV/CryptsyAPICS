using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptsy
{
    public class CryptsyResponse
    {
        public bool Success { get; private set; }
        public JToken Data { get; private set; }
        public string Error { get; private set; }

        //Only sometimes available:
        public Int64 OrderID { get; private set; }
        public string Info { get; private set; }

        public static CryptsyResponse ReadFromJObject(JObject o)
        {
            var r = new CryptsyResponse()
            {
                Success = o.Value<int>("success") == 1,
                Error = o.Value<string>("error")
            };
            r.Data = o.Value<JToken>("return");
            r.OrderID = o.Value<Int64?>("orderid") ?? -1;
            r.Info = o.Value<string>("moreinfo") ?? r.Error;


            return r;
        }
    }
}
