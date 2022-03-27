using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Services
{
    public static class StaticService
    {
        public static IContainer Container { get; set; }
        public static string Version = "1.0";
        public static string VersionKind = "beta";
        public static string BuildDate = "27 марта 2022";
    }
}
