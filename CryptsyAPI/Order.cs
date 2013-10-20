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
    public class Order
    {
        public decimal Price { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal Total { get; private set; }
        public enum ORDER_TYPE { BUY, SELL, NA }
        public ORDER_TYPE OrderType { get; private set; }
        public DateTime? CreatedUTC { get; private set; }
        public decimal OriginalQuantity { get; private set; } //Original Total Order Quantity
        public Int64 MarketID { get; private set; }


        public static Order ReadFromJObject(JObject o, Int64 marketID = -1, ORDER_TYPE orderType = ORDER_TYPE.NA)
        {
            if (o == null)
                return null;


            var order = new Order()
            {
                Price = o.Value<decimal>("price"),
                Quantity = o.Value<decimal>("quantity"),
                Total = o.Value<decimal>("total"),
                OriginalQuantity = o.Value<decimal?>("orig_quantity") ?? -1,
                MarketID = o.Value<Int64?>("marketid") ?? marketID,

                //If ordertype is present, use it, if not: use the ordertype passed to the method
                OrderType = o.Value<string>("ordertype") == null ? orderType : (o.Value<string>("ordertype").ToLower() == "buy" ? ORDER_TYPE.BUY : ORDER_TYPE.SELL)

            };

            order.CreatedUTC = o.Value<DateTime?>("created");
            if (order.CreatedUTC != null)
                order.CreatedUTC = TimeZoneInfo.ConvertTime((DateTime)order.CreatedUTC, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc); //Convert to UTC



            return order;
        }

        public static List<Order> ReadMultipleFromJArray(JArray array, Int64 marketID = -1)
        {
            if (array == null)
                return new List<Order>();

            List<Order> orders = new List<Order>();
            foreach (JObject o in array)
                orders.Add(Order.ReadFromJObject(o, marketID));

            return orders;
        }



    }
}
