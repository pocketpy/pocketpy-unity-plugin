using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{
    public struct PySlice
    {
        public object start;
        public object stop;
        public object step;

        public PySlice(object start, object stop, object step)
        {
            this.start = start;
            this.stop = stop;
            this.step = step;
        }
    }

    public class PySliceType : PyTypeObject
    {
        public override string Name => "slice";
        public override System.Type CSType => typeof(PySlice);
    }
}
