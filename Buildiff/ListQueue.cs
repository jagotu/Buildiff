using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildiff
{
    public class ListStack<T> : List<T>
    {
        public void Push(T item)
        {
            Add(item);
        }

        public T Peek()
        {
            return this[Count - 1];
        }

        public T Pop()
        {
            T tmp = this[Count - 1];
            RemoveAt(Count - 1);
            return tmp;
        }
    }
}
