using Akka.Actor;
namespace Example.Akka
{
    public interface IActorProvider
    {
        IActorRef ProvideActor();
    }

    public interface IPluginsActorProvider : IActorProvider
    {
        IActorRef ProvideActor();
    }

    public class PluginsActorProvider : IPluginsActorProvider
    {
        private readonly IActorRef actor;
        public PluginsActorProvider(IActorRef actor)
        {
            this.actor = actor;
        }

        public IActorRef ProvideActor() => this.actor;
    }
}