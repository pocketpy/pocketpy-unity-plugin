using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PocketPython
{
    public class ValueStack : List<object>
    {
        public object Pop()
        {
            object o = this[this.Count - 1];
            this.RemoveAt(this.Count - 1);
            return o;
        }

        public void Push(object o)
        {
            this.Add(o);
        }

        public object Top()
        {
            return this[this.Count - 1];
        }

        public object Second()
        {
            return this[this.Count - 2];
        }

        public object Third()
        {
            return this[this.Count - 3];
        }

        public object Peek(int i)
        {
            return this[this.Count - i];
        }

        public void SetTop(object o)
        {
            this[this.Count - 1] = o;
        }

        public void SetSecond(object o)
        {
            this[this.Count - 2] = o;
        }

        public void SetThird(object o)
        {
            this[this.Count - 3] = o;
        }

        public void Shrink(int n)
        {
            this.RemoveRange(this.Count - n, n);
        }

        public object[] PopNReversed(int n)
        {
            object[] tuple = new object[n];
            for (int i = 0; i < n; i++) tuple[i] = this[this.Count - n + i];
            this.Shrink(n);
            return tuple;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i] == null) sb.Append("null");
                else sb.Append(this[i].ToString());
                if (i != this.Count - 1) sb.Append(", ");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}