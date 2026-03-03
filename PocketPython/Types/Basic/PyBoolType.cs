using System;

namespace PocketPython
{
    public class PyBoolType : PyTypeObject
    {
        public override string Name => "bool";
        public override Type CSType => typeof(bool);

        [PythonBinding]
        public object __new__(PyTypeObject type, object value)
        {
            return vm.PyBool(value);
        }

        [PythonBinding]
        public object __repr__(bool value)
        {
            return value ? "True" : "False";
        }

        [PythonBinding]
        public object __and__(bool a, object b)
        {
            if (b is bool) return a && (bool)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __or__(bool a, object b)
        {
            if (b is bool) return a || (bool)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __xor__(bool a, object b)
        {
            if (b is bool) return a ^ (bool)b;
            return VM.NotImplemented;
        }

        [PythonBinding]
        public object __eq__(bool a, object b)
        {
            if (b is bool) return a == (bool)b;
            return VM.NotImplemented;
        }
    }
}
