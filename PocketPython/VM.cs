using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace PocketPython
{
    public partial class VM
    {
        public static NoneType None = new NoneType();
        public static EllipsisType Ellipsis = new EllipsisType();
        public static NotImplementedType NotImplemented = new NotImplementedType();
        public static StopIterationType StopIteration = new StopIterationType();

        private static IntPtr p;

        public Stack<Frame> callStack = new Stack<Frame>();
        public PyModule builtins;
        public PyModule main;
        public bool debug = false;
        public int maxRecursionDepth = 100;

        public System.Action<string> stdout = Debug.Log;
        public System.Action<string> stderr = null;

        public Dictionary<string, PyModule> modules = new Dictionary<string, PyModule>();
        public Dictionary<string, string> lazyModules = new Dictionary<string, string>();

        public Dictionary<Type, PyTypeObject> allTypes = new Dictionary<Type, PyTypeObject>();

        public VM()
        {
            if (p == IntPtr.Zero) p = Bindings.pkpy_new_vm(false);

            builtins = NewModule("builtins");
            main = NewModule("__main__");

            RegisterType(new PyObjectType());       // object
            RegisterType(new PyTypeType());         // System.Type
            RegisterType(new PyIntType());          // int
            RegisterType(new PyFloatType());        // float
            RegisterType(new PyBoolType());         // bool
            RegisterType(new PyStrType());          // string
            RegisterType(new PyListType());         // List<object>
            RegisterType(new PyTupleType());        // object[]

            RegisterType(new PySliceType());
            RegisterType(new PyRangeType());
            RegisterType(new PyModuleType());
            RegisterType(new PySuperType());
            RegisterType(new PyDictType());
            RegisterType(new PyPropertyType());
            RegisterType(new PyStarWrapperType());
            RegisterType(new PyIteratorType());

            RegisterType(new CSharpMethodType());
            RegisterType(new CSharpLazyMethodType());
            RegisterType(new CSharpLambdaType());
            RegisterType(new PyFunctionType());
            RegisterType(new PyBoundMethodType());

            builtins["type"] = typeof(Type).GetPyType(this);
            builtins["object"] = typeof(object).GetPyType(this);
            builtins["bool"] = typeof(bool).GetPyType(this);
            builtins["int"] = typeof(int).GetPyType(this);
            builtins["float"] = typeof(float).GetPyType(this);
            builtins["str"] = typeof(string).GetPyType(this);
            builtins["list"] = typeof(List<object>).GetPyType(this);
            builtins["tuple"] = typeof(object[]).GetPyType(this);
            builtins["range"] = typeof(PyRange).GetPyType(this);
            builtins["dict"] = typeof(PyDict).GetPyType(this);
            builtins["property"] = typeof(PyProperty).GetPyType(this);
            builtins["StopIteration"] = StopIteration;
            builtins["NotImplemented"] = NotImplemented;
            builtins["slice"] = typeof(PySlice).GetPyType(this);

            /*******************************************************/
            BindBuiltinFunc("repr", 1, (VM vm, object[] args) => PyRepr(args[0]));
            BindBuiltinFunc("len", 1, (VM vm, object[] args) => CallMethod(args[0], "__len__"));
            BindBuiltinFunc("iter", 1, (VM vm, object[] args) => vm.PyIter(args[0]));
            BindBuiltinFunc("next", 1, (VM vm, object[] args) => vm.PyNext(args[0]));
            BindBuiltinFunc("super", 2, (VM vm, object[] args) =>
            {
                Utils.Assert(args[0] is PyTypeObject, "super(): first arg must be type");
                // TODO: assert isinstance(args[1], args[0])
                object @base = (args[0] as PyTypeObject).GetBaseType();
                Utils.Assert(@base != None, "super(): object does not have a base");
                return new PySuper(args[1], @base as PyTypeObject);
            });
            BindBuiltinFunc("isinstance", 2, (VM vm, object[] args) => IsInstance(args[0], args[1] as PyTypeObject));

            BindBuiltinFunc("getattr", 2, (VM vm, object[] args) => GetAttr(args[0], PyCast<string>(args[1])));
            BindBuiltinFunc("setattr", 3, (VM vm, object[] args) => SetAttr(args[0], PyCast<string>(args[1]), args[2]));
            BindBuiltinFunc("hasattr", 2, (VM vm, object[] args) => HasAttr(args[0], PyCast<string>(args[1])));

            BindBuiltinFunc("dir", 1, (VM vm, object[] args) =>
            {
                List<object> result = new List<object>();
                if (args[0] is PyObject obj)
                {
                    foreach (var kv in obj.attr)
                    {
                        result.Add(kv.Key);
                    }
                }
                return result;
            });

            BindBuiltinFunc("__sys_stdout_write", 1, (VM vm, object[] args) =>
            {
                stdout((string)args[0]);
                return None;
            });

            Exec(Utils.LoadPythonLib("builtins"), "<builtins>", CompileMode.EXEC_MODE, builtins);
            Exec(Utils.LoadPythonLib("_set"), "<set>", CompileMode.EXEC_MODE, builtins);
            foreach (string name in new string[] { "bisect", "collections", "heapq" })
            {
                lazyModules[name] = Utils.LoadPythonLib(name);
            }
        }

        public PyModule NewModule(string name)
        {
            Utils.Assert(!modules.ContainsKey(name), $"Module {name} already exists");
            var module = new PyModule(name);
            modules[name] = module;
            return module;
        }

        public PyDynamicType NewTypeObject(PyModule module, string name, PyTypeObject @base)
        {
            string fullname;
            if (module != builtins) fullname = module.name + "." + name;
            else fullname = name;
            PyDynamicType type = new PyDynamicType(fullname, @base);
            module[name] = type;
            return type;
        }

        public void RegisterType(PyTypeObject type, PyModule module = null)
        {
            type.vm = this;
            type.Initialize();
            Utils.Assert(!allTypes.ContainsKey(type.CSType), $"Type {type.CSType} already exists");
            allTypes[type.CSType] = type;
            if (module != null) module[type.Name] = type;
        }

        public void RegisterAutoType<T>(PyModule module = null)
        {
            PyTypeObject type = new PyAutoTypeObject<T>();
            RegisterType(type, module);
        }

        public void RegisterEnumType<T>(string name, PyModule module = null) where T: System.Enum{
            PyTypeObject @base = typeof(object).GetPyType(this);
            PyTypeObject type = NewTypeObject(module ?? builtins, name, @base);
            foreach (var value in Enum.GetValues(typeof(T))){
                string key = Enum.GetName(typeof(T), value);
                type[key] = value;
            }
        }

        public CSharpLambda BindBuiltinFunc(string name, int argc, NativeFuncC f)
        {
            return BindFunc(builtins, name, argc, f);
        }

        public CSharpLambda BindFunc(PyObject obj, string name, int argc, NativeFuncC f)
        {
            var func = new CSharpLambda(argc, f);
            obj[name] = func;
            return func;
        }

        public CodeObject Compile(string source, string filename, CompileMode mode)
        {
            Bindings.pkpy_compile_to_string(p, source, filename, (int)mode, out bool ok, out string res);
            if (ok)
            {
                return CodeObject.FromBytes(res, source);
            }
            else
            {
                Error("CompileError", res);
                return null;
            }
        }

        public object Call(object callable){
            return Call(callable, new object[0], null);
        }

        public object Call(object callable, object[] args, Dictionary<string, object> kwargs)
        {
            Utils.Assert(callable != null, "callable must not be null");
            object self;
            if (callable is PyBoundMethod bm)
            {
                return Call(bm.func, args.Prepend(bm.self), kwargs);
            }
            if (callable is CSharpMethod cm)
            {
                Utils.Assert(kwargs == null || kwargs.Count == 0, "CSharpMethod does not support kwargs");
                object res = cm.Invoke(this, args);
                if (res == null) res = None;
                return res;
            }
            if (callable is CSharpLazyMethod clm)
            {
                Utils.Assert(kwargs == null || kwargs.Count == 0, "CSharpLazyMethod does not support kwargs");
                object res = clm.Invoke(this, args);
                if (res == null) res = None;
                return res;
            }
            if (callable is PyFunction f)
            {
                Frame frame = new Frame(f.decl.code, f.module);
                FuncDecl decl = f.decl;
                CodeObject co = decl.code;
                int i = 0;
                if (args.Length < decl.args.Count) TypeError($"expected {decl.args.Count} positional arguments, got {args.Length}");
                // prepare args
                foreach (int ni in decl.args)
                {
                    string name = I2N(co.varnames[ni]);
                    frame.locals[name] = args[i++];
                }
                // prepare kwdefaults
                foreach (FuncDecl.KwArg kv in decl.kwargs)
                {
                    string name = I2N(co.varnames[kv.key]);
                    frame.locals[name] = kv.value;
                }
                // handle *args
                if (decl.starredArg != -1)
                {
                    object[] rest = new object[args.Length - decl.args.Count];
                    Array.Copy(args, decl.args.Count, rest, 0, rest.Length);
                    string name = I2N(co.varnames[decl.starredArg]);
                    frame.locals[name] = rest;
                }
                else
                {
                    // kwdefaults override
                    foreach (FuncDecl.KwArg kv in decl.kwargs)
                    {
                        if (i >= args.Length) break;
                        string name = I2N(co.varnames[kv.key]);
                        frame.locals[name] = args[i++];
                    }
                    if (i < args.Length) TypeError($"too many arguments ({f.decl.code.name})");
                }

                PyDict vkwargs = null;
                if (decl.starredKwarg != -1)
                {
                    vkwargs = new PyDict();
                    string name = I2N(co.varnames[decl.starredKwarg]);
                    frame.locals[name] = vkwargs;
                }

                if (kwargs != null)
                {
                    foreach (KeyValuePair<string, object> kv in kwargs)
                    {
                        bool ok = frame.locals.ContainsKey(kv.Key);
                        if (!ok)
                        {
                            if (vkwargs == null)
                            {
                                TypeError($"unexpected keyword argument '{kv.Key}'");
                            }
                            vkwargs[new PyDictKey(this, kv.Key)] = kv.Value;
                        }
                        else
                        {
                            frame.locals[kv.Key] = kv.Value;
                        }
                    }
                }

                if (co.isGenerator)
                {
                    NotImplementedError();
                }

                if (callStack.Count >= maxRecursionDepth)
                {
                    Error("RecursionError", "maximum recursion depth exceeded");
                    return null;
                }
                callStack.Push(frame);
                return RunTopFrame();
            }

            if (callable is PyTypeObject type)
            {
                // __new__
                object new_f = FindNameInMro(type, "__new__");
                object res = Call(new_f, args.Prepend(type), kwargs);
                // __init__

                object init_f = GetUnboundMethod(res, "__init__", out self, false);
                if (init_f != null) Call(init_f, args.Prepend(self), kwargs);
                return res;
            }

            object call_f = GetUnboundMethod(callable, "__call__", out self, false);
            if (call_f != null) return Call(call_f, args.Prepend(self), kwargs);

            TypeError("'" + callable.GetPyType(this).Name + "' object is not callable");
            return null;
        }

        public object Eval(string source, PyModule mod = null)
        {
            CodeObject code = Compile(source, "<eval>", CompileMode.EVAL_MODE);
            return Exec(code, mod);
        }

        public object Exec(CodeObject co, PyModule mod = null)
        {
            mod ??= main;
            callStack.Push(new Frame(co, mod));
            try
            {
                return RunTopFrame();
            }
            catch (PyException e)
            {
                if (stderr == null) throw;
                else stderr(e.Message);
                return null;
            }
            catch (Exception e)
            {
                if (stderr == null) throw;
                else
                {
                    while (e.InnerException != null) e = e.InnerException;
                    string msg = MakeErrorMsgAndClearStack(e.GetType().Name, e.Message);
                    stderr(msg);
                }
                return null;
            }
        }

        public object Exec(string source, string filename, CompileMode mode = CompileMode.EXEC_MODE, PyModule mod = null)
        {
            var co = Compile(source, filename, mode);
            return Exec(co, mod);
        }

        public object CallMethod(object obj, string name, params object[] args)
        {
            object f = GetUnboundMethod(obj, name, out object self);
            return Call(f, args.Prepend(self), null);
        }

        public object CallMethod(object self, object callable, params object[] args)
        {
            return Call(callable, args.Prepend(self), null);
        }

        private string MakeErrorMsgAndClearStack(string name, string msg)
        {
            if (callStack.Count == 0)
            {
                return name + ": " + msg;
            }
            Frame frame = callStack.Peek();
            string filename = frame.co.filename;
            string line = frame.GetCurrentLine(out int lineno).TrimStart();
            string pos = "File \"" + filename + "\", line " + lineno + "\n";
            pos += "  " + line;
            callStack.Clear();
            msg = pos + "\n" + name + ": " + msg;
            return msg;
        }

        public void Error(string name, string msg)
        {
            msg = MakeErrorMsgAndClearStack(name, msg);
            throw new PyException(msg);
        }

        public void NameError(string name)
        {
            Error("NameError", "name '" + name + "' is not defined");
        }

        public void TypeError(string msg)
        {
            Error("TypeError", msg);
        }

        public void AttributeError(object obj, string name)
        {
            Error("AttributeError", $"'{obj.GetPyType(this).Name}' object has no attribute '{name}'");
        }

        public void IndexError(string msg)
        {
            Error("IndexError", msg);
        }

        public void ValueError(string msg)
        {
            Error("ValueError", msg);
        }

        public void KeyError(object key)
        {
            Error("KeyError", PyRepr(key));
        }

        public void NotImplementedError()
        {
            Error("NotImplementedError", "");
        }

        public void NotImplementedOpcode(ushort op)
        {
            Error("NotImplementedError", ((Opcode)op).ToString() + " is not supported yet");
        }

        public void CheckType<T>(object t)
        {
            if (t is T) return;
            TypeError($"expected {typeof(T).GetPyType(this).Name.Escape()}, got {t.GetPyType(this).Name.Escape()}");
        }

        public bool IsInstance(object obj, PyTypeObject type)
        {
            return obj.GetPyType(this).IsSubclassOf(type);
        }

        public T PyCast<T>(object obj)
        {
            CheckType<T>(obj);
            return (T)obj;
        }

        public bool PyEquals(object lhs, object rhs)
        {
            if (lhs == rhs) return true;
            object res;
            res = CallMethod(lhs, "__eq__", rhs);
            if (res != NotImplemented) return (bool)res;
            res = CallMethod(rhs, "__eq__", lhs);
            if (res != NotImplemented) return (bool)res;
            return false;
        }

        object PyIter(object obj)
        {
            object f = GetUnboundMethod(obj, "__iter__", out object self, false);
            if (f != null) return CallMethod(self, f);
            if (obj is IEnumerator enumerator) return new PyIterator(enumerator);
            if (obj is IEnumerable enumerable) return new PyIterator(enumerable.GetEnumerator());
            TypeError($"'{obj.GetPyType(this).Name}' object is not iterable");
            return null;
        }

        object PyNext(object obj)
        {
            object f = GetUnboundMethod(obj, "__next__", out object self, false);
            if (f != null) return CallMethod(self, f);
            if (obj is PyIterator it)
            {
                if (it.MoveNext())
                {
                    object val = it.Current;
                    if (val is char v) return new string(v, 1);
                    return val;
                }
                return StopIteration;
            }
            TypeError($"'{obj.GetPyType(this).Name}' object is not an iterator");
            return null;
        }

        public bool PyBool(object obj)
        {
#pragma warning disable IDE0038
            if (obj is bool) return (bool)obj;
            if (obj == None) return false;
            if (obj is int) return (int)obj != 0;
            if (obj is float) return (float)obj != 0.0f;
#pragma warning restore IDE0038
            // check __len__ for other types
            object f = GetUnboundMethod(obj, "__len__", out object self, false);
            if (f != null) return (int)CallMethod(self, f) > 0;
            return true;
        }

        public string PyStr(object obj)
        {
            object f = GetUnboundMethod(obj, "__str__", out object self, false);
            if (f != null) return (string)CallMethod(self, f);
            return PyRepr(obj);
        }

        public string PyRepr(object obj)
        {
            return (string)CallMethod(obj, "__repr__");
        }

        public int PyHash(object obj)
        {
            return (int)CallMethod(obj, "__hash__");
        }

        public PyModule PyImport(string key)
        {
            if (modules.TryGetValue(key, out PyModule module))
            {
                return module;
            }
            if (lazyModules.TryGetValue(key, out string source))
            {
                module = new PyModule(key);
                modules[key] = module;
                lazyModules.Remove(key);
                Exec(source, key + ".py", CompileMode.EXEC_MODE, module);
                return module;
            }
            Error("ImportError", "cannot import name " + key.Escape());
            return null;
        }

        public List<object> PyList(object obj)
        {
            object it = PyIter(obj);
            var res = new List<object>();
            while (true)
            {
                object next = PyNext(it);
                if (next == StopIteration) break;
                res.Add(next);
            }
            return res;
        }

        public object GetUnboundMethod(object obj, string name, out object self, bool throwErr = true, bool fallback = false)
        {
            self = null;
            PyTypeObject objtype;
            // handle super() proxy
            if (obj is PySuper super)
            {
                obj = super.first;
                objtype = super.second;
            }
            else
            {
                objtype = obj.GetPyType(this);
            }
            object clsVar = FindNameInMro(objtype, name);

            if (fallback)
            {
                if (clsVar != null)
                {
                    // handle descriptor
                    if (clsVar is PyProperty prop)
                    {
                        return Call(prop.getter, new object[] { obj }, null);
                    }
                }
                // handle instance __dict__
                if (obj is PyObject)
                {
                    if ((obj as PyObject).attr.TryGetValue(name, out object val))
                    {
                        return val;
                    }
                }
            }
            if (clsVar != null)
            {
                // bound method is non-data descriptor
                if (clsVar is ITrivialCallable)
                {
                    self = obj;
                }
                return clsVar;
            }
            if (throwErr) AttributeError(obj, name);
            return null;
        }

        public object GetAttr(object obj, string name, bool throwErr = true)
        {
            PyTypeObject objtype;
            // handle super() proxy
            if (obj is PySuper)
            {
                PySuper super = obj as PySuper;
                obj = super.first;
                objtype = super.second;
            }
            else
            {
                objtype = obj.GetPyType(this);
            }
            object clsVar = FindNameInMro(objtype, name);
            if (clsVar != null)
            {
                // handle descriptor
                if (clsVar is PyProperty)
                {
                    PyProperty prop = clsVar as PyProperty;
                    return Call(prop.getter, new object[] { obj }, null);
                }
            }
            // handle instance __dict__
            if (obj is PyObject)
            {
                if ((obj as PyObject).attr.TryGetValue(name, out object val))
                {
                    return val;
                }
            }
            if (clsVar != null)
            {
                // bound method is non-data descriptor
                if (clsVar is ITrivialCallable)
                {
                    return new PyBoundMethod(obj, clsVar);
                }
                return clsVar;
            }

            object getattr = FindNameInMro(objtype, "__getattr__");
            if (getattr != null)
            {
                return Call(getattr, new object[] { obj, name }, null);
            }
            if (throwErr) AttributeError(obj, name);
            return null;
        }

        public bool HasAttr(object obj, string name)
        {
            object res = GetAttr(obj, name, false);
            return res != null;
        }

        public NoneType SetAttr(object obj, string name, object value)
        {
            PyTypeObject objtype;
            if (obj is PySuper super)
            {
                obj = super.first;
                objtype = super.second;
            }
            else
            {
                objtype = obj.GetPyType(this);
            }

            object setattr = FindNameInMro(objtype, "__setattr__");
            if (setattr != null)
            {
                Call(setattr, new object[] { obj, name, value }, null);
                return None;
            }

            object clsVar = FindNameInMro(objtype, name);
            if (clsVar != null)
            {
                // handle descriptor
                if (clsVar is PyProperty prop)
                {
                    if (prop.setter != None)
                    {
                        Call(prop.setter, new object[] { obj, value }, null);
                    }
                    else
                    {
                        TypeError("readonly attribute");
                    }
                    return None;
                }
            }
            // handle instance __dict__
            PyObject val = obj as PyObject;
            if (val == null) TypeError("cannot set attribute");
            val[name] = value;
            return None;
        }

        public object FindNameInMro(PyTypeObject cls, string name)
        {
            do
            {
                if (cls.attr.TryGetValue(name, out object val)) return val;
                object @base = cls.GetBaseType();
                if (@base == None) break;
                cls = @base as PyTypeObject;
            } while (true);
            return null;
        }

        public int NormalizedIndex(int index, int size)
        {
            if (index < 0) index += size;
            if (index < 0 || index >= size)
            {
                IndexError($"{index} not in [0, {size})");
            }
            return index;
        }

        public void ParseIntSlice(PySlice s, int length, out int start, out int stop, out int step)
        {
            static int clip(int value, int min, int max)
            {
                if (value < min) return min;
                if (value > max) return max;
                return value;
            }
            if (s.step == None) step = 1;
            else step = PyCast<int>(s.step);
            if (step == 0) ValueError("slice step cannot be zero");
            if (step > 0)
            {
                if (s.start == None)
                {
                    start = 0;
                }
                else
                {
                    start = PyCast<int>(s.start);
                    if (start < 0) start += length;
                    start = clip(start, 0, length);
                }
                if (s.stop == None)
                {
                    stop = length;
                }
                else
                {
                    stop = PyCast<int>(s.stop);
                    if (stop < 0) stop += length;
                    stop = clip(stop, 0, length);
                }
            }
            else
            {
                if (s.start == None)
                {
                    start = length - 1;
                }
                else
                {
                    start = PyCast<int>(s.start);
                    if (start < 0) start += length;
                    start = clip(start, -1, length - 1);
                }
                if (s.stop == None)
                {
                    stop = -1;
                }
                else
                {
                    stop = PyCast<int>(s.stop);
                    if (stop < 0) stop += length;
                    stop = clip(stop, -1, length - 1);
                }
            }
        }

        internal void BINARY_OP_SPECIAL_EX(Frame frame, string op, string name, string rname = null)
        {
            object _1 = frame.s.Pop();
            object _0 = frame.s.Top();
            object _2 = GetUnboundMethod(_0, name, out object self, false);
            if (_2 != null) frame.s.SetTop(CallMethod(self, _2, _1));
            else frame.s.SetTop(NotImplemented);
            if (frame.s.Top() != NotImplemented) return;
            if (rname != null)
            {
                _2 = GetUnboundMethod(_1, rname, out self, false);
                if (_2 != null) frame.s.SetTop(CallMethod(self, _2, _0));
                else frame.s.SetTop(NotImplemented);
            }
            if (frame.s.Top() == NotImplemented)
            {
                Error("TypeError", "unsupported operand type(s) for " + op);
            }
        }

        internal PyDict PopUnpackAsDict(ValueStack s, int n)
        {
            PyDict d = new PyDict();
            object[] args = s.PopNReversed(n);
            foreach (var item in args)
            {
                if (item is PyStarWrapper w)
                {
                    if (w.level != 2) TypeError("expected level 2 star wrapper");
                    PyDict other = PyCast<PyDict>(w.obj);
                    foreach (var item2 in other) d[item2.Key] = item2.Value;
                }
                else
                {
                    object[] t = PyCast<object[]>(item);
                    if (t.Length != 2) TypeError("expected tuple of length 2");
                    d[new PyDictKey(this, t[0])] = t[1];
                }
            }
            return d;
        }

        internal static string I2N(int index) => I2N(new StrName(index));

        internal static string I2N(StrName name)
        {
            if (CodeObject.nameMapping.TryGetValue(name, out var val))
            {
                return val;
            }
            throw new InternalException($"{name.index} does not exist in CodeObject.nameMapping");
        }

        internal List<object> PopUnpackAsList(ValueStack s, int n)
        {
            object[] tuple = s.PopNReversed(n);
            List<object> list = new List<object>();
            for (int i = 0; i < n; i++)
            {
                if (!(tuple[i] is PyStarWrapper wrapper)) list.Add(tuple[i]);
                else
                {
                    if (wrapper.level != 1) TypeError("expected level 1 star wrapper");
                    list.AddRange(PyList(wrapper.obj));
                }
            }
            return list;
        }
    }
}