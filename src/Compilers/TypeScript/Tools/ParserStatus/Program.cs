using System;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.TypeScript;

namespace Microsoft.CodeAnalysis.TypeScript.ParserStatus
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ParserStatus <directory_path>");
                return;
            }

            string directoryPath = args[0];
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory not found: {directoryPath}");
                return;
            }

            var files = Directory.GetFiles(directoryPath, "*.ts", SearchOption.AllDirectories);
            Console.WriteLine($"Found {files.Length} TypeScript files in {directoryPath}");

            int totalFiles = files.Length;
            int successfulParses = 0;
            int failedParses = 0;

            Parallel.ForEach(files, file =>
            {
                try
                {
                    var sourceText = File.ReadAllText(file);
                    var tree = TypeScriptSyntaxTree.ParseText(sourceText);
                    var diagnostics = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

                    if (diagnostics.Count == 0)
                    {
                        Interlocked.Increment(ref successfulParses);
                    }
                    else
                    {
                        Interlocked.Increment(ref failedParses);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                    Interlocked.Increment(ref failedParses);
                }
            });

            double successRate = totalFiles > 0 ? (double)successfulParses / totalFiles * 100 : 0;

            Console.WriteLine($"\n--- TypeScript Parser Status ---");
            Console.WriteLine($"Total Files: {totalFiles}");
            Console.WriteLine($"Successful Parses: {successfulParses}");
            Console.WriteLine($"Failed Parses: {failedParses}");
            Console.WriteLine($"Success Rate: {successRate:F2}%");

            // GitHub Actions Summary
            var githubStepSummary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
            if (!string.IsNullOrEmpty(githubStepSummary))
            {
                File.AppendAllText(githubStepSummary, $@"
### TypeScript Parser Status Report
- **Directory**: `{directoryPath}`
- **Total Files**: {totalFiles}
- **Successful Parses**: {successfulParses}
- **Failed Parses**: {failedParses}
- **Success Rate**: {successRate:F2}%
");
            }
        }
    }
}
