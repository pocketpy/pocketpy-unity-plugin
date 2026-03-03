using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PocketPython
{
    public class PyListType : PyTypeObject
    {
        public override string Name { get { return "list"; } }
        public override System.Type CSType { get { return typeof(List<object>); } }

        [PythonBinding]
        public object __new__(PyTypeObject type, object value)
        {
            return vm.PyList(value);
        }

        [PythonBinding]
        public object __getitem__(List<object> a, object b)
        {
            if (b is PySlice)
            {
                int start, stop, step;
                vm.ParseIntSlice((PySlice)b, a.Count, out start, out stop, out step);
                List<object> result = new List<object>();
                if (step > 0)
                {
                    for (int i = start; i < stop; i += step) result.Add(a[i]);
                }
                else
                {
                    for (int i = start; i > stop; i += step) result.Add(a[i]);
                }
                return result;
            }

            int index = vm.NormalizedIndex(vm.PyCast<int>(b), a.Count);
            return a[index];
        }

        [PythonBinding]
        public object __setitem__(List<object> list, object index, object value)
        {
            list[vm.PyCast<int>(index)] = value;
            return VM.None;
        }

        [PythonBinding]
        public object __delitem__(List<object> list, object index)
        {
            list.RemoveAt(vm.PyCast<int>(index));
            return VM.None;
        }

        [PythonBinding]
        public object __len__(List<object> list)
        {
            return list.Count;
        }

        [PythonBinding]
        public object __contains__(List<object> list, object value)
        {
            foreach (var item in list)
            {
                if (!vm.PyEquals(item, value)) return false;
            }
            return true;
        }

        [PythonBinding]
        public object __add__(List<object> a, object b)
        {
            List<object> list = vm.PyCast<List<object>>(b);
            var res = new List<object>();
            res.AddRange(a);
            res.AddRange(list);
            return res;
        }

        [PythonBinding]
        public object __mul__(List<object> a, object b)
        {
            int count = vm.PyCast<int>(b);
            var res = new List<object>();
            for (int i = 0; i < count; i++)
            {
                res.AddRange(a);
            }
            return res;
        }

        [PythonBinding]
        public object __eq__(List<object> a, object b)
        {
            List<object> list = b as List<object>;
            if (list == null) return VM.NotImplemented;
            if (list.Count != a.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (!vm.PyEquals(a[i], list[i])) return false;
            }
            return true;
        }

        [PythonBinding]
        public object append(List<object> list, object value)
        {
            list.Add(value);
            return VM.None;
        }

        [PythonBinding]
        public object clear(List<object> list)
        {
            list.Clear();
            return VM.None;
        }

        [PythonBinding]
        public object copy(List<object> list)
        {
            var res = new List<object>();
            res.AddRange(list);
            return res;
        }

        [PythonBinding]
        public object count(List<object> list, object value)
        {
            int count = 0;
            foreach (var item in list)
            {
                if (vm.PyEquals(item, value)) count++;
            }
            return count;
        }

        [PythonBinding]
        public object extend(List<object> list, object value)
        {
            list.AddRange(vm.PyList(value));
            return VM.None;
        }

        [PythonBinding]
        public object index(List<object> list, object value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (vm.PyEquals(list[i], value)) return i;
            }
            vm.ValueError("list.index(x): x not in list");
            return null;
        }

        [PythonBinding]
        public object insert(List<object> list, object index, object value)
        {
            list.Insert(vm.PyCast<int>(index), value);
            return VM.None;
        }

        [PythonBinding]
        public object pop(List<object> list, params object[] args)
        {
            if (list.Count == 0)
            {
                vm.IndexError("pop from empty list");
                return null;
            }
            int index = list.Count - 1;
            if (args.Length != 0)
            {
                if (args.Length != 1) vm.TypeError("pop expected at most 1 arguments, got " + args.Length);
                index = vm.PyCast<int>(args[0]);
            }
            index = vm.NormalizedIndex(index, list.Count);
            object res = list[index];
            list.RemoveAt(index);
            return res;
        }

        [PythonBinding]
        public object remove(List<object> list, object value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (vm.PyEquals(list[i], value))
                {
                    list.RemoveAt(i);
                    return VM.None;
                }
            }
            vm.ValueError("list.remove(x): x not in list");
            return null;
        }

        [PythonBinding]
        public object reverse(List<object> list)
        {
            list.Reverse();
            return VM.None;
        }

        [PythonBinding]
        public int __hash__(List<object> list)
        {
            vm.TypeError("unhashable type: 'list'");
            return 0;
        }
    }
}