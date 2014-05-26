using FSharp.Markdown;
using FSharp.Markdown.Pdf;
using Microsoft.FSharp.Core;
using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace MarkdownToEbook.Commands
{
    public class Markdown2Pdf : ICommand
    {

        public void Execute(string[] args)
        {
            if (args.Length < 2 || args[1].Length == 0)
            {
                Console.WriteLine("dude, a minimum would be a markdown file to convert from");
                Trace.TraceInformation("dude, a minimum would be a markdown file to convert from");
                return;
            }

            if (!File.Exists(args[1]))
            {
                Console.WriteLine("dude, let me explain: a minimum would be a markdown file to convert from WHICH EXISTS");
                Trace.TraceInformation("dude, let me explain: a minimum would be a markdown file to convert from WHICH EXISTS");
                return;
            }

            var outFile = Path.Combine(new FileInfo(args[1]).DirectoryName, "output.pdf");
            if (args.Length == 3) outFile = args[2];
            Trace.TraceInformation("using outfile {0}", outFile);

            var text = File.ReadAllText(args[1]);

            // lets try the native way
            var parsed = Markdown.Parse(text);
            //parsed.DefinedLinks.Add("foo", new Tuple<string, FSharpOption<string>>("bar",new FSharpOption<string>("jane") { }));
            var doc = new Document();

            var style = doc.Styles.AddStyle(MarkdownStyleNames.Heading1, StyleNames.Normal);
            style.Font.Size = 16;
            style.Font.Bold = true;
            style.ParagraphFormat.PageBreakBefore = true;
            style.ParagraphFormat.SpaceAfter = new MigraDoc.DocumentObjectModel.Unit(1.0, UnitType.Centimeter);

            style = doc.Styles.AddStyle(MarkdownStyleNames.Heading2, StyleNames.Normal);
            style.Font.Size = 14;
            style.Font.Bold = true;
            style.ParagraphFormat.SpaceBefore = new MigraDoc.DocumentObjectModel.Unit(0.7, UnitType.Centimeter);

            var pdfDoc = PdfFormatting.formatMarkdown(doc, parsed.DefinedLinks, parsed.Paragraphs);
            pdfDoc.Save(outFile);

            pdfDoc.Close();

            Console.WriteLine("pdf file created at {0}", new FileInfo(outFile).Name);
        }
    }

    public class MergeFiles : ICommand
    {

        public void Execute(string[] args)
        {
            if (args.Length < 2 || args[1].Length == 0)
            {
                Console.WriteLine("at least I need a folder for find and merge all .md files");
                Trace.TraceInformation("at least I need a folder for find and merge all .md files");
                return;
            }

            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine("c'mon ... this folder does not exists");
                Trace.TraceInformation("c'mon ... this folder does not exists");
                return;
            }
            var dir = new DirectoryInfo(args[1]);

            var filePattern = "*.md";
            if (args.Length == 3) filePattern = args[2];
            Trace.TraceInformation("using file pattern {0}", filePattern);

            var outFile = Path.Combine(args[1], "output.md");
            if (args.Length == 4) outFile = args[3];
            Trace.TraceInformation("using outfile {0}", outFile);

            var files = dir.GetFiles(filePattern);
            if (files.Length == 0)
            {
                Console.WriteLine("u gonna be kidding me ... there is no file!");
                Trace.TraceInformation("u gonna be kidding me ... there is no file!");
                return;
            }

            using (StreamWriter writer = File.CreateText(outFile))
            {
                foreach (var file in files.OrderBy(f => f.FullName))
                {
                    Console.WriteLine("... merging file {0}", file.Name);

                    var rdr = file.OpenText();
                    writer.Write(rdr.ReadToEnd());
                    writer.WriteLine();
                    rdr.Close();
                }
            }

            Console.WriteLine("md file created at {0}", new FileInfo(outFile).Name);
        }
    }

    public class DoNothing : ICommand
    {
        public void Execute(string[] args)
        {
            Console.WriteLine("Command 'DoNothing' executed");
            Trace.TraceInformation("Command: I am doing nothing, as you wish ... that was easy.");
        }
    }
}
