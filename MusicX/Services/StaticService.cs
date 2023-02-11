using System;
using System.Net;

namespace MusicX.Services
{
    public static class StaticService
    {
        public static IServiceProvider Container { get; set; }
        public static string Version = "0.38";
        public static string VersionKind = "beta";
        public static string BuildDate = "11 февраля 2023";

        public static WebClient WebClient { get; set; }
    }
}
