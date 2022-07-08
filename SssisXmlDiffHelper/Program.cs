using System.Text.RegularExpressions;
using System.Xml.Linq;
using SsisXmlDiffHelper.Models;
using System.Text.Json;

namespace SsisXmlDiffHelper
{
    internal class Program
    {
        private static Regex sWhitespace = new Regex(@"\s+");
        private static readonly string _version = "SsisXmlDiffHelper v1.0";
        private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        private static string _fileName = "";
        private static string? _errorMessage = null;

        static void Main(string[] args)
        {
            XNamespace dtsNs = "www.microsoft.com/SqlServer/Dts";
            XNamespace sqlTaskNs = "www.microsoft.com/sqlserver/dts/tasks/sqltask";
            var currentDirectory = Directory.GetCurrentDirectory();
            List<string> processedFiles = new();

            var dtsxFilePaths = Directory.GetFiles(currentDirectory, "*.dtsx");

            if (dtsxFilePaths.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No dtsx files can be found at {currentDirectory}");
                Console.ReadKey();

                Environment.Exit(0);
            }

            try
            {
                foreach (var file in dtsxFilePaths)
                {
                    _fileName = new FileInfo(file).Name;

                    Console.WriteLine($"Processing: {_fileName}");
                    var document = XElement.Load(file);

                    List<ExecutableDtsTask> executableTasks = document
                        .Descendants(dtsNs + "Executable")
                        .Where(d => d.Attribute(dtsNs + "ExecutableType") != null
                            && (
                            d.Attribute(dtsNs + "ExecutableType")?.Value == "Microsoft.Pipeline"
                            ||
                            d.Attribute(dtsNs + "ExecutableType")?.Value == "Microsoft.ExecuteSQLTask")
                            )
                        .Select(e => new ExecutableDtsTask
                        {
                            Name = e.Attribute(dtsNs + "ObjectName")?.Value,
                            RefId = e.Attribute(dtsNs + "refId")?.Value,
                            Desc = e.Attribute(dtsNs + "Description")?.Value,
                            Disabled = e.Attribute(dtsNs + "Disabled")?.Value ?? "False",
                            DataFlowComponents = e.Descendants("component")?
                                .Select(c => new DataFlowComponent
                                {
                                    Name = c.Attribute("name")?.Value,
                                    ComponentId = c.Attribute("componentClassID")?.Value,
                                    SqlScriptProperty = c.Descendants("property")?
                                        .Where(z => z.Attribute("name")?.Value == "OpenRowset"
                                            || z.Attribute("name")?.Value == "SqlCommand"
                                            || z.Attribute("name")?.Value == "TableOrViewName")
                                        .Select(z => new SqlScriptProperty
                                        {
                                            PropertyType = z.Attribute("name")?.Value,
                                            PropertyValue = ReplaceWhiteSpaceAndOtherChars(z.Value)
                                        })
                                        .FirstOrDefault(z => !string.IsNullOrEmpty(z.PropertyValue)),
                                    InputColumns = c.Descendants("inputColumn").Select(i => i.Attribute("cachedName")?.Value),
                                    OutputColumns = c.Descendants("output")
                                        .Where(o => o.Attribute("name") != null && !o.Attribute("name").Value.ToLower().Contains("error"))
                                        .Descendants("outputColumn")
                                        .Select(oc => oc.Attribute("name")?.Value)
                                })
                                .Where(result => result.SqlScriptProperty != null),
                            SqlTaskScript = e.Descendants(sqlTaskNs + "SqlTaskData")?
                                            .Select(s => ReplaceWhiteSpaceAndOtherChars(s.Attribute(sqlTaskNs + "SqlStatementSource")?.Value))
                                            .FirstOrDefault()

                        })
                        .ToList();

                    string diffHelperJsonOutput = JsonSerializer.Serialize(executableTasks, _jsonOptions);

                    File.WriteAllText($"{currentDirectory}\\{_fileName}.json", diffHelperJsonOutput);

                    processedFiles.Add(_fileName);
                }
            }
            catch (Exception ex)
            {
                _errorMessage = $"Error while processing {_fileName}: {ex.Message}, {ex.StackTrace}";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(_errorMessage);
                Console.ResetColor();
                Console.ReadKey();
            }
            finally
            {
                // Write log
                var versionInfo = new
                {
                    VersionInfo = _version,
                    GeneratedDateUtc = DateTime.UtcNow,
                    Errors = _errorMessage ?? "",
                    ProcessedFiles = processedFiles
                };

                string versionInfoJson = JsonSerializer.Serialize(versionInfo, _jsonOptions);
                File.WriteAllText($"{currentDirectory}\\SsisXmlDiffHelperLog.json", versionInfoJson);
            }
        }

        private static string? ReplaceWhiteSpaceAndOtherChars(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            input = input.Replace("[", "");
            input = input.Replace("]", "");
            input = input.Replace("\"", "");

            return sWhitespace.Replace(input, " ");
        }
    }
}