using System;
using UnityEngine;

namespace PocketPython
{
    public class PyFloatType : PyTypeObject
    {
        public override string Name => "float";
        public override Type CSType => typeof(float);

        [PythonBinding]
        public object __new__(PyTypeObject type, object value)
        {
            if (value is float) return (float)value;
            if (value is int) return (float)(int)value;
            if (value is string) return float.Parse((string)value);
            vm.TypeError("expected int, float or string, got " + type.Name);
            return 0;
        }

        [PythonBinding]
        public object __add__(float a, object b)
        {
            if (b is int) return a + (int)b;
            if (b is float) return a + (float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __sub__(float a, object b)
        {
            if (b is int) return a - (int)b;
            if (b is float) return a - (float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __mul__(float a, object b)
        {
            if (b is int) return a * (int)b;
            if (b is float) return a * (float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __truediv__(float a, object b)
        {
            if (b is int) return a / (float)(int)b;
            if (b is float) return a / (float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __pow__(float a, object b)
        {
            if (b is int) return Mathf.Pow(a, (int)b);
            if (b is float) return Mathf.Pow(a, (float)b);
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __repr__(float value)
        {
            return value.ToString("0.0########");
        }

        [PythonBinding]
        public object __eq__(float a, object b)
        {
            if (b is int) return a == (int)b;
            if (b is float) return a == (float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __lt__(float a, object b)
        {
            if (b is int) return a < (int)b;
            if (b is float) return a < (float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __gt__(float a, object b)
        {
            if (b is int) return a > (int)b;
            if (b is float) return a > (float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __le__(float a, object b)
        {
            if (b is int) return a <= (int)b;
            if (b is float) return a <= (float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __ge__(float a, object b)
        {
            if (b is int) return a >= (int)b;
            if (b is float) return a >= (float)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __neg__(float value)
        {
            return -value;
        }
    }

}
