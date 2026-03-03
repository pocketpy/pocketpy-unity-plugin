using System.Collections.Generic;
using UnityEngine;

namespace PocketPython
{
    /// <summary>
    /// Runs all the tests in the Tests folder.
    /// </summary>
    public class TestRunner : MonoBehaviour
    {
        public List<TextAsset> scripts = new List<TextAsset>();
        public VM vm { get; private set; }

        void Start()
        {
            vm = new VM();
            foreach (TextAsset script in scripts)
            {
                Debug.Log("> Tests/" + script.name + ".py");
                // if (script.name == "46_star") vm.debug = true;
                vm.Exec(script.text, script.name + ".py");
            }
        }

        void Update()
        {
            // do compile in update to see if memory leaks
            string s = Utils.LoadPythonLib("heapq");
            CodeObject co = vm.Compile(s, "heapq.py", CompileMode.EXEC_MODE);
            Utils.Assert(co != null);
        }
    }

}