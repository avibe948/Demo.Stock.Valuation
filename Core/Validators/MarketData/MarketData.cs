using System;
using System.Collections.Generic;
using System.Text;
using Domain.MarketData;
using FluentValidation;

namespace Core.Validators
{
    public class MarketDataValidator : AbstractValidator<MarketDataItem>
    {
        public MarketDataValidator()
        {
            RuleFor(x => x.Price);
        }
    }
}
