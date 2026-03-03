using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{
    public class PyModule : PyObject
    {
        public string name { get; private set; }

        public PyModule(string name)
        {
            this.name = name;
        }
    }

    public class PyModuleType : PyTypeObject
    {
        public override string Name => "module";
        public override System.Type CSType => typeof(PyModule);
    }
}