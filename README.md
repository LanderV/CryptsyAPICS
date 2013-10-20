CryptsyAPI
============

C# API for Cryptsy.
Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)

Many thanks to HaasOnline!


Usage
============

Many calls need a market ID. This market ID can be retrieved by calling GetMarketInfo(string currencyCode1, string currencyCode2, bool basicInfoOnly = false).
With this method you can search a market with currency codes like "BTC" and "LTC". Currency codes are case insensitive and the order of the two doesn't matter. The method will return a MarketInfo-object with some info about the market, inter alia the market ID.
When no third argument is passed to GetMarketInfo(), or is it false, the returned MarketInfo-object will also contain the recent trades and top 20 buy & sell orders. If basicInfoOnly (the third argument) is set to true, it will not contain the recent trades and the top 20 buy & sell orders.

To retrieve the full orderbook, just call GetFullOrderBook() on the MarketInfo-object.

No markets are hard coded in the API. However, for your convenience, all currency codes supported by Cryptsy at this moment (20/20/2013) are available in the class CurrencyCodes.

All other methods should be self explanatory. If not, contact me :)


License
============

The code can be freely used under the MIT license (attached). When you find the code usefull, you can donate some change as sign of your appreciation :)
