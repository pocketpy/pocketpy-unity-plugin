using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{
    public class PyDynamic : PyObject
    {
        public PyTypeObject type;

        public PyDynamic(PyTypeObject type)
        {
            this.type = type;
        }

        public override string ToString()
        {
            return $"<{type.Name} object at {this.GetHashCode()}>";
        }
    }

    public class PyDynamicType : PyTypeObject
    {
        public string mName;
        public PyTypeObject mBase;

        public PyDynamicType(string name, PyTypeObject baseType)
        {
            mName = name;
            mBase = baseType;
        }

        public override string Name => this.mName;
        public override Type CSType => mBase.CSType;
        public override object GetBaseType() => mBase;

        public override string ToString()
        {
            return $"<class '{Name}'>";
        }
    }

}
