using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.TypeScript;

namespace Microsoft.CodeAnalysis.TypeScript.ParserStatus
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ParserStatus <directory_path>");
                return;
            }

            // Worker mode
            if (args.Length == 2 && args[0] == "--parse-single")
            {
                string file = args[1];
                try
                {
                    var sourceText = File.ReadAllText(file);
                    var tree = TypeScriptSyntaxTree.ParseText(sourceText);
                    var diagnostics = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

                    if (diagnostics.Count == 0)
                    {
                        Environment.Exit(0); // Success
                    }
                    else
                    {
                        Environment.Exit(1); // Parsed with errors
                    }
                }
                catch
                {
                    Environment.Exit(2); // Exception
                }
                return;
            }

            // Coordinator mode
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

            // Timeout per file
            var timeout = TimeSpan.FromSeconds(3);

            // Path to the current executable
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "dotnet";
            string argumentsPrefix = string.Empty;

            // Determine if running via `dotnet run` or compiled exe
            if (exePath.EndsWith("dotnet") || exePath.EndsWith("dotnet.exe"))
            {
                // Assuming we are run via `dotnet run --project ...`
                // We'll just invoke our own assembly.
                exePath = typeof(Program).Assembly.Location;
                // If it's a dll, we use `dotnet path/to.dll`
                if (exePath.EndsWith(".dll"))
                {
                    argumentsPrefix = $"\"{exePath}\" ";
                    exePath = "dotnet";
                }
            }

            // Limit concurrency
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount);

            var tasks = files.Select(async file =>
            {
                await semaphore.WaitAsync();
                try
                {
                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = exePath;
                        process.StartInfo.Arguments = $"{argumentsPrefix}--parse-single \"{file}\"";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;

                        process.Start();

                        // Wait for exit with timeout
                        bool exited = process.WaitForExit((int)timeout.TotalMilliseconds);

                        if (exited)
                        {
                            if (process.ExitCode == 0)
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
                            // Kill hanging parser
                            try { process.Kill(entireProcessTree: true); } catch { }

                            Interlocked.Increment(ref failedParses);
                            Interlocked.Increment(ref timedOutParses);
                            Console.WriteLine($"Timeout processing file: {file}");
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

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

            if (failedParses > 0)
            {
                Environment.Exit(1);
            }
        }
    }
}
