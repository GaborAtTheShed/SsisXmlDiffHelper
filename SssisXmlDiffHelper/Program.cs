using System.Text.RegularExpressions;
using System.Xml.Linq;
using SsisXmlDiffHelper.Models;
using System.Text.Json;

namespace SsisXmlDiffHelper
{
    internal class Program
    {
        private static Regex sWhitespace = new Regex(@"\s+");

        static void Main(string[] args)
        {
            // TODO: add versioning to files and app

            XNamespace dtsNs = "www.microsoft.com/SqlServer/Dts";
            XNamespace sqlTaskNs = "www.microsoft.com/sqlserver/dts/tasks/sqltask";
            var currentDirectory = Directory.GetCurrentDirectory();

            var dtsxFilePaths = Directory.GetFiles(currentDirectory, "*.dtsx");

            if (dtsxFilePaths.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No dtsx files can be found at {currentDirectory}");

                Environment.Exit(0);
            }

            foreach (var file in dtsxFilePaths)
            {
                var fileName = new FileInfo(file).Name;

                try
                {
                    Console.WriteLine($"Processing: {fileName}");
                    var document = XElement.Load(file);

                    List<ExecutableDtsTask> executableTasks = document
                        .Descendants(dtsNs + "Executable")
                        .Where(d => d.Attribute(dtsNs + "ExecutableType") != null
                            && (
                            d.Attribute(dtsNs + "ExecutableType").Value == "Microsoft.Pipeline"
                            ||
                            d.Attribute(dtsNs + "ExecutableType").Value == "Microsoft.ExecuteSQLTask")
                            )
                        .Select(e => new ExecutableDtsTask
                        {
                            Name = e.Attribute(dtsNs + "ObjectName").Value,
                            RefId = e.Attribute(dtsNs + "refId").Value,
                            Desc = e.Attribute(dtsNs + "Description").Value,
                            Disabled = e.Attribute(dtsNs + "Disabled") == null ? "False" : e.Attribute(dtsNs + "Disabled").Value,
                            DataFlowComponents = e.Descendants("component")?
                                .Select(c => new DataFlowComponent
                                {
                                    Name = c.Attribute("name").Value,
                                    ComponentId = c.Attribute("componentClassID").Value,
                                    SqlScriptProperty = c.Descendants("property")?
                                        .Where(z => z.Attribute("name").Value == "OpenRowset"
                                            || z.Attribute("name").Value == "SqlCommand"
                                            || z.Attribute("name").Value == "TableOrViewName")
                                        .Select(z => new SqlScriptProperty
                                        {
                                            PropertyType = z.Attribute("name").Value,
                                            PropertyValue = ReplaceWhiteSpaceAndOtherChars(z.Value)
                                        })
                                        .FirstOrDefault(z => !string.IsNullOrEmpty(z.PropertyValue))
                                }),
                            SqlTaskScript = e.Descendants(sqlTaskNs + "SqlTaskData")?
                                            .Select(s => ReplaceWhiteSpaceAndOtherChars(s.Attribute(sqlTaskNs + "SqlStatementSource").Value))
                                            .FirstOrDefault()

                        })
                        .ToList();

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(executableTasks, options);
                    File.WriteAllText($"{currentDirectory}\\{fileName}.json", json);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error while processing {fileName}: {ex.Message}, {ex.StackTrace}");
                    Console.WriteLine("To continue processing the rest of the files, press any key.");
                    Console.ResetColor();

                    Console.ReadKey();
                }
            }
        }

        private static string ReplaceWhiteSpaceAndOtherChars(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            input = input.Replace("[", "");
            input = input.Replace("]", "");
            input = input.Replace("\"", "");

            return sWhitespace.Replace(input, " ");
        }
    }
}