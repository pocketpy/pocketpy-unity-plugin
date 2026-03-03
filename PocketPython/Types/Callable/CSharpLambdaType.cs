namespace PocketPython
{
    public delegate object NativeFuncC(VM vm, object[] args);

    public class CSharpLambda : ITrivialCallable
    {
        public int argc;
        public NativeFuncC f;

        public CSharpLambda(int argc, NativeFuncC f)
        {
            this.argc = argc;
            this.f = f;
        }
    }

    public class CSharpLambdaType : PyTypeObject
    {
        public override string Name => "CSharpLambda";
        public override System.Type CSType => typeof(CSharpLambda);

        [PythonBinding]
        public object __call__(CSharpLambda self, params object[] args)
        {
            if (self.argc != -1 && args.Length != self.argc)
            {
                vm.TypeError($"expected {self.argc} arguments, got {args.Length}");
            }
            return self.f(vm, args);
        }
    }
}