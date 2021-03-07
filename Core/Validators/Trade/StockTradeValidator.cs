using Domain.TradeTypes;
using FluentValidation;

namespace Cibc.PricingValidators.Trades
{
    public class StockTradeValidator : TradeBaseValidator<StockTrade>
    {

        public StockTradeValidator()
        {
            RuleFor(x => x.Ticker).NotEmpty();
        }
    }

}
