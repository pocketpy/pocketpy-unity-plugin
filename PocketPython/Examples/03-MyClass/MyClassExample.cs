using UnityEngine;
using System;

namespace PocketPython
{
    public enum MyEnum{
        A,
        B,
        C
    }

    public class MyClass
    {
        public string title;
        public string msg;

        public void Print()
        {
            Debug.Log(title + ": " + msg);
        }
    }

    public class PyMyclassType : PyTypeObject
    {
        public override string Name => "my_class";
        public override Type CSType => typeof(MyClass);

        [PythonBinding]
        public object __new__(PyTypeObject cls)
        {
            return new MyClass();
        }

        [PythonBinding(BindingType.Getter)]
        public string title(MyClass value) => value.title;

        [PythonBinding(BindingType.Getter)]
        public string msg(MyClass value) => value.msg;

        [PythonBinding(BindingType.Setter)]
        public void title(MyClass value, string title) => value.title = title;

        [PythonBinding(BindingType.Setter)]
        public void msg(MyClass value, string msg) => value.msg = msg;

        [PythonBinding]
        public void print(MyClass value) => value.Print();
    }


    /// <summary>
    /// Example of binding a custom C# class to Python.
    /// </summary>
    public class MyClassExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var vm = new VM();

            // register MyClass type into the builtins module
            vm.RegisterType(new PyMyclassType(), vm.builtins);
            vm.Exec("print(my_class)", "main.py"); // <class 'my_class'>
            vm.Exec("c = my_class()", "main.py");
            vm.Exec("c.title = 'Greeting'", "main.py");
            vm.Exec("c.msg = 'Hello, world!'", "main.py");

            string title = vm.Eval("c.title").ToString();
            string msg = vm.Eval("c.msg").ToString();

            Debug.Log(title + ": " + msg); // Greeting: Hello, world!

            vm.Exec("c.print()", "main.py"); // Greeting: Hello, world!

            vm.RegisterEnumType<MyEnum>("my_enum", vm.builtins);
            int a = (int)vm.Eval("my_enum.A");
            int b = (int)vm.Eval("my_enum.B");
            Debug.Log(a + ", " + b); // 0, 1
        }
    }

}