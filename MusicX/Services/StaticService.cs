using DryIoc;
using System.Net;

namespace MusicX.Services
{
    public static class StaticService
    {
        public static IContainer Container { get; set; }
        public static string Version = "0.27.1";
        public static string VersionKind = "beta";
        public static string BuildDate = "5 мая 2022";

        public static WebClient WebClient { get; set; }
    }
}
