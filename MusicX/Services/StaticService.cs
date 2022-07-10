using DryIoc;
using System.Net;

namespace MusicX.Services
{
    public static class StaticService
    {
        public static IContainer Container { get; set; }
        public static string Version = "0.29";
        public static string VersionKind = "beta";
        public static string BuildDate = "10 июля 2022";

        public static WebClient WebClient { get; set; }
    }
}
