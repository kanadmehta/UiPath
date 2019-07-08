using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace ProjectAnalyser
{
    internal class Program
    {
        static string projectLocation= @"C:\Users\u6ic_kme\Desktop\Work\RPA\EDI_China\Final draft\";

        private static void Main(string[] args)
        {
            /*
             * Done. Get project info
             * Get workflows with argument and variable list (check workflow file order)
             * Call map/ chart
             * Parse code for review guideline
             * 
             * Handle input args
             * Write to word
             */

            LoadJson();
            GetWorkFlowFiles();
        }

        private static void LoadJson()
        {
            string jsonFilePath = Path.Combine(projectLocation,"project.json");

            string json = File.ReadAllText(jsonFilePath);
            Dictionary<string, object> json_Dictionary = (new JavaScriptSerializer()).Deserialize<Dictionary<string, object>>(json);

            Console.WriteLine("------ Key Value--------");
            foreach (var item in json_Dictionary)
            {
                Console.WriteLine("Key: " + item.Key);

                if (IsDictionary(item.Value) && item.Key.ToLower() == "dependencies")
                {
                    foreach (var keyVal in (Dictionary<string, object>)item.Value)
                    {
                        Console.WriteLine("Key: " + keyVal.Key + " - " + keyVal.Value);
                    }
                }
                else
                    Console.WriteLine("Value: " + item.Value);
            }
        }

        private static void GetWorkFlowFiles()
        {
            Console.WriteLine("------ Workflow files--------");
            string[] files = Directory.GetFiles(projectLocation, "*.xaml", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (new FileInfo(file).Name.StartsWith("~")) continue;
                Console.WriteLine(file.Replace(projectLocation, ""));

            }
        }


        public static bool IsList(object o)
        {
            if (o == null) return false;
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsDictionary(object o)
        {
            if (o == null) return false;
            return o is IDictionary &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }
    }

}