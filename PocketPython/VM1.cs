using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PocketPython
{
    public partial class VM
    {
        public object RunTopFrame()
        {
            Frame frame = callStack.Peek();
            CodeObject co = frame.co;
            ValueStack s = frame.s;
            while (true)
            {
                Bytecode bc = frame.NextBytecode();
                if (debug)
                {
                    string name;
                    if (co.filename == co.name) name = co.filename;
                    else name = $"{co.filename}.{co.name}";
                    string line = frame.GetCurrentLine(out int lineno);
                    line = $"{lineno}: {line}";
                    Debug.Log(((Opcode)bc.op).ToString() + $" ({name})\n" + line + '\n' + frame.s.ToString());
                }
                switch ((Opcode)bc.op)
                {
                    case Opcode.NO_OP: break;
                    /*****************************************/
                    case Opcode.POP_TOP: s.Pop(); break;
                    case Opcode.DUP_TOP: s.Push(s.Top()); break;
                    case Opcode.ROT_TWO:
                        {
                            object _0 = s.Top();
                            s.SetTop(s.Second());
                            s.SetSecond(_0);
                            break;
                        }
                    case Opcode.ROT_THREE:
                        {
                            object _0 = s.Top();
                            s.SetTop(s.Second());
                            s.SetSecond(s.Third());
                            s.SetThird(_0);
                            break;
                        }
                    case Opcode.PRINT_EXPR:
                        {
                            if (s.Top() != None) stdout(PyRepr(s.Top()) + "\n");
                            s.Pop();
                            break;
                        }
                    /*****************************************/
                    case Opcode.LOAD_CONST: s.Push(co.consts[bc.arg]); break;
                    case Opcode.LOAD_NONE: s.Push(None); break;
                    case Opcode.LOAD_TRUE: s.Push(true); break;
                    case Opcode.LOAD_FALSE: s.Push(false); break;
                    case Opcode.LOAD_INTEGER: s.Push(bc.arg); break;
                    case Opcode.LOAD_ELLIPSIS: s.Push(Ellipsis); break;
                    case Opcode.LOAD_FUNCTION:
                        {
                            FuncDecl decl = frame.co.funcDecls[bc.arg];
                            PyFunction func = new PyFunction(decl, frame.module);
                            s.Push(func);
                        }
                        break;
                    case Opcode.LOAD_NULL: s.Push(null); break;
                    /*****************************************/
                    case Opcode.LOAD_FAST:
                        {
                            string name = I2N(co.varnames[bc.arg]);
                            if (frame.locals.TryGetValue(name, out object value))
                            {
                                s.Push(value);
                                break;
                            }
                            NameError(name);
                            break;
                        }
                    case Opcode.LOAD_NAME:
                        {
                            // search locals
                            string name = I2N(bc.arg);
                            if (frame.locals.TryGetValue(name, out object value))
                            {
                                s.Push(value);
                                break;
                            }
                            // search globals
                            if (frame.globals.TryGetValue(name, out value))
                            {
                                s.Push(value);
                                break;
                            }
                            // search builtins
                            if (builtins.attr.TryGetValue(name, out value))
                            {
                                s.Push(value);
                                break;
                            }
                            NameError(name);
                        }
                        break;
                    case Opcode.LOAD_NONLOCAL:
                        NotImplementedOpcode(bc.op);
                        break;
                    case Opcode.LOAD_GLOBAL:
                        {
                            string name = I2N(bc.arg);
                            if (frame.globals.TryGetValue(name, out object value))
                            {
                                s.Push(value);
                                break;
                            }
                            if (builtins.attr.TryGetValue(name, out value))
                            {
                                s.Push(value);
                                break;
                            }
                            NameError(name);
                            break;
                        }
                    case Opcode.LOAD_ATTR:
                        {
                            s.SetTop(GetAttr(s.Top(), I2N(bc.arg)));
                            break;
                        }
                    case Opcode.LOAD_METHOD:
                        {
                            object self;
                            s.SetTop(GetUnboundMethod(s.Top(), I2N(bc.arg), out self, false, true));
                            s.Push(self);
                            break;
                        }
                    case Opcode.LOAD_SUBSCR:
                        {
                            object _1 = s.Pop();
                            object _0 = s.Top();
                            s.SetTop(CallMethod(_0, "__getitem__", _1));
                            break;
                        }
                    case Opcode.STORE_FAST:
                        {
                            string name = I2N(co.varnames[bc.arg]);
                            frame.locals[name] = s.Pop();
                            break;
                        }
                    case Opcode.STORE_NAME:
                        NotImplementedOpcode(bc.op);
                        break;
                    case Opcode.STORE_GLOBAL:
                        {
                            string name = I2N(bc.arg);
                            frame.globals[name] = s.Pop();
                            break;
                        }
                    case Opcode.STORE_ATTR:
                        {
                            object _0 = s.Top();
                            object _1 = s.Second();
                            SetAttr(_0, I2N(bc.arg), _1);
                            s.Shrink(2);
                            break;
                        }
                    case Opcode.STORE_SUBSCR:
                        {
                            object _2 = s.Pop();
                            object _1 = s.Pop();
                            object _0 = s.Pop();
                            CallMethod(_1, "__setitem__", _2, _0);
                            break;
                        }
                    case Opcode.DELETE_FAST:
                        {
                            string name = I2N(co.varnames[bc.arg]);
                            bool ok = frame.locals.Remove(name);
                            if (!ok) NameError(name);
                            break;
                        }
                    case Opcode.DELETE_NAME:
                        NotImplementedOpcode(bc.op);
                        break;
                    case Opcode.DELETE_GLOBAL:
                        {
                            string name = I2N(bc.arg);
                            bool ok = frame.globals.Remove(name);
                            if (!ok) NameError(name);
                            break;
                        }
                    case Opcode.DELETE_ATTR:
                        NotImplementedOpcode(bc.op);
                        break;
                    case Opcode.DELETE_SUBSCR:
                        {
                            object _1 = s.Pop();
                            object _0 = s.Pop();
                            CallMethod(_0, "__delitem__", _1);
                            break;
                        }
                    /*****************************************/
                    case Opcode.BUILD_LONG:
                        NotImplementedOpcode(bc.op);
                        break;
                    case Opcode.BUILD_BYTES:
                        NotImplementedOpcode(bc.op);
                        break;
                    case Opcode.BUILD_TUPLE:
                        {
                            object[] items = s.PopNReversed(bc.arg);
                            s.Push(items);
                            break;
                        }
                    case Opcode.BUILD_LIST:
                        {
                            object[] items = s.PopNReversed(bc.arg);
                            s.Push(new List<object>(items));
                            break;
                        }
                    case Opcode.BUILD_DICT:
                        {
                            object[] items = s.PopNReversed(bc.arg);
                            s.Push(Call(builtins["dict"], new object[] { items }, null));
                            break;
                        }
                    case Opcode.BUILD_SET:
                        {
                            object[] items = s.PopNReversed(bc.arg);
                            s.Push(Call(builtins["set"], new object[] { items }, null));
                            break;
                        }
                    case Opcode.BUILD_SLICE:
                        {
                            object _2 = s.Pop();        // step
                            object _1 = s.Pop();        // stop
                            object _0 = s.Pop();        // start
                            s.Push(new PySlice(_0, _1, _2));
                            break;
                        }
                    case Opcode.BUILD_STRING:
                        {
                            object[] items = s.PopNReversed(bc.arg);
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < items.Length; i++) sb.Append(items[i]);
                            s.Push(sb.ToString());
                            break;
                        }
                    /*****************************************/
                    case Opcode.BUILD_TUPLE_UNPACK:
                        s.Push(PopUnpackAsList(s, bc.arg).ToArray());
                        break;
                    case Opcode.BUILD_LIST_UNPACK:
                        s.Push(PopUnpackAsList(s, bc.arg));
                        break;
                    case Opcode.BUILD_DICT_UNPACK:
                        s.Push(PopUnpackAsDict(s, bc.arg));
                        break;
                    case Opcode.BUILD_SET_UNPACK:
                        {
                            List<object> items = PopUnpackAsList(s, bc.arg);
                            s.Push(Call(builtins["set"], new object[] { items }, null));
                            break;
                        }
                    /*****************************************/
                    case Opcode.BINARY_TRUEDIV:
                        BINARY_OP_SPECIAL_EX(frame, "/", "__truediv__");
                        break;
                    case Opcode.BINARY_POW:
                        BINARY_OP_SPECIAL_EX(frame, "**", "__pow__");
                        break;
                    case Opcode.BINARY_ADD:
                        BINARY_OP_SPECIAL_EX(frame, "+", "__add__", "__radd__");
                        break;
                    case Opcode.BINARY_SUB:
                        BINARY_OP_SPECIAL_EX(frame, "-", "__sub__", "__rsub__");
                        break;
                    case Opcode.BINARY_MUL:
                        BINARY_OP_SPECIAL_EX(frame, "*", "__mul__", "__rmul__");
                        break;
                    case Opcode.BINARY_FLOORDIV:
                        BINARY_OP_SPECIAL_EX(frame, "//", "__floordiv__");
                        break;
                    case Opcode.BINARY_MOD:
                        BINARY_OP_SPECIAL_EX(frame, "%", "__mod__");
                        break;
                    case Opcode.COMPARE_LT:
                        BINARY_OP_SPECIAL_EX(frame, "<", "__lt__", "__gt__");
                        break;
                    case Opcode.COMPARE_LE:
                        BINARY_OP_SPECIAL_EX(frame, "<=", "__le__", "__ge__");
                        break;
                    case Opcode.COMPARE_EQ:
                        {
                            object _1 = s.Pop();
                            object _0 = s.Top();
                            s.SetTop(PyEquals(_0, _1));
                            break;
                        }
                    case Opcode.COMPARE_NE:
                        {
                            object _1 = s.Pop();
                            object _0 = s.Top();
                            s.SetTop(!PyEquals(_0, _1));
                            break;
                        }
                    case Opcode.COMPARE_GT:
                        BINARY_OP_SPECIAL_EX(frame, ">", "__gt__", "__lt__");
                        break;
                    case Opcode.COMPARE_GE:
                        BINARY_OP_SPECIAL_EX(frame, ">=", "__ge__", "__le__");
                        break;
                    case Opcode.BITWISE_LSHIFT:
                        BINARY_OP_SPECIAL_EX(frame, "<<", "__lshift__");
                        break;
                    case Opcode.BITWISE_RSHIFT:
                        BINARY_OP_SPECIAL_EX(frame, ">>", "__rshift__");
                        break;
                    case Opcode.BITWISE_AND:
                        BINARY_OP_SPECIAL_EX(frame, "&", "__and__");
                        break;
                    case Opcode.BITWISE_OR:
                        BINARY_OP_SPECIAL_EX(frame, "|", "__or__");
                        break;
                    case Opcode.BITWISE_XOR:
                        BINARY_OP_SPECIAL_EX(frame, "^", "__xor__");
                        break;
                    case Opcode.BINARY_MATMUL:
                        BINARY_OP_SPECIAL_EX(frame, "@", null, "__matmul__");
                        break;
                    case Opcode.IS_OP:
                        {
                            object _1 = s.Pop();
                            object _0 = s.Top();
                            s.SetTop(((_0 == _1)) ^ (bc.arg != 0));
                            break;
                        }
                    case Opcode.CONTAINS_OP:
                        {
                            object _0 = CallMethod(s.Top(), "__contains__", s.Second());
                            s.Pop();
                            s.SetTop(PyBool(_0) ^ (bc.arg != 0));
                            break;
                        }
                    /*****************************************/
                    case Opcode.JUMP_ABSOLUTE:
                        frame.JumpAbs(bc.arg);
                        break;
                    case Opcode.POP_JUMP_IF_FALSE:
                        if (!PyBool(s.Pop())) frame.JumpAbs(bc.arg);
                        break;
                    case Opcode.JUMP_IF_TRUE_OR_POP:
                        if (PyBool(s.Top()) == true) frame.JumpAbs(bc.arg);
                        else s.Pop();
                        break;
                    case Opcode.JUMP_IF_FALSE_OR_POP:
                        if (PyBool(s.Top()) == false) frame.JumpAbs(bc.arg);
                        else s.Pop();
                        break;
                    case Opcode.SHORTCUT_IF_FALSE_OR_POP:
                        if (PyBool(s.Top()) == false)
                        {
                            s.Shrink(2);
                            s.Push(false);
                            frame.JumpAbs(bc.arg);
                        }
                        else s.Pop();
                        break;
                    case Opcode.LOOP_CONTINUE:
                        frame.JumpAbs(co.blocks[bc.block].start);
                        break;
                    case Opcode.LOOP_BREAK:
                        frame.JumpAbsBreak(co.blocks[bc.block].end);
                        break;
                    case Opcode.GOTO:
                        {
                            if (!co.labels.ContainsKey(new StrName(bc.arg))) KeyError(I2N(bc.arg));
                            int index = co.labels[new StrName(bc.arg)];
                            frame.JumpAbsBreak(index);
                            break;
                        }
                    /*****************************************/
                    case Opcode.EVAL:{
                            s.SetTop(Call(builtins["eval"], new object[] { s.Pop() }, null));
                            break;
                        }
                    case Opcode.CALL:
                        {
                            int argc = bc.arg & 0xffff;
                            int kwargc = (bc.arg >> 16) & 0xffff;
                            object[] kwargs = s.PopNReversed(kwargc * 2);
                            object[] args;
                            if (s.Peek(argc + 1) != null)
                            {
                                args = s.PopNReversed(argc + 1);
                            }
                            else
                            {
                                args = s.PopNReversed(argc);
                                s.Pop();    // pop null
                            }
                            object callable = s.Pop();
                            if (kwargc == 0)
                            {
                                s.Push(Call(callable, args, null));
                            }
                            else
                            {
                                Dictionary<string, object> kw = new Dictionary<string, object>();
                                for (int i = 0; i < kwargs.Length; i += 2)
                                {
                                    int index = PyCast<int>(kwargs[i]);
                                    kw[I2N(index)] = kwargs[i + 1];
                                }
                                s.Push(Call(callable, args, kw));
                            }
                            break;
                        }
                    case Opcode.CALL_TP:
                        {
                            PyDict kwargs = null;
                            if (bc.arg == 1)
                            {
                                kwargs = s.Pop() as PyDict;
                            }
                            object[] args = s.Pop() as object[];
                            object self = s.Pop();  // may be null
                            if (self != null) args = args.Prepend(self);
                            object callable = s.Pop();
                            Dictionary<string, object> kw = null;
                            if (kwargs != null)
                            {
                                kw = new Dictionary<string, object>();
                                foreach (var kv in kwargs) kw[(string)kv.Key.obj] = kv.Value;
                            }
                            s.Push(Call(callable, args, kw));
                            break;
                        }
                    case Opcode.RETURN_VALUE:
                        {
                            object res = s.Pop();
                            callStack.Pop();
                            return res;
                        }
                    case Opcode.YIELD_VALUE:
                        NotImplementedOpcode(bc.op);
                        break;
                    /*****************************************/
                    case Opcode.LIST_APPEND:
                        {
                            object _0 = s.Pop();
                            PyCast<List<object>>(s.Second()).Add(_0);
                            break;
                        }
                    case Opcode.DICT_ADD:
                        {
                            object _0 = s.Pop();
                            object[] t = PyCast<object[]>(_0);
                            CallMethod(s.Second(), "__setitem__", t[0], t[1]);
                            break;
                        }
                    case Opcode.SET_ADD:
                        {
                            object _0 = s.Pop();
                            CallMethod(s.Second(), "add", _0);
                            break;
                        }
                    /*****************************************/
                    case Opcode.UNARY_NEGATIVE:
                        s.SetTop(CallMethod(s.Top(), "__neg__"));
                        break;
                    case Opcode.UNARY_NOT:
                        s.SetTop(!PyBool(s.Top()));
                        break;
                    case Opcode.UNARY_STAR:
                        s.SetTop(new PyStarWrapper(s.Top(), bc.arg));
                        break;
                    case Opcode.UNARY_INVERT:
                        s.SetTop(CallMethod(s.Top(), "__invert__"));
                        break;
                    /*****************************************/
                    case Opcode.GET_ITER:
                        s.SetTop(PyIter(s.Top()));
                        break;
                    case Opcode.FOR_ITER:
                        {
                            object _0 = PyNext(s.Top());
                            if (_0 != StopIteration)
                            {
                                s.Push(_0);
                            }
                            else
                            {
                                frame.JumpAbsBreak(co.blocks[bc.block].end);
                            }
                            break;
                        }
                    /*****************************************/
                    case Opcode.IMPORT_NAME:
                        s.Push(PyImport(I2N(bc.arg)));
                        break;
                    case Opcode.IMPORT_NAME_REL:
                        NotImplementedOpcode(bc.op);
                        break;
                    case Opcode.IMPORT_STAR:
                        {
                            PyModule _0 = s.Pop() as PyModule;
                            foreach (var kv in _0.attr)
                            {
                                string name = kv.Key;
                                object value = kv.Value;
                                if (name.Length == 0 || name[0] == '_') continue;
                                frame.globals[name] = value;
                            }
                            break;
                        }
                    /*****************************************/
                    case Opcode.UNPACK_SEQUENCE:
                        {
                            List<object> _0 = PyList(s.Pop());
                            if (bc.arg > _0.Count) ValueError("not enough values to unpack");
                            for (int i = 0; i < bc.arg; i++) s.Push(_0[i]);
                            if (bc.arg < _0.Count) ValueError("too many values to unpack");
                            break;
                        }
                    case Opcode.UNPACK_EX:
                        {
                            List<object> _0 = PyList(s.Pop());
                            if (bc.arg > _0.Count) ValueError("not enough values to unpack");
                            for (int i = 0; i < bc.arg; i++) s.Push(_0[i]);
                            List<object> rest = new List<object>();
                            for (int i = bc.arg; i < _0.Count; i++) rest.Add(_0[i]);
                            s.Push(rest);
                            break;
                        }
                    /*****************************************/
                    case Opcode.BEGIN_CLASS:
                        {
                            string name = I2N(bc.arg);
                            object _0 = s.Pop();        // super
                            if (_0 == None) _0 = typeof(object).GetPyType(this);
                            // check_non_tagged_type(_0, tp_type);
                            object _1 = NewTypeObject(frame.module, name, _0 as PyTypeObject);
                            s.Push(_1);
                            break;
                        }
                    case Opcode.END_CLASS:
                        s.Pop();
                        break;
                    case Opcode.STORE_CLASS_ATTR:
                        {
                            string name = I2N(bc.arg);
                            object _0 = s.Pop();
                            (s.Top() as PyObject)[name] = _0;
                            break;
                        }
                    /*****************************************/
                    case Opcode.WITH_ENTER:
                        NotImplementedOpcode(bc.op);
                        break;
                    case Opcode.WITH_EXIT:
                        NotImplementedOpcode(bc.op);
                        break;
                    /*****************************************/
                    case Opcode.ASSERT:
                        {
                            object _0 = s.Pop();
                            string msg = "";
                            object[] t = _0 as object[];
                            if (t != null)
                            {
                                if (t.Length != 2) ValueError("assertion must be a tuple of length 2");
                                _0 = t[0];
                                msg = PyCast<string>(t[1]);
                            }
                            if (!PyBool(_0)) Error("AssertionError", msg);
                            break;
                        }
                    case Opcode.EXCEPTION_MATCH:
                        {
                            throw new System.NotImplementedException();
                        }
                    case Opcode.RAISE:
                        {
                            throw new System.NotImplementedException();
                        }
                    case Opcode.RE_RAISE:
                        {
                            throw new System.NotImplementedException();
                        }
                    case Opcode.POP_EXCEPTION:
                        {
                            throw new System.NotImplementedException();
                        }
                    /*****************************************/
                    case Opcode.FORMAT_STRING:
                        {
                            object _0 = s.Pop();
                            string spec = PyCast<string>(co.consts[bc.arg]);
                            // spec is not implemented
                            s.Push(PyStr(_0));
                            break;
                        }
                    /*****************************************/
                    case Opcode.INC_FAST:
                        {
                            string name = I2N(co.varnames[bc.arg]);
                            if (frame.locals.TryGetValue(name, out object value))
                            {
                                frame.locals[name] = (int)value + 1;
                            }
                            else
                            {
                                NameError(name);
                            }
                            break;
                        }
                    case Opcode.DEC_FAST:
                        {
                            string name = I2N(co.varnames[bc.arg]);
                            if (frame.locals.TryGetValue(name, out object value))
                            {
                                frame.locals[name] = (int)value - 1;
                            }
                            else
                            {
                                NameError(name);
                            }
                            break;
                        }
                    case Opcode.INC_GLOBAL:
                        {
                            string name = I2N(bc.arg);
                            if (frame.globals.TryGetValue(name, out object value))
                            {
                                frame.globals[name] = (int)value + 1;
                            }
                            else
                            {
                                NameError(name);
                            }
                            break;
                        }
                    case Opcode.DEC_GLOBAL:
                        {
                            string name = I2N(bc.arg);
                            if (frame.globals.TryGetValue(name, out object value))
                            {
                                frame.globals[name] = (int)value - 1;
                            }
                            else
                            {
                                NameError(name);
                            }
                            break;
                        }
                    default: NotImplementedOpcode(bc.op); break;
                }
            }
        }
    }
}