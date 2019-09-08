using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildiff
{
    static class DiffHelpers<T>
    {
        public static void ThreeWayDiff(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, string> name, ExportContext ec)
        {
            ThreeWayDiff(oldItems, newItems, name, (a,b,c) => { }, ec);
        }

        public static void ThreeWayDiff(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, string> name, Action<T, T, ExportContext> compareItems, ExportContext ec )
        {
            foreach (T oldItem in oldItems)
            {
                T newItem = newItems.Where(x => name(x) == name(oldItem)).FirstOrDefault();
                if (newItem == null)
                {
                    ec.ReportChild(name(oldItem), CompareResult.Removed);
                }
                else
                {
                    ec.Enter(name(oldItem));
                    compareItems(oldItem, newItem, ec);
                    ec.Leave();
                }

            }

            foreach (T newItem in newItems)
            {
                if (!oldItems.Where(x => name(x) == name(newItem)).Any())
                {
                    ec.ReportChild(name(newItem), CompareResult.Added);
                }

            }
        }
    }
}
