using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PocketPython
{
    public class PySuper
    {
        public object first;
        public PyTypeObject second;

        public PySuper(object first, PyTypeObject second)
        {
            this.first = first;
            this.second = second;
        }
    }

    public class PySuperType : PyTypeObject
    {
        public override string Name => "super";
        public override System.Type CSType => typeof(PySuper);
    }
}
