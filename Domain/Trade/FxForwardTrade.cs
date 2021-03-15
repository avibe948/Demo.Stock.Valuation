using Domain.Trade;
using System;

namespace Domain.Trade
{
    public class FxForwardTrade : Trade
    {
        public FxForwardTrade() : base() { }
        public string CurrencyPair => base.Underlying;
        public decimal Strike { get; protected set; }
        public DateTime ExpiryDate { get; protected set; }

        public FxForwardTrade(long tradeId, string currencyPair, decimal strike, DateTime expiryDate, decimal quantity, string coutnerparty, TradeType tradeType) :
            base(tradeId, currencyPair, quantity, coutnerparty, TradeType.FXForward)
        {

            Strike = strike;
            ExpiryDate = expiryDate;
        }
    }


}
