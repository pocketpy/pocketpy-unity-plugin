using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{
    public class PyTupleType : PyTypeObject
    {
        public override string Name => "tuple";
        public override Type CSType => typeof(object[]);

        [PythonBinding]
        public object __new__(PyTypeObject type, object value)
        {
            List<object> list = vm.PyList(value);
            return list.ToArray();
        }

        [PythonBinding]
        public object __getitem__(object[] a, object b)
        {
            if (b is PySlice)
            {
                int start, stop, step;
                vm.ParseIntSlice((PySlice)b, a.Length, out start, out stop, out step);
                List<object> result = new List<object>();
                if (step > 0)
                {
                    for (int i = start; i < stop; i += step) result.Add(a[i]);
                }
                else
                {
                    for (int i = start; i > stop; i += step) result.Add(a[i]);
                }
                return result.ToArray();
            }

            int index = vm.NormalizedIndex(vm.PyCast<int>(b), a.Length);
            return a[index];
        }

        [PythonBinding]
        public object __len__(object[] value)
        {
            return value.Length;
        }

        [PythonBinding]
        public object __contains__(object[] value, object item)
        {
            foreach (var v in value)
            {
                if (vm.PyEquals(v, item)) return true;
            }
            return false;
        }

        [PythonBinding]
        public object __eq__(object[] a, object b_)
        {
            object[] b = b_ as object[];
            if (b == null) return VM.NotImplemented;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (!vm.PyEquals(a[i], b[i])) return false;
            }
            return true;
        }

        [PythonBinding]
        public int __hash__(object[] value)
        {
            long x = 1000003;
            foreach (var item in value)
            {
                int y = vm.PyHash(item);
                x = x ^ (y + 0x9e3779b9 + (x << 6) + (x >> 2));
            }
            return (int)x;
        }
    }
}
