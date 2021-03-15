using Domain.Trade;
using FluentValidation;

namespace Cibc.Core.Validators.Trades
{
    public class IRSwapTradeValidator : AbstractValidator<IRSwapTrade>
    {
        public IRSwapTradeValidator()
        {
            RuleFor(x => x.FixedRate).NotEmpty().GreaterThan(-1);
            RuleFor(x => x.Counterparty).NotEmpty();
            RuleFor(x => x.Quantity).NotEmpty();
            RuleFor(x => x.TradeId).NotEmpty();
            RuleFor(x => x.TradeType).NotEmpty();
            RuleFor(x => x.Underlying).NotEmpty();
        }
    }

}
