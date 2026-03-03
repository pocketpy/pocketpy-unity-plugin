using System.Reflection;
using System;
using UnityEngine;

namespace PocketPython
{
    public enum BindingType
    {
        Method,
        Getter,
        Setter
    }

    public class PythonBindingAttribute : System.Attribute
    {
        public BindingType type;
        public PythonBindingAttribute(BindingType type = BindingType.Method)
        {
            this.type = type;
        }
    }

    public class PyTypeType : PyTypeObject
    {
        public override string Name => "type";
        public override Type CSType => typeof(Type);

        [PythonBinding]
        public object __new__(PyTypeObject type, object value)
        {
            return value.GetPyType(vm);
        }

        [PythonBinding]
        public object __repr__(PyTypeObject value)
        {
            return $"<class '{value.Name}'>";
        }

        [PythonBinding(BindingType.Getter)]
        public object __base__(PyTypeObject value)
        {
            return value.GetBaseType();
        }

        [PythonBinding(BindingType.Getter)]
        public string __name__(PyTypeObject value)
        {
            return value.Name;
        }
    }

    public abstract class PyTypeObject : PyObject
    {
        public abstract string Name { get; }
        public abstract System.Type CSType { get; }
        public VM vm { get; internal set; }
        public virtual object GetBaseType() => typeof(object).GetPyType(vm);

        public bool IsSubclassOf(PyTypeObject type)
        {
            object t = this;
            while (t != VM.None)
            {
                if (t == type) return true;
                t = (t as PyTypeObject).GetBaseType();
            }
            return false;
        }

        internal virtual void Initialize()
        {
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                PythonBindingAttribute bAttr = method.GetCustomAttribute<PythonBindingAttribute>();
                if (bAttr == null) continue;
                CSharpMethod cm = new CSharpMethod(this, method);
                if (bAttr.type == BindingType.Method)
                {
                    attr[method.Name] = cm;
                }
                else if (bAttr.type == BindingType.Getter)
                {
                    if (!attr.ContainsKey(method.Name))
                    {
                        var prop = new PyProperty();
                        prop.getter = cm;
                        prop.setter = VM.None;
                        attr[method.Name] = prop;
                    }
                    else
                    {
                        (attr[method.Name] as PyProperty).getter = cm;
                    }
                }
                else if (bAttr.type == BindingType.Setter)
                {
                    if (!attr.ContainsKey(method.Name))
                    {
                        var prop = new PyProperty();
                        prop.getter = VM.None;
                        prop.setter = cm;
                        attr[method.Name] = prop;
                    }
                    else
                    {
                        (attr[method.Name] as PyProperty).setter = cm;
                    }
                }
            }
        }
    }
}
