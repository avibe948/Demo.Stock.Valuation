using FluentValidation.Results;
using System.Collections.Generic;

namespace Domain.Trade
{
    public class TradeValidationResult
    {
        public Trade Trade { get; set; }
        public ValidationResult ValidationResults { set; get; }
    }


}
