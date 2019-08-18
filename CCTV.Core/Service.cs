using GoogleCast;
using GoogleCast.Channels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CCTV.Core
{
    public class Service
    {
        private Settings Settings;
        private bool ShutdownRequested = false;
        private Task RunLoopTask;
        private Timer DiscoveryTimer;

        private void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("g")}: {message}");
        }

        public Task Start()
        {
            Settings = Settings.Load("Settings.json");
            RunLoopTask = RunLoop();

            // run discovery periodically when TVs are M.I.A.
            DiscoveryTimer = new Timer(async s => await DiscoverDevices(), 
                null, 10000, 10000);

            return RunLoopTask;
        }

        private async Task RunLoop()
        {
            // run initial discovery
            await Task.Delay(500);
            await DiscoverDevices();

            while (!ShutdownRequested)
            {
                foreach (var tv in Settings.TVs)
                {
                    // skip TVs that are not available
                    if (tv.IP == null) continue;

                    var recv = new Receiver
                    {
                        IPEndPoint = tv.IP,
                        FriendlyName = tv.Name
                    };
                    var sender = new Sender();
                    try
                    {
                        await sender.ConnectAsync(recv);
                        var rchan = sender.GetChannel<IReceiverChannel>();
                        var status = await rchan.GetStatusAsync();
                        var application = status.Applications.FirstOrDefault();
                        if (application?.DisplayName == "Backdrop")
                        {
                            tv.SecondsIdle += Settings.PollIntervalSeconds;
                            Log($"Device is idle: {recv.FriendlyName} {recv.IPEndPoint}");

                            // only turn off at exactly the timeout to avoid
                            // turning it off repeatedly
                            if (tv.SecondsIdle == Settings.IdleTimeout)
                            {
                                await RunCommand("TurnOff", tv, tv.OffCommand);
                            }
                        }
                        else
                        {
                            if (tv.SecondsIdle != 0)
                            {
                                await RunCommand("TurnOn", tv, tv.OnCommand);
                            }
                            tv.SecondsIdle = 0;
                            Log($"Device is busy: {application.DisplayName} {recv.IPEndPoint}");

                            // if the volume is at 100% reset it to 50% 
                            if (!(status.Volume.IsMuted ?? false) &&
                                status.Volume.Level == 1)
                            {
                                await rchan.SetVolumeAsync(tv.DefaultVolume);
                            }
                        }
                    }
                    catch
                    {
                        Log($"Device error: {recv.FriendlyName} {recv.IPEndPoint}");

                        // if anything fails, mark the device for rediscovery and go to next
                        tv.IP = null;
                        continue;
                    }
                    sender.Disconnect();
                }
                await Task.Delay(Settings.PollIntervalSeconds * 1000); // poll wait
            }
        }

        private async Task DiscoverDevices()
        {
            // only run discovery if there are any unknown tvs
            if (Settings.TVs.Any(tv => tv.IP == null))
            {
                Log("Discovering Devices...");
                var receivers = await new DeviceLocator().FindReceiversAsync();
                foreach (var recv in receivers)
                {
                    foreach (var tv in Settings.TVs.Where(tv =>
                    tv.Name == recv.FriendlyName && tv.IP == null))
                    {
                        Log($"Discovered {recv.FriendlyName}={recv.IPEndPoint}");
                        tv.IP = recv.IPEndPoint;
                    }
                }
                foreach (var tv in Settings.TVs.Where(tv => tv.IP == null))
                {
                    Log($"TV Missing: {tv.Name}");
                }
                Log("Discovery complete.");
            }
        }

        public async Task Stop()
        {
            Log("Shutting down...");
            ShutdownRequested = true;
            DiscoveryTimer.Dispose();
            await RunLoopTask;
        }

        private async Task RunCommand(string label, TV tv, Command command)
        {
            var tries = 3;
            while (tries-- > 0)
            {
                try
                {
                    var process = new Process()
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = command.File,
                            Arguments = String.Format(command.Arguments, tv.Name, tv.IP.Address),
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    Log($"{label} Command: " + process.StandardOutput.ReadToEnd());
                    return;
                }
                catch (Exception ex)
                {
                    Log($"{label} error: {ex.Message}");

                    // bugger, retry in a bit
                    await Task.Delay(5000);
                }
            }
        }
    }
}
