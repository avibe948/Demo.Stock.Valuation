using Domain.TradeTypes;
using FluentValidation;

namespace Cibc.PricingValidators.Trades
{
    public class FxOptionTradeValidator : TradeBaseValidator<FxOptionTrade>
    {
        public FxOptionTradeValidator()
        {
            RuleFor(x => x).NotEmpty();
        }
    }

}
