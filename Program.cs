using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using MicroBatchFramework;
using Microsoft.Extensions.Hosting;
using Utf8Json;

namespace NUnitReporter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await BatchHost.CreateDefaultBuilder().RunBatchEngineAsync<Batch>(args);
        }

        public class Batch : BatchBase
        {
            [Command(new []{ "slack-block" })]
            public void MakeSlackBlock([Option(0)]string file, [Option("-o", "output file path\noutput file is json")]string outputFile, [Option("-r", "UserName & Repository")]string repository, [Option("-c", "Commit SHA")]string commitSha)
            {
                var doc = new XmlDocument();
                doc.Load(file);
                using var writer = new StreamWriter(outputFile);
                writer.Write(@"{""text"":");
                writer.Write(Encoding.UTF8.GetString(JsonSerializer.Serialize(new ConsoleEncoder().Encode(doc, out var success))));
                writer.Write(@",""blocks"":[");
                writer.Write(new BlockEncoder(repository, commitSha).Encode(doc, out _));
                writer.Write("]}");
            }
        }
    }
}
