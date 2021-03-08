using System;
using System.Threading.Tasks;

namespace Cibc.Pricing.ValuationModels
{
    public interface IValuationModel
    {
        ValuationResult Calc(PricingInputs input);
    }
}