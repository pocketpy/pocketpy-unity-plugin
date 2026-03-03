using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PocketPython
{
    public class PyRange : IEnumerable<int>
    {
        public int start;
        public int stop;
        public int step;

        public IEnumerator<int> GetEnumerator()
        {
            if (step > 0)
            {
                for (int i = start; i < stop; i += step) yield return i;
            }
            else
            {
                for (int i = start; i > stop; i += step) yield return i;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class PyRangeType : PyTypeObject
    {
        public override string Name => "range";
        public override System.Type CSType => typeof(PyRange);

        [PythonBinding]
        public object __new__(PyTypeObject type, params object[] args)
        {
            var r = new PyRange();
            if (args.Length == 1)
            {
                r.start = 0;
                r.stop = vm.PyCast<int>(args[0]);
                r.step = 1;
            }
            else if (args.Length == 2)
            {
                r.start = vm.PyCast<int>(args[0]);
                r.stop = vm.PyCast<int>(args[1]);
                r.step = 1;
            }
            else if (args.Length == 3)
            {
                r.start = vm.PyCast<int>(args[0]);
                r.stop = vm.PyCast<int>(args[1]);
                r.step = vm.PyCast<int>(args[2]);
            }
            else
            {
                vm.TypeError("range expected 1-3 arguments, got " + args.Length);
            }
            return r;
        }

        [PythonBinding(BindingType.Getter)]
        public object start(PyRange range)
        {
            return range.start;
        }

        [PythonBinding(BindingType.Getter)]
        public object stop(PyRange range)
        {
            return range.stop;
        }

        [PythonBinding(BindingType.Getter)]
        public object step(PyRange range)
        {
            return range.step;
        }
    }

}