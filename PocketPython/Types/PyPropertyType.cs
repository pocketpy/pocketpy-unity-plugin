using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{
    public class PyProperty
    {
        public object getter;
        public object setter;
    }

    public class PyPropertyType : PyTypeObject
    {
        public override string Name => "property";
        public override System.Type CSType => typeof(PyProperty);

        [PythonBinding]
        public object __new__(PyTypeObject type, params object[] args)
        {
            if (args.Length == 1)
            {
                return new PyProperty()
                {
                    getter = args[0],
                    setter = VM.None
                };
            }
            else if (args.Length == 2)
            {
                return new PyProperty()
                {
                    getter = args[0],
                    setter = args[1]
                };
            }
            else
            {
                vm.TypeError("property() takes 1 or 2 arguments");
                return null;
            }
        }
    }
}