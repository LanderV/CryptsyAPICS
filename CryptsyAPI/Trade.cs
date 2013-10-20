/* Developed by Lander V
 * Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)
 * 
 * Many thanks to HaasOnline!
 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptsy
{
    public class Trade
    {
        public Int64 TradeID { get; private set; }
        public DateTime DateTimeUTC { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal TotalPrice { get; private set; }
        public Order.ORDER_TYPE InitiateOrderType { get; private set; }
        public Order.ORDER_TYPE TradeType { get; private set; }
        public Int64 OrderID { get; private set; } //Original order id this trade was executed against. -1 if not applicable
        public decimal Fee { get; private set; }

        public static Trade ReadFromJObject(JObject o)
        {
            if (o == null)
                return null;

            /*
             * Method: mytrades (authenticated)
             * Present properties: tradeid, tradetype, datetime, tradeprice, quantity, total, fee, initiate_ordertype, order_id
             * 
             * Method: markettrades (authenticated)
             * Present properties: tradeid, datetime, tradeprice, quantity, total, initiate_ordertype
             * 
             * Method: singlemarketdata (public)
             * Present properties: id, time, price, quantity, total
             * 
             */


            Trade trade = new Trade()
            {
                TradeID = o.Value<Int64?>("id") ?? o.Value<Int64>("tradeid"),
                DateTimeUTC = TimeZoneInfo.ConvertTime(o.Value<DateTime?>("time") ?? o.Value<DateTime>("datetime"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc),
                UnitPrice = o.Value<decimal ?>("price") ?? o.Value<decimal>("tradeprice"),
                Quantity = o.Value<decimal>("quantity"),
                TotalPrice = o.Value<decimal>("total"),
                Fee = o.Value<decimal?>("fee") ?? 0,

                //If not present: UNKNOWN; if present: Buy or Sell
                InitiateOrderType = o.Value<String>("initiate_ordertype") == null ? Order.ORDER_TYPE.NA : (o.Value<String>("initiate_ordertype").ToLower() == "buy" ? Order.ORDER_TYPE.BUY : Order.ORDER_TYPE.SELL),
                TradeType = o.Value<String>("tradetype") == null ? Order.ORDER_TYPE.NA : (o.Value<String>("tradetype").ToLower() == "buy" ? Order.ORDER_TYPE.BUY : Order.ORDER_TYPE.SELL),
                OrderID = o.Value<Int64?>("order_id") ?? -1
            };




            return trade;
        }


        public static List<Trade> ReadMultipleFromJArray(JArray array)
        {
            if (array == null)
                return new List<Trade>();

            List<Trade> trades = new List<Trade>();
            foreach (JObject o in array)
                trades.Add(Trade.ReadFromJObject(o));

            return trades;
        }
    }
}
