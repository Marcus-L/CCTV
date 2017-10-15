using System;
using System.Threading.Tasks;

namespace CCTV.Core.App
{
    /// <summary>
    /// .NET Core Console App wrapper
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var service = new Service();
            var serviceTask = service.Start();

            // allow shutdown with CTRL+C
            Console.CancelKeyPress += async (obj, evt) =>
            {
                evt.Cancel = true;
                await service.Stop();
            };

            // exit when service stops
            Task.WaitAll(serviceTask);
        }
    }
}
