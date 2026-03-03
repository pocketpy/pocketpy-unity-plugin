using System;

namespace PocketPython
{
    /// <summary>
    /// A function defined in Python.
    /// </summary>
    public class PyFunction : ITrivialCallable
    {
        public FuncDecl decl;
        public PyModule module;

        public PyFunction(FuncDecl decl, PyModule module)
        {
            this.decl = decl;
            this.module = module;
        }

        public override string ToString()
        {
            return $"{decl.code.name}()";
        }
    }

    public class PyFunctionType : PyTypeObject
    {
        public override string Name => "function";
        public override Type CSType => typeof(PyFunction);
    }
}
