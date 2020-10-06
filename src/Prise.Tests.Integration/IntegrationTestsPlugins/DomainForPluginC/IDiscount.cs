namespace DomainForPluginC
{
    public interface IDiscount
    {
        decimal Amount { get; }
    }

    public class Discount : IDiscount
    {
        private readonly decimal amount;
        public Discount(decimal amount)
        {
            this.amount = amount;
        }

        public decimal Amount => amount;
    }
}