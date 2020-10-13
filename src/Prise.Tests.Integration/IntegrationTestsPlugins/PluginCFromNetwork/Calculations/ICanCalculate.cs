using DomainForPluginC;

namespace PluginCFromNetwork.Calculations
{
    public interface ICanCalculate
    {
        string ServiceName { get; }
        decimal DoCalculation(decimal a, decimal b);
    }

    public class DivideCalculation : ICanCalculate
    {
        public string ServiceName => "DivideCalculation";
        public decimal DoCalculation(decimal a, decimal b)
        {
            return a / b;
        }
    }

    public class MultiplyCalculation : ICanCalculate
    {
        private readonly IDiscountService discountService;
        public MultiplyCalculation(IDiscountService discountService)
        {
            this.discountService = discountService;
        }

        public string ServiceName => "MultiplyCalculation";

        public decimal DoCalculation(decimal a, decimal b)
        {
            return this.discountService.ApplyDiscount((a * b));
        }
    }
}