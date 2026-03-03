using System.Collections.Generic;

namespace PocketPython
{
    public static class Version
    {
        public const string Frontend = "1.1.4";
        public const string Backend = "0.8.x";
    }

    public class NoneType { internal NoneType() { } }
    public class NotImplementedType { internal NotImplementedType() { } }
    public class StopIterationType { internal StopIterationType() { } }
    public class EllipsisType { internal EllipsisType() { } }

    public class PyObject
    {
        public Dictionary<string, object> attr = new Dictionary<string, object>();

        public object this[string key]
        {
            get => attr[key];
            set => attr[key] = value;
        }
    }

    public class PyException : System.Exception
    {
        public PyException(string msg) : base(msg) { }
    }
}