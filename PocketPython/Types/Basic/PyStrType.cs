using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace PocketPython
{

    public class PyStrType : PyTypeObject
    {
        public override string Name { get { return "str"; } }
        public override System.Type CSType { get { return typeof(string); } }

        [PythonBinding]
        public object __new__(PyTypeObject type, object value)
        {
            return vm.PyStr(value);
        }

        [PythonBinding]
        public object __add__(string a, object b)
        {
            if (b is string) return a + (string)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __mul__(string a, object b)
        {
            if (b is int)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < (int)b; i++)
                {
                    sb.Append(a);
                }
                return sb.ToString();
            }
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __eq__(string a, object b)
        {
            if (b is string) return a == (string)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __lt__(string a, object b)
        {
            if (b is string) return a.CompareTo((string)b) < 0;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __le__(string a, object b)
        {
            if (b is string) return a.CompareTo((string)b) <= 0;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __gt__(string a, object b)
        {
            if (b is string) return a.CompareTo((string)b) > 0;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __ge__(string a, object b)
        {
            if (b is string) return a.CompareTo((string)b) >= 0;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __getitem__(string a, object b)
        {
            if (b is PySlice)
            {
                int start, stop, step;
                vm.ParseIntSlice((PySlice)b, a.Length, out start, out stop, out step);
                StringBuilder sb = new StringBuilder();
                if (step > 0)
                {
                    for (int i = start; i < stop; i += step) sb.Append(a[i]);
                }
                else
                {
                    for (int i = start; i > stop; i += step) sb.Append(a[i]);
                }
                return sb.ToString();
            }

            int index = vm.NormalizedIndex(vm.PyCast<int>(b), a.Length);
            return a[index].ToString();
        }

        [PythonBinding]
        public object __len__(string a)
        {
            return a.Length;
        }

        [PythonBinding]
        public object replace(string a, string old, string new_, params object[] args)
        {
            if (args.Length != 0)
            {
                if (args.Length == 1)
                {
                    int count = vm.PyCast<int>(args[0]);
                    int index = 0;
                    for (int i = 0; i < count; i++)
                    {
                        index = a.IndexOf(old, index);
                        if (index == -1) break;
                        a = a.Remove(index, old.Length).Insert(index, new_);
                        index += new_.Length;
                    }
                    return a;
                }
                else
                {
                    vm.TypeError("replace() takes at most 3 arguments");
                    return null;
                }
            }
            else
            {
                return a.Replace(old, new_);
            }
        }

        [PythonBinding]
        public object startswith(string a, string b)
        {
            return a.StartsWith(b);
        }

        [PythonBinding]
        public object join(string a, object b)
        {
            List<object> list = vm.PyList(b);
            return string.Join(a, list.ConvertAll<string>(x => vm.PyStr(x)).ToArray());
        }

        [PythonBinding]
        public object endswith(string a, string b)
        {
            return a.EndsWith(b);
        }

        [PythonBinding]
        public object __contains__(string a, object b)
        {
            if (b is string) return a.Contains((string)b);
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __repr__(string a)
        {
            return a.Escape();
        }

        [PythonBinding]
        public object lower(string a)
        {
            return a.ToLower();
        }

        [PythonBinding]
        public object upper(string a)
        {
            return a.ToUpper();
        }

        [PythonBinding]
        public object __str__(string a)
        {
            return a;
        }

        [PythonBinding]
        public string format(string a, params object[] args)
        {
            return typeof(string).GetMethod(
                "Format",
                BindingFlags.Public | BindingFlags.Static
            ).Invoke(null, args.Prepend(a)) as string;
        }
    }
}