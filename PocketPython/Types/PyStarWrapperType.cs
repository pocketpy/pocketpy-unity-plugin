using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{
    public class PyStarWrapper
    {
        public object obj;
        public int level;

        public PyStarWrapper(object obj, int level)
        {
            this.obj = obj;
            this.level = level;
        }
    }

    public class PyStarWrapperType : PyTypeObject
    {
        public override string Name => "_star_wrapper";
        public override System.Type CSType => typeof(PyStarWrapper);
    }
}
