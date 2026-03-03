using System;
using System.Reflection;

namespace PocketPython
{
    public class CSharpMethod : PyObject, ITrivialCallable
    {
        public MethodInfo method;
        public PyTypeObject target;

        // cached
        public bool IsVariadic;
        public ParameterInfo[] parameters;

        public CSharpMethod(PyTypeObject target, MethodInfo method)
        {
            this.target = target;
            this.method = method;

            this.parameters = method.GetParameters();
            this.IsVariadic = parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
        }

        public override string ToString()
        {
            return method.Name + "()";
        }

        public object Invoke(VM vm, object[] args)
        {
            Utils.Assert(vm == target.vm);
            if (IsVariadic)
            {
                int normalCount = parameters.Length - 1;
                if (args.Length < normalCount)
                {
                    vm.TypeError($"expected at least {normalCount} arguments, got {args.Length}");
                }
                object[] newArgs = new object[parameters.Length];
                for (int i = 0; i < normalCount; i++) newArgs[i] = args[i];
                object[] variadicArgs = new object[args.Length - normalCount];
                for (int i = 0; i < variadicArgs.Length; i++) variadicArgs[i] = args[i + normalCount];
                newArgs[normalCount] = variadicArgs;
                return method.Invoke(target, newArgs);
            }
            else
            {
                if (args.Length != parameters.Length)
                {
                    vm.TypeError($"expected {parameters.Length} arguments, got {args.Length} (${method.Name})");
                }
                return method.Invoke(target, args);
            }
        }
    }

    public class CSharpMethodType : PyTypeObject
    {
        public override string Name => "CSharpMethod";
        public override Type CSType => typeof(CSharpMethod);

        [PythonBinding]
        public string __repr__(CSharpMethod value)
        {
            return $"<method '{value.method.Name}'>";
        }
    }
}