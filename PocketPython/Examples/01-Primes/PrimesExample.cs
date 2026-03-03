using UnityEngine;

namespace PocketPython
{

    /// <summary>
    /// Example of using PocketPython to find prime numbers.
    /// </summary>
    public class PrimesExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var vm = new VM();
            const string source = @"
def is_prime(x):
  if x < 2:
    return False
  for i in range(2, x):
    if x % i == 0:
      return False
  return True

primes = [i for i in range(2, 20) if is_prime(i)]
print(primes)
";
            CodeObject code = vm.Compile(source, "main.py", CompileMode.EXEC_MODE);
            vm.Exec(code);  // [2, 3, 5, 7, 11, 13, 17, 19]
        }
    }

}