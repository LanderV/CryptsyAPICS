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
    public class Transaction //Deposit or Withdrawal
    {
        public string Currency{get; private set;}
        public DateTime DateTimeUTC { get; private set; }
        public enum TRANSACTIONTYPE { DEPOSIT, WITHDRAWAL };
        public TRANSACTIONTYPE TransactionType { get; private set; }
        public String Address { get; private set; } //Address to which the deposit posted or Withdrawal was sent
        public decimal Amount { get; private set; } //Not including fees
        public decimal Fee { get; private set; }

        public static Transaction ReadFromJObject(JObject o)
        {
            if (o == null)
                return null;

            Transaction t = new Transaction()
            {
                Currency = o.Value<string>("currency"),
                DateTimeUTC = TimeZoneInfo.ConvertTime(o.Value<DateTime>("datetime"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc),
                TransactionType = o.Value<string>("type").ToLower() == "deposit" ? TRANSACTIONTYPE.DEPOSIT : TRANSACTIONTYPE.WITHDRAWAL,
                Address = o.Value<string>("address"),
                Amount = o.Value<decimal>("amount"),
                Fee = o.Value<decimal>("fee")
            };

            return t;
        }

        public static List<Transaction> ReadMultipleFromJArray(JArray array)
        {
            if (array == null)
                return new List<Transaction>();

            List<Transaction> transactions = new List<Transaction>();
            foreach (JObject o in array)
                transactions.Add(Transaction.ReadFromJObject(o));

            return transactions;
        }
    }
}
