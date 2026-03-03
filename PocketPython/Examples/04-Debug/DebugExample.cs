using UnityEngine;

namespace PocketPython
{

    /// <summary>
    /// Example of using `debug` flag to see bytecode execution details.
    /// </summary>
    public class DebugExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var vm = new VM();
            vm.debug = true;

            const string code = @"
a = [1, 2, 3]

def f(x):
    return x[5]     # IndexError!!

print(f(a))";

            vm.Exec(code, "main.py");
        }
    }


}