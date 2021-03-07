using Autofac;
using Domain.TradeTypes;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cibc.PricingValidators.Trades
{
    public class AutofacValidatorFactory : ValidatorFactoryBase
    {
        private readonly IComponentContext container;

        public AutofacValidatorFactory(IComponentContext container)
        {
            this.container = container;
        }

        public override IValidator CreateInstance(Type validatorType)
        {
            object instance;
            if (container.TryResolve(validatorType, out instance))
            {
                var validator = instance as IValidator;
                return validator;
            }

            return null;
        }
    }
    public class TradeBaseValidator<T> : AbstractValidator<T> where T:Trade
    {
        public TradeBaseValidator()
        {
            RuleFor(x => x.TradeId).NotEmpty();
            RuleFor(x => x.TradeId).NotEmpty();
            RuleFor(x => x.TradeId).NotEmpty();
            RuleFor(x => x.Quantity).NotEmpty();
            RuleFor(x => x.Coutnerparty).NotEmpty();
        }
    }

    public class StockTradeValidator : TradeBaseValidator<StockTrade>
    {

        public StockTradeValidator()
        {
            RuleFor(x => x.Ticker).NotEmpty();
        }
    }
    public class FxForwardTradeValidator : TradeBaseValidator<FxForwardTrade>
    {
        public FxForwardTradeValidator()
        {
            RuleFor(x => x.CurrencyPair).NotEmpty();
            RuleFor(x => x.Strike).NotEmpty();
            RuleFor(x => x.ExpiryDate).NotEmpty();
        }
    }
    public class FxOptionTradeValidator : TradeBaseValidator<FxOptionTrade>
    {
        public FxOptionTradeValidator()
        {
            RuleFor(x => x).NotEmpty();
        }
    }

    public class IRSwapTradeValidator : TradeBaseValidator<IRSwapTrade>
    {
        public IRSwapTradeValidator()
        {
            RuleFor(x => x.Underlying).NotEmpty();
            RuleFor(x => x.FixedRate).NotEmpty();
        }
    }

}
