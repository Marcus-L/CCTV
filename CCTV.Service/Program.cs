using Topshelf;

namespace CCTV.Service
{
    /// <summary>
    /// A thin Windows Service wrapper around the .NET Core code
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<Core.Service>(s =>
                {
                    s.ConstructUsing(() => new Core.Service());
                    s.WhenStarted(p => p.Start());
                    s.WhenStopped(p => p.Stop().Wait());
                });
                x.RunAsLocalSystem();
                x.StartAutomatically();
                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(0);
                    r.OnCrashOnly();
                });

                x.SetDescription("CCTV: A Chromecast TV Power Utility");
                x.SetDisplayName("CCTV");
                x.SetServiceName("CCTV");
            });
        }
    }
}
