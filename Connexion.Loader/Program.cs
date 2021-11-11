using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Connexion.Loader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ResetColor();
            PrintConnexion();

            var firstAttempt = true;
            var pluginPath = "";
            do
            {
                if (firstAttempt)
                    Console.WriteLine("Enter your plugin's path:");
                else
                    Console.WriteLine("The folder does not exist, enter a valid plugin's path:");
                pluginPath = Console.ReadLine();
                firstAttempt = false;
            } while (!Directory.Exists(pluginPath));

            Console.Clear();

            PrintConnexion();

            var destFirstAttempt = true;
            var destinationPath = "";
            do
            {
                if (destFirstAttempt)
                    Console.WriteLine("Enter your destination path:");
                else
                    Console.WriteLine("The path is invalid, enter a valid destination path:");
                destinationPath = Console.ReadLine();
                destFirstAttempt = false;
                var hasValidParentDir = Directory.Exists(Path.GetDirectoryName(destinationPath));
                if (hasValidParentDir && !Directory.Exists(destinationPath))
                    try
                    {
                        var dir = Directory.CreateDirectory(destinationPath);
                    }
                    catch { }
            } while (!Directory.Exists(destinationPath));
            Console.Clear();

            var gravitySettings = Path.Combine(destinationPath, "metadata.json");
            if (!File.Exists(gravitySettings))
            {
                File.WriteAllText(gravitySettings, "{}");
            }

            JObject jsonData = null;
            try
            {
                jsonData = JObject.Parse(File.ReadAllText(gravitySettings));
            }
            catch (Exception)
            {
                throw new Exception("There is a problem with parsing your json file.");
            }

            var priorityFile = Path.Combine(pluginPath, "priority.txt");
            if (!File.Exists(priorityFile)) throw new Exception("'priority.txt' file not found.");

            var priorityList = File.ReadAllLines(priorityFile).Where(x => !string.IsNullOrEmpty(x)).Select(x => x.ToLower().Trim()).ToList();

            var pldups = priorityList.GetDuplicates().Distinct();
            if (pldups.Any())
                throw new Exception($"'priority.txt' file has duplicate items.{Environment.NewLine} Item(s): {pldups.Aggregate((a, b) => $"{a}, {b}")}");

            var baseInfo = new ConnectorInfo { Destination = destinationPath, CommandLineArgs = args };
            var jObj = jsonData;

            var loaders = new List<McMaster.NETCore.Plugins.PluginLoader>();

            var pluginsDir = Path.Combine(pluginPath);
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var dirName = Path.GetFileName(dir);
                var pluginDll = Path.Combine(dir, dirName + ".dll");
                if (File.Exists(pluginDll))
                {
                    var loader = McMaster.NETCore.Plugins.PluginLoader.CreateFromAssemblyFile(
                        pluginDll,
                        sharedTypes: new[] { typeof(BaseConnector) });
                    loaders.Add(loader);
                }
            }
            var plugins = loaders.SelectMany(l => l
                     .LoadDefaultAssembly()
                     .GetTypes()
                     .Where(t => typeof(BaseConnector).IsAssignableFrom(t) && !t.IsAbstract)).Select(x => (BaseConnector)Activator.CreateInstance(x));

            foreach (var lst in priorityList)
            {
                var plugin = plugins.Where(x => x.Id.ToLower().Trim() == lst).FirstOrDefault();
                if (plugin != null)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Starting '{plugin.Id}' process.");
                    Console.ResetColor();
                    Console.WriteLine();

                    jObj = plugin.Execute(jObj, baseInfo);

                    if (jObj == null)
                        break;

                    plugin.SaveSettings(jObj, baseInfo);
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Completing '{plugin.Id}' process.");
                    Console.WriteLine("Press a key to continue...");
                    Console.ResetColor();
                    Console.ReadKey();
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Processing completed. Press a key to exit.");
            Console.ResetColor();
            Console.ReadKey();
        }

        private static void PrintConnexion()
        {
            Console.WriteLine(@"
  _____                            _             
 / ____|                          (_)            
| |     ___  _ __  _ __   _____  ___  ___  _ __  
| |    / _ \| '_ \| '_ \ / _ \ \/ / |/ _ \| '_ \ 
| |___| (_) | | | | | | |  __/>  <| | (_) | | | |
 \_____\___/|_| |_|_| |_|\___/_/\_\_|\___/|_| |_|

");
        }
    }
}
