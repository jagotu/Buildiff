using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Buildiff
{
    public enum CompareResult
    {
        Identical,
        Modified,
        Added,
        Removed
    }

    public abstract class ExportContext
    {
        protected ListStack<string> tree = new ListStack<string>();
        private Stack<bool> reportedSelf = new Stack<bool>();
        private ListStack<bool> anyChanges = new ListStack<bool>();
        public string OutputBase { get; set; } = "diff\\";

        public virtual void ReportChild(string ChildName, CompareResult result)
        {
            Enter(ChildName);
            ReportSelf(result);
            Leave();
        }

        public virtual void ReportSelf(CompareResult result)
        {
            reportedSelf.Pop();
            reportedSelf.Push(true);
            if (result != CompareResult.Identical)
            {
                for (int i = 0; i < anyChanges.Count; i++)
                {
                    anyChanges[i] = true;
                }

            }
        }


        public virtual void Enter(string name)
        {
            tree.Push(name);
            reportedSelf.Push(false);
            anyChanges.Push(false);
        }

        public virtual void Leave()
        {
            if (!reportedSelf.Peek())
            {
                ReportSelf(anyChanges.Pop() ? CompareResult.Modified : CompareResult.Identical);
            } else
            {
                anyChanges.Pop();
            }
            reportedSelf.Pop();
            tree.Pop();
        }

        public string GetCurrentPath()
        {
            return OutputBase + string.Join(Path.DirectorySeparatorChar.ToString(), tree.Select(x => SanitizePath(x)));
        }

        private string SanitizePath(string name)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                name = name.Replace(c.ToString(), "_");
            }
            return name;
        }

        public virtual void ReportExtra(string key, string value) { }
    }

    public class ConsoleExportContext : ExportContext
    {
        Stack<StringBuilder> buffers = new Stack<StringBuilder>();
      

        public override void Enter(string name)
        {
            base.Enter(name);
            buffers.Push(new StringBuilder());
        }

        public override void Leave()
        {
            base.Leave();
            if(buffers.Count == 1)
            {
                Console.Write(buffers.Pop().ToString());
            } else
            {
                StringBuilder tmp = buffers.Pop();
                buffers.Peek().Append(tmp);
            }
        }

        public override void ReportSelf(CompareResult result)
        {
            base.ReportSelf(result);
            
            if(result == CompareResult.Identical)
            {
                return;
            }
            string reportString = $"{new String(' ', tree.Count - 1)}{tree.Peek()}: {result.ToString()}\n";
            
            buffers.Peek().Insert(0, reportString);
            
        }

    }

    public class XmlExportContext : ExportContext
    {
        private XmlDocument doc;

        private XmlElement current;

        public XmlExportContext()
        {
            doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);
            current = doc.CreateElement(string.Empty, "BuilDiff", string.Empty);
            doc.AppendChild(current);
        }

        public override void Enter(string name)
        {
            base.Enter(name);
            XmlElement tmp = doc.CreateElement(string.Empty, "Node", string.Empty);
            tmp.SetAttribute("name", name);
            current.AppendChild(tmp);
            current = tmp;
        }

        public override void Leave()
        {
            base.Leave();
            current = (XmlElement)current.ParentNode;
        }

        public override void ReportSelf(CompareResult result)
        {
            current.SetAttribute("compareResult", result.ToString());
        }

        public override void ReportExtra(string key, string value)
        {
            current.SetAttribute(key, value);
        }

        public XmlDocument GetResult()
        {
            if(tree.Count != 0)
            {
                throw new InvalidOperationException();
            }
            return doc;
        }
    }

    public static class Extras
    {
        public const string Format = "format";
        public const string DetailedDiff = "detailedDiff";
    }
}
