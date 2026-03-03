using System;
using System.Reflection;
using UnityEngine;

namespace PocketPython
{
    public class PyAutoTypeObject<T> : PyTypeObject
    {
        public override string Name => typeof(T).Name;
        public override Type CSType => typeof(T);

        private const BindingFlags iFlags = BindingFlags.Public | BindingFlags.Instance;

        internal override void Initialize()
        {
            base.Initialize();

            MethodInfo[] methods = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var method in methods)
            {
                if (method.Name.StartsWith("op_")) continue;
                attr[method.Name] = new CSharpLazyMethod(typeof(T), method.Name, true);
            }

            methods = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                attr[method.Name] = new CSharpLazyMethod(typeof(T), method.Name, false);
            }
        }

        [PythonBinding]
        public object __new__(PyTypeObject type, params object[] args)
        {
            return Activator.CreateInstance(typeof(T), args, null);
        }

        [PythonBinding]
        public object __getattr__(T value, string name)
        {
            var property = typeof(T).GetProperty(name, iFlags);
            if (property != null)
            {
                object val = property.GetValue(value);
                if (val == null) val = VM.None;
                return val;
            }
            var field = typeof(T).GetField(name, iFlags);
            if (field != null)
            {
                object val = field.GetValue(value);
                if (val == null) val = VM.None;
                return val;
            }
            vm.AttributeError(value, name);
            return null;
        }

        [PythonBinding]
        public object __setattr__(T value, string name, object attr)
        {
            if (attr == VM.None) attr = null;
            var property = typeof(T).GetProperty(name, iFlags);
            if (property != null)
            {
                property.SetValue(value, attr);
                return VM.None;
            }
            var field = typeof(T).GetField(name, iFlags);
            if (field != null)
            {
                field.SetValue(value, attr);
                return VM.None;
            }
            vm.AttributeError(value, name);
            return null;
        }

        [PythonBinding]
        public object __repr__(T value)
        {
            return value.ToString();
        }

        [PythonBinding]
        public object __add__(T a, object b)
        {
            var method = typeof(T).GetMethod("op_Addition", new Type[] { a.GetType(), b.GetType() });
            if (method == null) return VM.NotImplemented;
            return method.Invoke(null, new object[] { a, b });
        }

        [PythonBinding]
        public object __sub__(T a, object b)
        {
            var method = typeof(T).GetMethod("op_Subtraction", new Type[] { a.GetType(), b.GetType() });
            if (method == null) return VM.NotImplemented;
            return method.Invoke(null, new object[] { a, b });
        }

        [PythonBinding]
        public object __mul__(T a, object b)
        {
            var method = typeof(T).GetMethod("op_Multiply", new Type[] { a.GetType(), b.GetType() });
            if (method == null) return VM.NotImplemented;
            return method.Invoke(null, new object[] { a, b });
        }

        [PythonBinding]
        public object __rmul__(T a, object b)
        {
            var method = typeof(T).GetMethod("op_Multiply", new Type[] { b.GetType(), a.GetType() });
            if (method == null) return VM.NotImplemented;
            return method.Invoke(null, new object[] { b, a });
        }

        [PythonBinding]
        public object __truediv__(T a, object b)
        {
            var method = typeof(T).GetMethod("op_Division", new Type[] { a.GetType(), b.GetType() });
            if (method == null) return VM.NotImplemented;
            return method.Invoke(null, new object[] { a, b });
        }

        [PythonBinding]
        public object __lt__(T a, object b)
        {
            var method = typeof(T).GetMethod("op_LessThan", new Type[] { a.GetType(), b.GetType() });
            if (method == null) return VM.NotImplemented;
            return method.Invoke(null, new object[] { a, b });
        }

        [PythonBinding]
        public object __le__(T a, object b)
        {
            var method = typeof(T).GetMethod("op_LessThanOrEqual", new Type[] { a.GetType(), b.GetType() });
            if (method == null) return VM.NotImplemented;
            return method.Invoke(null, new object[] { a, b });
        }

        [PythonBinding]
        public object __gt__(T a, object b)
        {
            var method = typeof(T).GetMethod("op_GreaterThan", new Type[] { a.GetType(), b.GetType() });
            if (method == null) return VM.NotImplemented;
            return method.Invoke(null, new object[] { a, b });
        }

        [PythonBinding]
        public object __ge__(T a, object b)
        {
            var method = typeof(T).GetMethod("op_GreaterThanOrEqual", new Type[] { a.GetType(), b.GetType() });
            if (method == null) return VM.NotImplemented;
            return method.Invoke(null, new object[] { a, b });
        }

        [PythonBinding]
        public object __eq__(T a, object b)
        {
            return a.Equals(b);
        }

        [PythonBinding]
        public object __getitem__(T a, object b)
        {
            var method = typeof(T).GetMethod("get_Item", new Type[] { b.GetType() });
            if (method == null) vm.TypeError("object does not support indexing");
            return method.Invoke(a, new object[] { b });
        }

        [PythonBinding]
        public object __setitem__(T a, object b, object c)
        {
            var method = typeof(T).GetMethod("set_Item", new Type[] { b.GetType(), c.GetType() });
            if (method == null) vm.TypeError("object does not support indexing");
            method.Invoke(a, new object[] { b, c });
            return VM.None;
        }

        [PythonBinding]
        public object __len__(T a)
        {
            var property = typeof(T).GetProperty("Count", iFlags) ?? typeof(T).GetProperty("Length", iFlags);
            if (property == null) vm.TypeError("object does not support len()");
            return property.GetValue(a);
        }
    }

}
