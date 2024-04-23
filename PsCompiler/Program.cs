using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PsCompiler;

using SpecialFolder = Environment.SpecialFolder;

public static class Program
{
    public static int Main(string[] argv)
    {
        if (argv.Length == 0)
        {
            Console.WriteLine("""
                              Usage:
                                  pscompiler <src-dir> <dst-dir>

                              Compiles any *.fx file in `src-dir` and writes the generated *.ps file into the directory `dst-dir`.
                              """);

            return 0;
        }

        var archstr = Environment.Is64BitOperatingSystem ? "x64" : "x86";

        var fxc = (from f in new[]
            {
                SpecialFolder.Programs,
                SpecialFolder.ProgramFiles,
                SpecialFolder.ProgramFilesX86,
                SpecialFolder.CommonPrograms,
                SpecialFolder.CommonProgramFiles,
                SpecialFolder.CommonProgramFilesX86,
            }
            let path = Environment.GetFolderPath(f)
            let dir = new DirectoryInfo($@"{path}\Windows Kits")
            where dir.Exists
            from file in dir.GetFiles("fxc.exe", SearchOption.AllDirectories)
            where file?.Directory?.Name != null
            let arch = file.Directory.Name
            let verstr = file.Directory.FullName.Match(@"([0-9]+\.[0-9]+[\.0-9]*)", out var m) ? m.ToString() : "0.0.0.0"
            orderby new Version(verstr) descending, arch.Contains(archstr, StringComparison.OrdinalIgnoreCase) ? 1 : -1 descending
            select file).FirstOrDefault();

        foreach ((var func, var message) in new(Func<bool>, string)[]
                 {
                     (() => fxc is null, "The fxc compiler could not be found on this machine. Please install the latest Windows SDK version."),
                     (() => argv.Length < 1, "A source directory must be given."),
                     (() => argv.Length < 2, "A target directory must be given."),
                     (() => !Directory.Exists(argv[0]), "The source directory must exist.")
                 })
            if (func())
            {
                Console.Error.WriteLine(message);

                return -1;
            }

        var dst = Directory.CreateDirectory(argv[1]);
        var ret = 0;

        foreach (var fx in new DirectoryInfo(argv[0]).GetFiles("*.hlsl"))
        {
            var ps = $@"{dst.FullName}\{fx.Name.Replace(fx.Extension, "")}.ps";
            var psi = new ProcessStartInfo
            {
                FileName = fxc.FullName,
                Arguments = $"/T ps_3_0 /E main /Fo \"{ps}\" \"{fx.FullName}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            bool failed;

            using (var p = new Process())
            {
                p.StartInfo = psi;
                p.Start();

                var cout = p.StandardOutput.ReadToEnd();
                var cerr = p.StandardError.ReadToEnd();

                p.WaitForExit();
                failed = p.ExitCode != 0;

                if (failed)
                {
                    Console.Out.WriteLine(cout);
                    Console.Error.WriteLine(cerr);
                        
                    if (ret == 0)
                        ret = p.ExitCode;
                }
            }

            Console.WriteLine($"[{(failed ? "ERR." : " OK ")}] {fx.FullName} --> {ps}");
        }
            
        return ret;
    }

    private static bool Match(this string str, string pat, out Match m, RegexOptions opt = RegexOptions.Compiled | RegexOptions.IgnoreCase) =>
        (m = Regex.Match(str, pat, opt)).Success;
}