using System.Threading.Tasks;

namespace Cibc.Pricing.ValuationModels
{
    public interface IValuationModel
    {
        Task CalcMeasures();
    }
}