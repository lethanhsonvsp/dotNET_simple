using System;
using System.Threading.Tasks;

namespace SimpleOPCUAClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var opcUaClient = new OpcUaClient("opc.tcp://localhost:4840", "http://yourorganization.org/SimpleOpcUaServer/");
                await opcUaClient.ConnectAsync();

                await opcUaClient.WriteValueAsync("WaterFlowRate", 100.0);
                await opcUaClient.ReadValueAsync("WaterFlowRate");

                await opcUaClient.WriteValueAsync("PumpTemperature", 30.0);
                await opcUaClient.ReadValueAsync("PumpTemperature");

                await opcUaClient.WriteValueAsync("PumpStatus", 'A'.ToString());
                await opcUaClient.ReadValueAsync("PumpStatus");

                opcUaClient.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
