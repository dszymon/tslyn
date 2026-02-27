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
            int timedOutParses = 0;

            // Short timeout since real files should parse in milliseconds.
            // A 3-second timeout catches infinite loops gracefully.
            var timeout = TimeSpan.FromSeconds(3);

            // Give the entire parallel loop a maximum global time budget (e.g. 5 minutes)
            var globalTimeout = TimeSpan.FromMinutes(5);

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

            try
            {
                using (var ctsGlobal = new CancellationTokenSource(globalTimeout))
                {
                    parallelOptions.CancellationToken = ctsGlobal.Token;

                    Parallel.ForEach(files, parallelOptions, file =>
                    {
                        var sourceText = File.ReadAllText(file);

                        var task = Task.Run(() =>
                        {
                            try
                            {
                                var tree = TypeScriptSyntaxTree.ParseText(sourceText);
                                var diagnostics = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
                                return diagnostics.Count == 0;
                            }
                            catch
                            {
                                return false;
                            }
                        });

                        if (task.Wait(timeout))
                        {
                            if (task.Result)
                            {
                                Interlocked.Increment(ref successfulParses);
                            }
                            else
                            {
                                Interlocked.Increment(ref failedParses);
                            }
                        }
                        else
                        {
                            Interlocked.Increment(ref failedParses);
                            Interlocked.Increment(ref timedOutParses);
                            Console.WriteLine($"Timeout processing file: {file}");
                        }
                    });
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"\nWARNING: Global timeout of {globalTimeout.TotalMinutes} minutes reached. Terminating early.");
                // Count remaining unvisited files as failed.
                int unvisited = totalFiles - (successfulParses + failedParses);
                if (unvisited > 0)
                {
                    failedParses += unvisited;
                    timedOutParses += unvisited;
                    Console.WriteLine($"{unvisited} files abandoned due to global timeout.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal Error: {ex.Message}");
            }

            double successRate = totalFiles > 0 ? (double)successfulParses / totalFiles * 100 : 0;

            Console.WriteLine($"\n--- TypeScript Parser Status ---");
            Console.WriteLine($"Total Files: {totalFiles}");
            Console.WriteLine($"Successful Parses: {successfulParses}");
            Console.WriteLine($"Failed Parses: {failedParses}");
            Console.WriteLine($"  Timeouts: {timedOutParses}");
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
  - **Timeouts**: {timedOutParses}
- **Success Rate**: {successRate:F2}%
");
            }
        }
    }
}
