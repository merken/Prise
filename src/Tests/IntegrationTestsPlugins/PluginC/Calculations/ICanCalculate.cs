using DomainForPluginC;

namespace PluginC.Calculations
{
    public interface ICanCalculate
    {
        decimal DoCalculation(decimal a, decimal b);
    }

    public class DivideCalculation : ICanCalculate
    {
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

        public decimal DoCalculation(decimal a, decimal b)
        {
            return this.discountService.ApplyDiscount((a * b));
        }
    }
}