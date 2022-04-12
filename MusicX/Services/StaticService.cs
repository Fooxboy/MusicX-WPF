using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Services
{
    public static class StaticService
    {
        public static IContainer Container { get; set; }
        public static string Version = "0.24";
        public static string VersionKind = "beta";
        public static string BuildDate = "13 апреля 2022";

        public static WebClient WebClient { get; set; }
    }
}
