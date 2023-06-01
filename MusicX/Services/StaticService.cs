using System;
using System.Net;

namespace MusicX.Services
{
    public static class StaticService
    {
        public static IServiceProvider Container { get; set; }
        public static string Version = "0.40";
        public static string VersionKind = "";
        public static string BuildDate = "1 июня 2023";

        public static WebClient WebClient { get; set; }
    }
}
