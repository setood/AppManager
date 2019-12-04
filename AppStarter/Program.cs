using AppManager;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace AppStarter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Manager.LoadAssemblieFromFolder(ConfigurationManager.AppSettings["ModulesPath"]);
            await Manager.InitializeAsync();
            await Manager.RunAsync();
            await Manager.ShutdownAsync();
            Console.Read();
        }
    }
}
