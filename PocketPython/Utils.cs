using UnityEngine;

namespace PocketPython
{
    public class InternalException : System.Exception
    {
        public InternalException(string msg) : base(msg) { }
    }

    public static class Utils
    {
        public static void Assert(bool ok)
        {
            if (!ok)
            {
                throw new InternalException("Assertion failed");
            }
        }

        public static void Assert(bool ok, string msg)
        {
            if (!ok)
            {
                throw new InternalException(msg);
            }
        }

        public static string LoadPythonLib(string key)
        {
            var t = Resources.Load<TextAsset>("PocketPython/Python/" + key);
            Utils.Assert(t != null, $"{key}.py not found");
            return t.text;
        }
    }

}
