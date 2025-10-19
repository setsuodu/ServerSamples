using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using System;
using System.Threading.Tasks;

namespace Client
{
    public class Program
    {
        private static GameServer net;

        static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                // 连接逻辑服，这一步是Client中启动必须执行的

                using (var client = await ConnectClient())
                {
                    net = new GameServer(client);
                    await net.Start();

                    Console.WriteLine($"StartServer");
                    Console.ReadLine();
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nException while trying to run client: {e.Message}");
                Console.WriteLine("Make sure the silo the client is trying to connect to is running.");
                Console.WriteLine("\nPress any key to exit.");
                //Console.ReadKey();
                Console.ReadLine();
                return 1;
            }
        }

        private static async Task<IClusterClient> ConnectClient()
        {
            IClusterClient client;
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options => //通过配置，找对应Key的Silo
                {
                    options.ClusterId = "ClusterId";
                    options.ServiceId = "ServiceId";
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            await client.Connect();
            Console.WriteLine("Client successfully connected to silo host \n");
            return client;
        }
    }
}