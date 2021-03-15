namespace Domain.Trade
{
    public abstract class Trade : ITrade {

        protected Trade() { } // used in CsvHelper for instantiation.
        protected Trade(long tradeId, string underlying, decimal quantity, string coutnerparty, TradeType tradeType)
        {
            TradeId = tradeId;
            Underlying = underlying;
            Quantity = quantity;
            Counterparty = coutnerparty;
            TradeType = tradeType;
        }

        /* to craete immutable I could have made these protected set , but couldn't be bothers using CsvReader UsingContructor in the helper. TODO  */
        public long TradeId { get; /*protected*/ set; }
        public string Underlying { get; /*protected*/ set; }
        public decimal Quantity { get; /*protected*/ set; }
        public string Counterparty { get; /*protected*/ set; }
        public TradeType TradeType { get; /*protected*/ set; }

    }
    public interface ITrade
    {
        public long TradeId { get; set; }
        public decimal Quantity { get; set; }
        public string Counterparty { get; set; }
        public TradeType TradeType { get; set; }
        public string Underlying { get; /*protected*/ set; }

    }

    public class ResultRecord
    {
        public string TradeId { get; set; }
        public string PV { get; set; }
        public string ErrorMessage {get;set;}
    }


}
