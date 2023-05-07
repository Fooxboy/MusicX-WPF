using System;
using System.Net;

namespace MusicX.Services
{
    public static class StaticService
    {
        public static IServiceProvider Container { get; set; }
        public static string Version = "0.39";
        public static string VersionKind = "";
        public static string BuildDate = "7 мая 2023";

        public static WebClient WebClient { get; set; }
    }
}
