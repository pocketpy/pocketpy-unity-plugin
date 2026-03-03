using UnityEngine;

namespace PocketPython
{

    /// <summary>
    /// Example of making UnityEngine.Vector2 available to Python.
    /// </summary>
    public class Vector2Example : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var vm = new VM();
            // register UnityEngine.Vector2 type into the builtins module
            vm.RegisterAutoType<Vector2>(vm.builtins);

            vm.Exec("print(Vector2)", "main.py"); // <class 'Vector2'>
            vm.Exec("v = Vector2(1, 2)", "main.py");
            vm.Exec("print(v)", "main.py"); // (1.0, 2.0)
            vm.Exec("print(v.x)", "main.py"); // 1.0
            vm.Exec("print(v.y)", "main.py"); // 2.0
            vm.Exec("print(v.magnitude)", "main.py"); // 2.236068
            vm.Exec("print(v.normalized)", "main.py"); // (0.4472136, 0.8944272)
            vm.Exec("print(Vector2.Dot(v, v))", "main.py"); // 5.0
            vm.Exec("print(Vector2.get_up())", "main.py"); // (0.0, 1.0)

            Vector2 v = (Vector2)vm.Eval("Vector2(3, 4) + v");
            Debug.Log(v); // (4.0, 6.0)
        }
    }

}