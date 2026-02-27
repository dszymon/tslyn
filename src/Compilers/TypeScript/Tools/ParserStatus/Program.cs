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

            // Configure parallelism to avoid overwhelming the system
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

            Parallel.ForEach(files, parallelOptions, file =>
            {
                // Use a cancellation token source for timeout
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    try
                    {
                        var sourceText = File.ReadAllText(file);
                        var task = Task.Run(() =>
                        {
                            var tree = TypeScriptSyntaxTree.ParseText(sourceText, cancellationToken: cts.Token);
                            var diagnostics = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
                            return diagnostics.Count == 0;
                        }, cts.Token);

                        if (task.Wait(TimeSpan.FromSeconds(6))) // Wait slightly longer than CTS to allow graceful cancellation
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
                    }
                    catch (AggregateException ae)
                    {
                        foreach (var e in ae.InnerExceptions)
                        {
                            if (e is TaskCanceledException)
                            {
                                Interlocked.Increment(ref failedParses);
                                Interlocked.Increment(ref timedOutParses);
                                Console.WriteLine($"Timeout (cancelled) processing file: {file}");
                            }
                            else
                            {
                                Interlocked.Increment(ref failedParses);
                                Console.WriteLine($"Error processing file {file}: {e.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failedParses);
                        Console.WriteLine($"Error processing file {file}: {ex.Message}");
                    }
                }
            });

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
