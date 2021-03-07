using Domain.TradeTypes;
using FluentValidation;

namespace Cibc.PricingValidators.Trades
{
    public class FxForwardTradeValidator : TradeBaseValidator<FxForwardTrade>
    {
        public FxForwardTradeValidator()
        {
            RuleFor(x => x.CurrencyPair).NotEmpty();
            RuleFor(x => x.Strike).NotEmpty();
            RuleFor(x => x.ExpiryDate).NotEmpty();
        }
    }

}
