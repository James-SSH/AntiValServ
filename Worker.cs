namespace AntiValServ;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting Anti-Valorant Service");
            Process[] GameProcesses = Process.GetProcessesByName("VALORANT");
            if (GameProcesses != null)
            {
                foreach (var process in GameProcesses)
                {
                    _logger.LogCritical("Valorant Detected To be Running");
                    process.Kill();
                    process.WaitForExit();
                    Process win64ShippingSubTask = Process.GetProcessesByName("VALORANT-Win64-Shipping").First();
                    win64ShippingSubTask.Kill();
                    win64ShippingSubTask.WaitForExit();
                }
            }
            Process[] processes = Process.GetProcessesByName("RiotClientServices");
            foreach (Process process in processes)
            {
                var cmdline = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}").Get().Cast<ManagementBaseObject>().SingleOrDefault()["CommandLine"]?.ToString();
                if (cmdline != null && cmdline.ToLower().Contains("valorant"))
                {
                    _logger.LogCritical("Valorant Detected Opinion Rejected");
                    process.Kill();
                    process.WaitForExit();
                    var usrs = Registry.Users.GetSubKeyNames();
                    foreach (string usr in usrs) {
                        RegistryKey keys = Registry.Users.OpenSubKey($"{usr}\\SOFTWARE\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache");
                        if (keys == null) { continue; }
                        
                        var seyk = keys.GetValueNames().Reverse().ToArray();
                        foreach (string key in seyk)
                        {
                            if (keys.GetValue(key) != null && keys.GetValue(key).ToString().ToLower().Contains("valorant"))
                            {
                                _logger.LogWarning("Valorant Install Detected");
                                var installLocation = key.Split("\\live\\");
                                _logger.LogInformation($"Valorant Install Found at: {installLocation.First()}");
                                if (Directory.Exists(installLocation.First()))
                                {
                                    try
                                    {
                                        Directory.Delete(installLocation.First(), true);
                                    } 
                                    catch (UnauthorizedAccessException uae) 
                                    {
                                        _logger.LogInformation($"Read-Only file caught in exception: {uae.Message}");
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            await Task.Delay(1_800_000);
        }
    }
}
