using Domain.Trade;

namespace Domain.Trade
{
    public class StockTrade :  Trade
    {
        public StockTrade() : base() { }
        public StockTrade(string ticker, long tradeId, string coutnerparty, decimal quantity, TradeType tradeType) :
            base(tradeId, ticker, quantity, coutnerparty, TradeType.Stock)
        { }

        public string Ticker => Underlying;
    }


}
