using Microsoft.Extensions.DependencyInjection;

// ReSharper disable InconsistentlySynchronizedField

namespace WabiSabiMonitor.ApplicationCore
{
    public class Di
    {
        public static ServiceProvider ServiceProvider { get; internal set; }
    }
}