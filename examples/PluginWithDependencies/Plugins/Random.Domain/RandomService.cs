using System;
using Contract;

namespace Random.Domain
{
    public interface IRandomService
    {
        int ProvideRandomNumber();
    }

    public class RandomService : IRandomService
    {
        public int ProvideRandomNumber()
        {
            return new System.Random().Next(0, 100);
        }
    }
}
