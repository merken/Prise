using MyHost.Infrastructure;

namespace Tests
{
    public class UseLocalCommandLineArguments : ICommandLineArguments
    {
        public bool UseNetwork
        {
            get
            {
                return false;
            }
        }
    }
}
