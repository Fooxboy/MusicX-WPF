using System;
using System.Net;

namespace MusicX.Services
{
    public static class StaticService
    {
        public static IServiceProvider Container { get; set; }
        public static string Version = "0.33";
        public static string VersionKind = "beta";
        public static string BuildDate = "6 октября 2022";

        public static WebClient WebClient { get; set; }
    }
}
