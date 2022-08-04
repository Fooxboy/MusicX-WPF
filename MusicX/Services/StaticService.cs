using DryIoc;
using System.Net;

namespace MusicX.Services
{
    public static class StaticService
    {
        public static IContainer Container { get; set; }
        public static string Version = "0.30";
        public static string VersionKind = "beta";
        public static string BuildDate = "4 августа 2022";

        public static WebClient WebClient { get; set; }
    }
}
