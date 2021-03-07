using System;

namespace Domain.TradeTypes
{
    
    public enum TradeType {Stock, FxOption, IRSwap , FxForward}
    public enum OptionType {Put, Call}

    public abstract class Trade
    {
        protected Trade(long tradeId,string underlying, decimal quantity, string coutnerparty, TradeType tradeType)
        {
            TradeId = tradeId;
            Underlying = underlying;
            Quantity = quantity;
            Coutnerparty = coutnerparty;
            TradeType = tradeType;
        }

        
        public long TradeId { get; protected set; }
        public string Underlying { get; set; }
        public decimal Quantity { get; protected set; }
        public string Coutnerparty { get; protected set; }
        public TradeType TradeType { get; protected set; }

    }

    public class IRSwapTrade :Trade
    {
        public string Underlying { get => base.Underlying;}

        public decimal FixedRate { get; private set; }

        public IRSwapTrade(long tradeId,string underlying, decimal quantity, string coutnerparty) : 
            base(tradeId, underlying, quantity, coutnerparty, TradeType.IRSwap) {}
        
    }
    public class StockTrade : Trade
    {
        public StockTrade(long tradeId, string ticker, decimal quantity, string coutnerparty, TradeType tradeType) : 
            base(tradeId, ticker, quantity, coutnerparty, TradeType.Stock){}

        public string Ticker => Underlying; 
    }
    public class FxForwardTrade : Trade
    {
        public string CurrencyPair => base.Underlying;
        public decimal Strike { get; protected set; }
        public DateTime ExpiryDate { get; protected set; }

        public FxForwardTrade(long tradeId, string currencyPair, decimal strike, DateTime expiryDate, decimal quantity, string coutnerparty, TradeType tradeType) : 
            base(tradeId, currencyPair, quantity, coutnerparty, TradeType.FxForward){

            Strike = strike;
            ExpiryDate = expiryDate; 
        }
    }
    public class FxOptionTrade : Trade
    {
        public string CurrencyPair => base.Underlying;
        public decimal Strike { get; private set; }

        public OptionType OptionType { get; private set; }

        public DateTime ExpiryDate { get; set; }
        public FxOptionTrade(long tradeId, string currencyPair, decimal strike, OptionType optionType, DateTime expiryDate,
            decimal quantity, string coutnerparty, TradeType tradeType) :
            base(tradeId, currencyPair, quantity, coutnerparty, TradeType.FxOption){

            Strike = strike;
            OptionType = optionType;
            ExpiryDate = expiryDate;
        }
    }
}
