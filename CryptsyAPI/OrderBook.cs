using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptsy
{
    public class OrderBook
    {
        public List<Order> BuyOrders { get; private set; }
        public List<Order> SellOrders { get; private set; }

        public OrderBook()
        {
            BuyOrders = new List<Order>();
            SellOrders = new List<Order>();
        }

        public OrderBook(List<Order> buyOrders, List<Order> sellOrders)
        {
            this.BuyOrders = buyOrders;
            this.SellOrders = sellOrders;
        }

        public static OrderBook ReadFromJObject(JObject o, Int64 marketID)
        {
            OrderBook ob = new OrderBook();
            foreach (var order in o["sellorders"])
            {
                ob.SellOrders.Add(Order.ReadFromJObject(order as JObject,marketID,Order.ORDER_TYPE.SELL));
            }

            foreach (var order in o["buyorders"])
            {
                ob.BuyOrders.Add(Order.ReadFromJObject(order as JObject,marketID, Order.ORDER_TYPE.BUY));
            }

            return ob;
        }

        //returns: <marketID,OrderBook>
        public static Dictionary<Int64,OrderBook> ReadMultipleFromJObject(JObject o)
        {
            Dictionary<Int64,OrderBook> markets = new Dictionary<Int64,OrderBook>();
            foreach (var market in o.Children())
            {
                Int64 marketID = market.First().Value<Int64>("marketid");
                markets.Add(marketID,ReadFromJObject(market.First() as JObject,marketID));
            }
            return markets;
        }
    }
}
