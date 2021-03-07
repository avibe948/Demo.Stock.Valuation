using Domain.TradeTypes;
using FluentValidation;

namespace Cibc.PricingValidators.Trades
{
    public class TradeBaseValidator<T> : AbstractValidator<T> where T:Trade
    {
        public TradeBaseValidator()
        {
            RuleFor(x => x.TradeId).NotEmpty();
            RuleFor(x => x.Underlying).NotEmpty();
            RuleFor(x => x.TradeType).NotEmpty();
            RuleFor(x => x.Quantity).NotEmpty();
            RuleFor(x => x.Coutnerparty).NotEmpty();
        }
    }

}
