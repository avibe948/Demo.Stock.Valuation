using Domain.TradeTypes;
using FluentValidation;
using System.Collections.Generic;
using System.Text;

namespace Cibc.PricingValidators.Trades
{

    public class IRSwapTradeValidator : TradeBaseValidator<IRSwapTrade>
    {
        public IRSwapTradeValidator()
        {
            RuleFor(x => x.Underlying).NotEmpty();
            RuleFor(x => x.FixedRate).NotEmpty();
        }
    }

}
