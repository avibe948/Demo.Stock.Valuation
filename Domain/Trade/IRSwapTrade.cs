

namespace Domain.Trade
{
    public class IRSwapTrade : Trade
    {
        public IRSwapTrade() : base() { }

        public decimal FixedRate { get; set; }

        public IRSwapTrade(long tradeId, string underlying, decimal quantity, string coutnerparty) :
            base(tradeId, underlying, quantity, coutnerparty, TradeType.IRSwap)
        { }

    }


}
