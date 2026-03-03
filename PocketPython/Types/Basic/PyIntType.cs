using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{
    public class PyIntType : PyTypeObject
    {
        public override string Name { get { return "int"; } }
        public override System.Type CSType { get { return typeof(int); } }

        [PythonBinding]
        public object __new__(PyTypeObject type, object value)
        {
            if (value is int) return (int)value;
            if (value is float) return (int)(float)value;
            if (value is string) return int.Parse((string)value);
            vm.TypeError("expected int, float or string, got " + type.Name);
            return 0;
        }

        [PythonBinding]
        public object __add__(int a, object b)
        {
            if (b is int) return a + (int)b;
            if (b is float) return a + (int)(float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __sub__(int a, object b)
        {
            if (b is int) return a - (int)b;
            if (b is float) return a - (int)(float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __mul__(int a, object b)
        {
            if (b is int) return a * (int)b;
            if (b is float) return a * (int)(float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __truediv__(int a, object b)
        {
            if (b is int) return a / (float)(int)b;
            if (b is float) return a / (float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __floordiv__(int a, object b)
        {
            if (b is int) return a / (int)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __mod__(int a, object b)
        {
            if (b is int) return a % (int)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __pow__(int a, object b)
        {
            if (b is int)
            {
                int result = 1;
                for (int i = 0; i < (int)b; i++) result *= a;
                return result;
            }
            if (b is float) return Mathf.Pow(a, (float)b);
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __eq__(int a, object b)
        {
            if (b is int) return a == (int)b;
            if (b is float) return a == (int)(float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __lt__(int a, object b)
        {
            if (b is int) return a < (int)b;
            if (b is float) return a < (int)(float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __gt__(int a, object b)
        {
            if (b is int) return a > (int)b;
            if (b is float) return a > (int)(float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __le__(int a, object b)
        {
            if (b is int) return a <= (int)b;
            if (b is float) return a <= (int)(float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __ge__(int a, object b)
        {
            if (b is int) return a >= (int)b;
            if (b is float) return a >= (int)(float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __neg__(int a)
        {
            return -a;
        }

        [PythonBinding]
        public object __repr__(int a)
        {
            return a.ToString();
        }

        [PythonBinding]
        public object __lshift__(int a, object b)
        {
            if (b is int) return a << (int)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __rshift__(int a, object b)
        {
            if (b is int) return a >> (int)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __and__(int a, object b)
        {
            if (b is int) return a & (int)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __or__(int a, object b)
        {
            if (b is int) return a | (int)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __xor__(int a, object b)
        {
            if (b is int) return a ^ (int)b;
            return VM.NotImplemented;
        }
    }
}