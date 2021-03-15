using Domain.Trade;
using FluentValidation;

namespace Cibc.Core.Validators.Trades
{
    public class FxOptionTradeValidator : AbstractValidator<FxOptionTrade>
    {
        public FxOptionTradeValidator()
        {
            RuleFor(x => x.CurrencyPair).NotEmpty();
            RuleFor(x => x.Strike).NotEmpty();
            RuleFor(x => x.ExpiryDate).NotEmpty();
            RuleFor(x => x.Quantity).NotEmpty();
            RuleFor(x => x.TradeType).NotEmpty();
            RuleFor(x => x.Strike).NotEmpty();
            RuleFor(x => x.Counterparty).NotEmpty();
            RuleFor(x => x.OptionType).NotEmpty();

        }
    }

}
