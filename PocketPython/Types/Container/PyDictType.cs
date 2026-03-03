using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{
    public struct PyDictKey
    {
        public VM vm;
        public object obj;

        public PyDictKey(VM vm, object obj)
        {
            this.vm = vm;
            this.obj = obj;
        }

        public override bool Equals(object obj)
        {
            return vm.PyEquals(this.obj, ((PyDictKey)obj).obj);
        }

        public override int GetHashCode()
        {
            return vm.PyHash(obj);
        }
    }

    public class PyDict : Dictionary<PyDictKey, object> { }

    public class PyDictType : PyTypeObject
    {
        public override string Name => "dict";
        public override System.Type CSType => typeof(PyDict);

        [PythonBinding]
        public object __new__(PyTypeObject type, params object[] args)
        {
            if (args.Length == 0)
            {
                return new PyDict();
            }
            if (args.Length == 1)
            {
                var list = vm.PyList(args[0]);
                var d = new PyDict();
                foreach (var item in list)
                {
                    var pair = vm.PyCast<object[]>(item);
                    if (pair.Length != 2) vm.TypeError("expected a list of 2-tuples");
                    d.Add(new PyDictKey(vm, pair[0]), pair[1]);
                }
                return d;
            }
            vm.TypeError("dict expected at most 1 argument, got " + args.Length);
            return null;
        }

        [PythonBinding]
        public object __getitem__(PyDict dict, object key)
        {
            if (dict.TryGetValue(new PyDictKey(vm, key), out var value))
            {
                return value;
            }
            vm.KeyError(key);
            return null;
        }

        [PythonBinding]
        public object __setitem__(PyDict dict, object key, object value)
        {
            dict[new PyDictKey(vm, key)] = value;
            return VM.None;
        }

        [PythonBinding]
        public object __delitem__(PyDict dict, object key)
        {
            dict.Remove(new PyDictKey(vm, key));
            return VM.None;
        }

        [PythonBinding]
        public bool __contains__(PyDict dict, object key)
        {
            return dict.ContainsKey(new PyDictKey(vm, key));
        }

        [PythonBinding]
        public int __len__(PyDict dict)
        {
            return dict.Count;
        }

        [PythonBinding]
        public object[] keys(PyDict dict)
        {
            var list = new List<object>();
            foreach (var pair in dict) list.Add(pair.Key.obj);
            return list.ToArray();
        }

        [PythonBinding]
        public object values(PyDict dict)
        {
            var list = new List<object>();
            foreach (var pair in dict) list.Add(pair.Value);
            return list.ToArray();
        }

        [PythonBinding]
        public object items(PyDict dict)
        {
            var list = new List<object>();
            foreach (var pair in dict) list.Add(new object[] { pair.Key.obj, pair.Value });
            return list.ToArray();
        }

        [PythonBinding]
        public object clear(PyDict dict)
        {
            dict.Clear();
            return VM.None;
        }

        [PythonBinding]
        public object copy(PyDict dict)
        {
            var d = new PyDict();
            foreach (var pair in dict) d.Add(pair.Key, pair.Value);
            return d;
        }


        [PythonBinding]
        public object update(PyDict dict, object other)
        {
            var d = vm.PyCast<PyDict>(other);
            foreach (var pair in d) dict[pair.Key] = pair.Value;
            return VM.None;
        }

        [PythonBinding]
        public object __eq__(PyDict dict, object other)
        {
            var d = other as PyDict;
            if (d == null) return VM.NotImplemented;
            if (dict.Count != d.Count) return false;
            foreach (var pair in dict)
            {
                if (!d.TryGetValue(pair.Key, out var value)) return false;
                if (!vm.PyEquals(pair.Value, value)) return false;
            }
            return true;
        }

        [PythonBinding]
        public string __repr__(PyDict dict)
        {
            var s = "{";
            foreach (var pair in dict)
            {
                if (s.Length > 1) s += ", ";
                s += vm.PyRepr(pair.Key.obj) + ": " + vm.PyRepr(pair.Value);
            }
            s += "}";
            return s;
        }

        [PythonBinding]
        public object pop(PyDict dict, object key)
        {
            PyDictKey k = new PyDictKey(vm, key);
            if (!dict.ContainsKey(k)) vm.KeyError(key);
            object val = dict[k];
            dict.Remove(k);
            return val;
        }

        [PythonBinding]
        public object __iter__(PyDict dict)
        {
            object[] a = keys(dict);
            return new PyIterator(a.GetEnumerator());
        }

        [PythonBinding]
        public int __hash__(PyDict dict)
        {
            vm.TypeError("unhashable type: 'dict'");
            return 0;
        }
    }
}