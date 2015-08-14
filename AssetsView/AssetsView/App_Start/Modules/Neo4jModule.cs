using Neo4jClient;
using Ninject.Activation;
using Ninject.Modules;
using System;
using System.Configuration;

namespace AssetsView.App_Start.Modules
{
    public class Neo4jModule : NinjectModule
    {
        /// <summary>Loads the module into the kernel.</summary>
        public override void Load()
        {
            Bind<IGraphClient>().ToMethod(InitNeo4jClient).InSingletonScope();
        }

        private static IGraphClient InitNeo4jClient(IContext context)
        {
            var neo4jUri = new Uri(ConfigurationManager.ConnectionStrings["assets"].ConnectionString);
            var graphClient = new GraphClient(neo4jUri);
            graphClient.Connect();
            return graphClient;
        }
    }
}