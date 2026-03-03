using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{
    public class PyBoundMethod
    {
        public object self;
        public object func;

        public PyBoundMethod(object self, object func)
        {
            this.self = self;
            this.func = func;
        }
    }

    public class PyBoundMethodType : PyTypeObject
    {
        public override string Name => "bound_method";
        public override System.Type CSType => typeof(PyBoundMethod);

        [PythonBinding]
        public object __eq__(PyBoundMethod self, object other)
        {
            PyBoundMethod otherMethod = other as PyBoundMethod;
            if (otherMethod == null) return VM.NotImplemented;
            return self.self == otherMethod.self && self.func == otherMethod.func;
        }
    }
}