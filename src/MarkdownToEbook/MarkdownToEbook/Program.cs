using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownToEbook
{
    class Program
    {
        static void Main(string[] args)
        {
            // show actions of no parameter is given
            if (args.Length == 0)
            {
                Console.WriteLine("you need at least an operation. available actions:");
                Trace.TraceInformation("no command provided. do you now know what you wanna do?");
                Assembly.GetEntryAssembly().GetTypes()
                    .Where(t => t.GetInterface(typeof(ICommand).Name) != null && t.IsClass)
                    .ToList()
                    .ForEach(t => {
                        Console.WriteLine("    " + t.Name);
                        Trace.TraceInformation("    {0}", t.Name);
                    });
                return;
            }

            // find class for action and create instance
            var cmdType = Assembly.GetEntryAssembly().GetTypes().FirstOrDefault(f => f.Name.EndsWith(args[0])) ??
                          Assembly.GetEntryAssembly().GetTypes().FirstOrDefault(f => f.Name.EndsWith(args[0] + "Command"));
            if (cmdType == null)
            {
                Console.WriteLine("cannot find type for given action. check for typos and upper/lower case");
                Trace.TraceWarning("I don't understand '{0}'. what do you want me to do?", args[0]);
                return;
            }

            Trace.TraceInformation("Your wish is my command: {0}", cmdType.FullName);
            var cmd = (ICommand)Activator.CreateInstance(cmdType);
            cmd.Execute(args); // pass cmdline parameter to execute methode

        }
    }

    public interface ICommand
    {
        // args are the command line parameter (all of them)
        void Execute(string[] args);
    }
}
