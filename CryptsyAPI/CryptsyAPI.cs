/* Developed by Lander V
 * Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)
 * 
 * Many thanks to HaasOnline!
 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Security.Cryptography;
using System.Globalization;

namespace Cryptsy
{

    public class CryptsyAPI
    {
        // Public variable
        public string LastMessage { get; private set; }

        // Private variables
        private List<MarketInfo> marketInfos; //Save basic marketinfo, to quickly find the marketID of a given currency-pair
        private string publicKey = "";
        private UInt32 _nonce;
        private HMACSHA512 hmac = new HMACSHA512();
        private static readonly Encoding encoding = Encoding.UTF8;



        // Constructor
        public CryptsyAPI(string _publicKey, string _privateKey)
        {
            LastMessage = "";
            publicKey = _publicKey;
            _nonce = UnixTime.Now;
            hmac.Key = encoding.GetBytes(_privateKey);
        }

        // Public methods
        public void SetKeys(string publicKey, string privateKey)
        {
            this.publicKey = publicKey;
            hmac.Key = StringToByteArray(privateKey);
        }

        /* Returns marketinfo for the market for these two currencies (order doesn't matter)
         * Returns null if no market was found with these currencies
         * If basicInfoOnly = true, recent trades and top orders will not be loaded
         */
        public MarketInfo GetMarketInfo(string currencyCode1, string currencyCode2, bool basicInfoOnly = false)
        {
            if (marketInfos == null)
            {
                marketInfos = GetOpenMarkets(basicInfoOnly: true);//Don't load recent trades and orderbook for all markets
            }

            currencyCode1 = currencyCode1.ToUpper();
            currencyCode2 = currencyCode2.ToUpper();
            MarketInfo market = null;
            foreach (MarketInfo m in marketInfos)
            {
                if (currencyCode1 == m.PrimaryCurrencyCode && currencyCode2 == m.SecondaryCurrencyCode
                    || currencyCode2 == m.PrimaryCurrencyCode && currencyCode1 == m.SecondaryCurrencyCode)
                {
                    market = m;
                    break;
                }
            }
            if (market == null)
                return null;

            if (basicInfoOnly)
                return market;
            else
                return GetMarketInfo(market.MarketID); //Get all info from the requested market

        }

        public MarketInfo GetMarketInfo(Int64 marketID)
        {
            var args = new Dictionary<string, string>()
            {
                 { "marketid",marketID.ToString()},
                 {"method", "singlemarketdata"}
            };


            CryptsyResponse answer = CryptsyQuery(false, args);

            if (answer.Success)
            {
                //Answer is returned as an array of markets (with only the requested market)
                MarketInfo market = MarketInfo.ReadMultipleFromJObject(answer.Data as JObject, basicInfoOnly: false)[0];
                return market;
            }
            else
                return null;
        }

        //Set basicInfoOnly=true to skip recent trades & top 20 buy and sell orders
        public List<MarketInfo> GetOpenMarkets(bool basicInfoOnly = false)
        {
            CryptsyResponse answer = CryptsyQuery(false, new Dictionary<string, string>() { { "method", "marketdatav2" } });

            List<MarketInfo> markets = null;

            if (answer.Success)
            {
                markets = MarketInfo.ReadMultipleFromJObject(answer.Data as JObject, basicInfoOnly);
                return markets;
            }
            else
            {
                return null;
            }

        }

        public OrderBook GetOrderBook(Int64 marketID)
        {
            CryptsyResponse response = CryptsyQuery(false, new Dictionary<string, string>() { { "method", "singleorderdata" }, { "marketid", marketID.ToString() } });
            if (response.Success)
            {
                //Response is an array of markets, with only the requested market
                return OrderBook.ReadMultipleFromJObject(response.Data as JObject)[marketID];
            }
            else
                return null;
        }



        //Returns: Dictionary<MarketID, OrderBook>
        public Dictionary<Int64, OrderBook> GetAllOrderBooks()
        {
            CryptsyResponse response = CryptsyQuery(false, new Dictionary<string, string>() { { "method", "orderdata" } });
            if (response.Success)
            {
                //Response is an array of markets, with only the requested market
                return OrderBook.ReadMultipleFromJObject(response.Data as JObject);
            }
            else
                return null;
        }


        public Balance GetBalance()
        {
            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { { "method", "getinfo" } });

            if (response.Success)
                return Balance.ReadFromJObject(response.Data as JObject);
            else
                return null;
        }

        public decimal CalculateFee(Order.ORDER_TYPE orderType,decimal quantity, decimal price)
        {
            if (orderType == Order.ORDER_TYPE.NA) return -1;

            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { { "method", "calculatefees" },{"ordertype",orderType==Order.ORDER_TYPE.BUY ? "Buy" : "Sell"},{"quantity",quantity.ToString(CultureInfo.InvariantCulture)}, {"price",price.ToString(CultureInfo.InvariantCulture)} });

            if (response.Success)
            {
                JObject o = (JObject)response.Data;
                if (o != null)
                    return o.Value<decimal>("fee");
                else
                    return -1;
            }
            else
                return -1;
        }

        public string GenerateNewAddress(string currencyCode)
        {
                      CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { { "method", "generatenewaddress" }, { "currencycode", currencyCode } });

            if (response.Success)
            {
                JObject o = (JObject)response.Data;
                if (o != null)
                    return o.Value<string>("address");
                else
                    return null;
            }
            else
                return null;
        }

        public List<Trade> GetMarketTrades(Int64 marketID)
        {
            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { { "method", "markettrades" }, { "marketid", marketID.ToString() } });
            if (response.Success)
            {
                //Response is an array of markets, with only the requested market
                return Trade.ReadMultipleFromJArray(response.Data as JArray);
            }
            else
                return null;
        }

        public List<Trade> GetMyTrades(Int64 marketID, uint limitResults = 200)
        {
            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { { "method", "mytrades" }, { "marketid", marketID.ToString() }, { "limit", limitResults.ToString() } });
            if (response.Success)
            {
                //Response is an array of markets, with only the requested market
                return Trade.ReadMultipleFromJArray(response.Data as JArray);
            }
            else
                return null;
        }

        public List<Order> GetMyOrders(Int64 marketID)
        {
            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { { "method", "myorders" }, { "marketid", marketID.ToString() }});
            if (response.Success)
            {
                //Response is an array of markets, with only the requested market
                return Order.ReadMultipleFromJArray(response.Data as JArray,marketID);
            }
            else
                return null;
        }

        public List<Order> GetAllMyOrders()
        {
            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { { "method", "allmyorders" }});
            if (response.Success)
            {
                //Response is an array of markets, with only the requested market
                return Order.ReadMultipleFromJArray(response.Data as JArray);
            }
            else
                return null;
        }

        public List<Trade> GetAllMyTrades()
        {
            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { { "method", "allmytrades" }});
            if (response.Success)
            {
                //Response is an array of markets, with only the requested market
                return Trade.ReadMultipleFromJArray(response.Data as JArray);
            }
            else
                return null;
        }

        //Gets withdrawals & deposits
        public List<Transaction> GetTransactions()
        {
            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { { "method", "mytransactions" } });

            if (response.Success)
                return Transaction.ReadMultipleFromJArray(response.Data as JArray);
            else
                return null;
        }

        public class OrderResult
        {
            public bool Success;
            public Int64 OrderID;
            public string Message;
        }

        public OrderResult CreateOrder(Int64 marketID, Order.ORDER_TYPE orderType, decimal quantity, decimal price)
        {
            if (orderType == Order.ORDER_TYPE.NA) return new OrderResult() { Success = false, OrderID = -1, Message = "orderType must be BUY or SELL." };

            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { 
            { "method", "createorder" },
            {"marketid",marketID.ToString() },
            {"ordertype",orderType==Order.ORDER_TYPE.BUY?"Buy":"Sell"},
            {"quantity",quantity.ToString(CultureInfo.InvariantCulture)},
            {"price",price.ToString(CultureInfo.InvariantCulture)}});


            if (response.Success)
                return new OrderResult() { Success = true, OrderID = response.OrderID, Message = response.Info };
            else
                return new OrderResult() { Success = false, OrderID = -1, Message = response.Error };
        }

        public OrderResult CancelOrder(Int64 orderID)
        {
            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { 
            { "method", "cancelorder" },
            {"orderid",orderID.ToString() }});

            return new OrderResult() { Success = response.Success, OrderID = orderID, Message = response.Info ?? Convert.ToString(response.Data) };
        }

        //Returns null if unsuccessful, otherwise list of info-messages
        public List<string> CancelAllMarketOrders(Int64 marketID)
        {
            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { 
            { "method", "cancelmarketorders" },
            {"marketid",marketID.ToString() }});

            if (response.Success)
            {
                List<string> r = new List<string>();
                if (response.Data != null) //Any orders canceled?
                {
                    foreach (var o in response.Data)
                        r.Add(Convert.ToString(o));
                }

                return r;
            }
            else
            {
                return null;
            }

        }

        //Returns null if unsuccessful, otherwise list of info-messages
        public List<string> CancelAllOrders()
        {
            CryptsyResponse response = CryptsyQuery(true, new Dictionary<string, string>() { 
            { "method", "cancelallorders" }});

            if (response.Success)
            {
                List<string> r = new List<string>();
                if (response.Data != null) //Any orders canceled?
                {
                    foreach (var o in response.Data)
                        r.Add(Convert.ToString(o));
                }

                return r;
            }
            else
            {
                return null;
            }

        }



        //Private methods
        private static string BuildPostData(Dictionary<string, string> d)
        {
            string s = "";
            for (int i = 0; i < d.Count; i++)
            {
                var item = d.ElementAt(i);
                var key = item.Key;
                var val = item.Value;

                s += String.Format("{0}={1}", key, HttpUtility.UrlEncode(val));

                if (i != d.Count - 1)
                    s += "&";
            }
            return s;
        }
        private CryptsyResponse CryptsyQuery(bool authenticate, Dictionary<string, string> args)
        {
            var dataStr = BuildPostData(args);
            string url;
            if (authenticate)
            {
                url = "https://www.cryptsy.com/api";
                //Add extra nonce-header
                args["nonce"] = GetNonce().ToString();
                dataStr = BuildPostData(args);
            }
            else
            {
                url = "http://pubapi.cryptsy.com/api.php?" + dataStr;
            }



            var request = WebRequest.Create(new Uri(url));
            if (request == null)
                throw new Exception("Non HTTP WebRequest");

            if (authenticate)
            {
                var data = encoding.GetBytes(dataStr);
                request.Method = "POST";
                request.Headers.Add("Key", publicKey);
                request.Headers.Add("Sign", ByteToString(hmac.ComputeHash(data)).ToLower());

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                var reqStream = request.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            else
            {
                request.Method = "GET";
            }

            request.Timeout = 15000;


            var response = request.GetResponse();
            var resStream = response.GetResponseStream();
            var resStreamReader = new StreamReader(resStream);


            string sResult = resStreamReader.ReadToEnd();
            LastMessage = sResult;

            var result = JObject.Parse(sResult);
            CryptsyResponse cryptsyresponse = CryptsyResponse.ReadFromJObject(result);
            if(cryptsyresponse.Error!=null && cryptsyresponse.Error.Trim().Length>0)
                LastMessage = cryptsyresponse.Error;

            return cryptsyresponse;
        }

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
                sbinary += buff[i].ToString("X2"); /* hex format */
            return sbinary;
        }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        private UInt32 GetNonce()
        {
            return _nonce++;
        }
    }

}

