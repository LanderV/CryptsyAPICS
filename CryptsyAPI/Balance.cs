using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Cryptsy
{
    public class Balance
    {
        //<currency_code,available_balance>
        public Dictionary<string, decimal> BalanceAvailable { get; private set; }
        //<currency_code,balance_on_hold>
        public Dictionary<string, decimal> BalanceOnHold { get; private set; }
        public DateTime LastUpdateUTC { get; private set; }
        public Int32 OpenOrderCount { get; private set; }

        public static Balance ReadFromJObject(JObject o)
        {
            if (o == null)
                return null;

            var bal = new Balance();
            bal.BalanceAvailable = JsonConvert.DeserializeObject<Dictionary<string,decimal>>(o["balances_available"].ToString());
            bal.BalanceOnHold = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(o["balances_hold"].ToString());
            bal.LastUpdateUTC = TimeZoneInfo.ConvertTime(o.Value<DateTime>("serverdatetime"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc);
            bal.OpenOrderCount = o.Value<Int32>("openordercount");

            return bal;
        }

        public decimal GetCurrencyBalance(string currencyCode)
        {
            return BalanceAvailable[currencyCode.ToUpper()];
        }

    }
}
