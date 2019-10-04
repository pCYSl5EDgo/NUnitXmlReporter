using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using Utf8Json;

namespace NUnitReporter
{
    class Program
    {
        static int Main(string[] args)
        {
            var doc = new XmlDocument();
            if (args.Length == 0)
            {
                args = new[] { "" };
            }
            if (args.Length == 1)
            {
                var versionString = Assembly.GetEntryAssembly()
                                        ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                        ?.InformationalVersion
                                    ?? throw new NullReferenceException();
                Console.WriteLine($"botsay v{versionString}");
                Console.WriteLine("-------------");
                Console.WriteLine("\nUsage:");
                Console.WriteLine("  nunit-xml-reporter inputFile outputFile --option");
                return 0;
            }
            doc.LoadXml(File.ReadAllText(args[0], Encoding.UTF8));
            var outputPath = args[1];
            switch (args[2])
            {
                case "--json":
                    {
                        using var writer = new StreamWriter(outputPath);
                        writer.Write(@"{""text"":");
                        writer.Write(Encoding.UTF8.GetString(JsonSerializer.Serialize(new JsonEncoder().Encode(doc, out var success))));
                        writer.Write('}');
                        return success ? 0 : 2;
                    }
                case "--slack-text":
                    {
                        using var writer = new StreamWriter(outputPath);
                        writer.Write(@"{""text"":");
                        writer.Write(Encoding.UTF8.GetString(JsonSerializer.Serialize(new ConsoleEncoder().Encode(doc, out var success))));
                        writer.Write('}');
                        return success ? 0 : 2;
                    }
                case "--slack-block":
                    {
                        using var writer = new StreamWriter(outputPath);
                        writer.Write(@"{""text"":");
                        writer.Write(Encoding.UTF8.GetString(JsonSerializer.Serialize(new ConsoleEncoder().Encode(doc, out var success))));
                        writer.Write(@",""blocks"":[");
                        writer.Write(new BlockEncoder(args[3], args[4], args.Length >= 6 ? args[5] : "ACTION").Encode(doc, out _));
                        writer.Write("]}");
                        return success ? 0 : 2;
                    }
                default:
                    return 1;
            }
        }
    }
}
