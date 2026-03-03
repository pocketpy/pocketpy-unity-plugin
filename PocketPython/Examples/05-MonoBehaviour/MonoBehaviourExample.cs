using UnityEngine;
using UnityEngine.UI;

namespace PocketPython
{
    /// <summary>
    /// This is an example of how to use Python to write a MonoBehaviour.
    /// </summary>
    public class MonoBehaviourExample : MonoBehaviour
    {
        Text text;

        const string script = @"
x = 0

def start():
    print('start!!')

def update():
    global x
    x += 1
    text.text = str(x)
";

        VM vm;
        PyModule mod;

        // Start is called before the first frame update
        void Start()
        {
            text = GetComponent<Text>();
            try
            {
                vm = new VM();
                vm.RegisterAutoType<Text>();

                mod = vm.NewModule("test");

                mod["text"] = text;
                vm.Exec(script, "test.py", CompileMode.EXEC_MODE, mod);

                // Call the function "start"
                vm.Call(mod["start"]);
            }
            catch (System.Exception e)
            {
                text.text = e.ToString();
            }

        }

        // Update is called once per frame
        void Update()
        {
            // Call the function "update"
            vm.Call(mod["update"]);
        }
    }

}

