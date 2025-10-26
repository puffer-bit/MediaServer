namespace Server.MainServer.Main.Server.Orchestrator.InitialServerLoader
{
    public class InitialServerLoader
    {
        public InitialServerLoaderContext Context { get; set; }

        public InitialServerLoader(InitialServerLoaderContext context)
        {
            Context = context;
        }
    }
}
