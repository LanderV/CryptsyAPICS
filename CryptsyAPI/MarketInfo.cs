using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
/* Developed by Lander V
 * Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)
 * 
 * Many thanks to HaasOnline!
 */

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptsy
{
    public class MarketInfo
    {
        public string PrimaryCurrencyCode { get; private set; }
        public string PrimaryCurrencyName { get; private set; }
        public string SecondaryCurrencyCode { get; private set; }
        public string SecondaryCurrencyName { get; private set; }
        public string Label { get; private set; } //Name of the market, for example AMC,BTC
        public Int64 MarketID { get; private set; }
        public decimal Volume { get; private set; }
        public Trade LastTrade { get; private set; }
        public List<Trade> RecentTrades { get; private set; } //null if MatketInfo was loaded with basicInfoOnly = true

        public OrderBook OrderBook { get; private set; } /* Can contain: - null if MarketInfo was loaded with basicInfoOnly = true
                                                          *              - top 20 buy & sell orders, if the marketinfo was loaded with basicInfoOnly = false (or not present)
                                                          *              - full orderbook, if method LoadFullOrderBook was called
                                                          */


        //If basicInfoOnly flag is set to true, RecentTrades & OrderBook (top 20) won't be loaded
        //This can be used to reduce unnecessary memory usage
        public static List<MarketInfo> ReadMultipleFromJObject(JObject o, bool basicInfoOnly = false)
        {
            List<MarketInfo> markets = new List<MarketInfo>();
            foreach (var market in o["markets"])
            {
                markets.Add(ReadFromJObject(market.First() as JObject, basicInfoOnly));
            }
            return markets;
        }

        //Loads the full order book of Cryptsy, instead of top 20 orders
        public OrderBook GetFullOrderBook(CryptsyAPI api)
        {
            OrderBook = api.GetOrderBook(MarketID);
            return OrderBook;
        }

        //If basicInfoOnly flag is set to true, RecentTrades & OrderBook (top 20) won't be loaded
        //This can be used to reduce unnecessary memory usage
        private static MarketInfo ReadFromJObject(JObject o, bool basicInfoOnly = false)
        {
            MarketInfo marketInfo = new MarketInfo()
            {
                MarketID = o.Value<Int64>("marketid"),
                Label = o.Value<string>("label"),
                PrimaryCurrencyCode = o.Value<string>("primarycode"),
                PrimaryCurrencyName = o.Value<string>("primaryname"),
                SecondaryCurrencyCode = o.Value<string>("secondarycode"),
                SecondaryCurrencyName = o.Value<string>("secondaryname"),
                Volume = o.Value<decimal>("volume"),
                //CreationTimeUTC = TimeZoneInfo.ConvertTime(o.Value<DateTime>("created"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc)
            };

            if (!basicInfoOnly)
            {

                marketInfo.RecentTrades = new List<Trade>();
                marketInfo.OrderBook = new OrderBook();

                foreach (var t in o["recenttrades"])
                {
                    Trade trade = Trade.ReadFromJObject(t as JObject);
                    marketInfo.RecentTrades.Add(trade);
                    marketInfo.LastTrade = trade;
                }

                //orderbook is returnd as array of markets (with only the requested market)
                marketInfo.OrderBook = OrderBook.ReadFromJObject(o,marketInfo.MarketID);

            }

            return marketInfo;
        }

    }
}
