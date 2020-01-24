using System;

namespace DomainForPluginC
{
    public interface IDiscountService
    {
        decimal ApplyDiscount(decimal result);
    }

    public class DiscountService : IDiscountService
    {
        private readonly IDiscount discount;
        public DiscountService(IDiscount discount)
        {
            this.discount = discount;

        }
        public decimal ApplyDiscount(decimal result)
        {
            return result * this.discount.Amount;
        }
    }
}