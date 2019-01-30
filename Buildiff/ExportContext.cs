using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if(result != CompareResult.Identical)
            {
                for(int i = 0; i < anyChanges.Count; i++)
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
            if(!reportedSelf.Peek())
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
            return string.Join(Path.DirectorySeparatorChar.ToString(), tree.Select(x => sanitizePath(x)));
        }

        private string sanitizePath(string name)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                name = name.Replace(c.ToString(), "_");
            }
            return name;
        }
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
            //DEBUG skip idnetical
            if (result == CompareResult.Identical) return;
            string reportString = $"{new String(' ', tree.Count - 1)}{tree.Peek()}: {result.ToString()}\n";
            
            buffers.Peek().Insert(0, reportString);
            
        }

    }
}
