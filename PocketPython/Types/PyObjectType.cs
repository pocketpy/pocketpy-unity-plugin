using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{

    public class PyObjectType : PyTypeObject
    {
        public override string Name => "object";
        public override Type CSType => typeof(object);

        public override object GetBaseType() => VM.None;

        [PythonBinding]
        public object __new__(PyTypeObject type, params object[] _)
        {
            return new PyDynamic(type);
        }

        [PythonBinding]
        public object __repr__(object value)
        {
            return $"<{value.GetPyType(vm).Name} object at {value.GetHashCode()}>";
        }

        [PythonBinding]
        public object __eq__(object self, object other)
        {
            return self == other;
        }

        [PythonBinding]
        public int __hash__(object self)
        {
            return self.GetHashCode();
        }
    }
}