using Domain.Trade;
using System;

namespace Domain.Trade
{
    public class FxOptionTrade : Trade
    {
        public FxOptionTrade() : base() { }
        public string CurrencyPair => base.Underlying;
        public decimal Strike { get; private set; }

        public OptionType OptionType { get; private set; }

        public DateTime ExpiryDate { get; set; }
        public FxOptionTrade(long tradeId, string currencyPair, decimal strike, OptionType optionType, DateTime expiryDate,
            decimal quantity, string coutnerparty, TradeType tradeType) :
            base(tradeId, currencyPair, quantity, coutnerparty, TradeType.FxOption)
        {

            Strike = strike;
            OptionType = optionType;
            ExpiryDate = expiryDate;
        }
    }


}
