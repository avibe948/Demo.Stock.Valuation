using Domain.Trade;
using FluentValidation;

namespace Cibc.Core.Validators.Trades
{
    public class PolymorphicTradeValidator : AbstractValidator<Domain.Trade.Trade>
    {
        public PolymorphicTradeValidator()
        {
            When((trade) => trade.TradeType == TradeType.Stock, () =>
            {
                RuleFor(trade => trade).NotNull().SetInheritanceValidator(v =>
                {
                    v.Add<StockTrade>(new StockTradeValidator());
                    v.Add<IRSwapTrade>(new IRSwapTradeValidator());
                    v.Add<FxForwardTrade>(new FxForwardTradeValidator());
                    v.Add<FxOptionTrade>(new FxOptionTradeValidator());
                });

            });

        }
    }

}
