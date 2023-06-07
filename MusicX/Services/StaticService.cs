using System;
using System.Net;
using System.Reflection;

namespace MusicX.Services
{
    public static class StaticService
    {
        public static IServiceProvider Container { get; set; }
        public static string Version = typeof(StaticService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0";
        public static string VersionKind = "";
        public static string BuildDate = "8 июня 2023";

        public static WebClient WebClient { get; set; }
    }
}
