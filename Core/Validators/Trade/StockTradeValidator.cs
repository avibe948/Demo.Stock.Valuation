using Domain.Trade;
using FluentValidation;

namespace Cibc.Core.Validators.Trades
{
    public class StockTradeValidator : AbstractValidator<StockTrade>
    {
        public StockTradeValidator()
        {
            RuleFor(x => x.Ticker).NotEmpty();
            RuleFor(x => x.TradeId).NotEmpty().GreaterThan(0);
            RuleFor(x => x.TradeType).NotEmpty();
            RuleFor(x => x.Quantity).NotEmpty().GreaterThan(0);
            RuleFor(x => x.Counterparty).NotEmpty();
          
        }
    }

}

