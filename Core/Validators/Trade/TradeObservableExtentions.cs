using Domain.Trade;
using FluentValidation;
using System;
using System.Reactive;
using System.Reactive.Linq; 

namespace Core.Validators.Trades
{
    public static class TradeObservableExtentions
    {
        public static IObservable<TradeValidationResult> ValidateTrades(this IObservable<Trade> valuationResults, IValidator<Trade> tradeValidator)
        {
            _ = tradeValidator ?? throw new ArgumentNullException(nameof(tradeValidator)); 

            return valuationResults.Select(trade =>
            {
                var validationResult= tradeValidator.Validate(trade);

                return new TradeValidationResult() { Trade = trade, ValidationResults = validationResult };
            });
        }
    }


}
