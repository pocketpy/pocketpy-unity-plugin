using System;
using System.Text;

namespace PocketPython
{
    public static class TupleExtension
    {
        public static object[] Prepend(this object[] b, object a)
        {
            object[] result = new object[b.Length + 1];
            result[0] = a;
            for (int i = 0; i < b.Length; i++) result[i + 1] = b[i];
            return result;
        }

        public static object[] SubArray(this object[] b, int start){
            object[] result = new object[b.Length - start];
            for (int i = start; i < b.Length; i++) result[i - start] = b[i];
            return result;
        }
    }

    public static class StringExtension
    {
        public static string Escape(this String str, bool singleQuote = true)
        {
            var ss = new StringBuilder();
            ss.Append(singleQuote ? '\'' : '\"');

            foreach (char c in str)
            {
                switch (c)
                {
                    case '\"':
                        if (!singleQuote) ss.Append('\\');
                        ss.Append('\"');
                        break;
                    case '\'':
                        if (singleQuote) ss.Append('\\');
                        ss.Append('\'');
                        break;
                    case '\\': ss.Append("\\\\"); break;
                    case '\n': ss.Append("\\n"); break;
                    case '\r': ss.Append("\\r"); break;
                    case '\t': ss.Append("\\t"); break;
                    default:
                        if ('\x00' <= c && c <= '\x1f')
                        {
                            ss.Append("\\x" + ((int)c).ToString("x2"));
                        }
                        else
                        {
                            ss.Append(c);
                        }
                        break;
                }
            }

            ss.Append(singleQuote ? '\'' : '\"');
            return ss.ToString();
        }

        public static string Unescape(this String str)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length;)
            {
                if (str[i] == '\\')
                {
                    i++;
                    if (i == str.Length) throw new ArgumentException("Invalid escape sequence at end of string.");
                    switch (str[i])
                    {
                        case 'x':
                            if (i + 2 >= str.Length)
                                throw new ArgumentException($"Invalid escape sequence at position {i}");
                            sb.Append((char)Int32.Parse(str.Substring(i + 1, 2), System.Globalization.NumberStyles.HexNumber));
                            i += 3;
                            break;
                        case 'n': sb.Append('\n'); i++; break;
                        case 'r': sb.Append('\r'); i++; break;
                        case 't': sb.Append('\t'); i++; break;
                        case '\"': sb.Append('\"'); i++; break;
                        case '\'': sb.Append('\''); i++; break;
                        case '\\': sb.Append('\\'); i++; break;
                        default: throw new ArgumentException($"Invalid escape sequence at position {i}");
                    }
                }
                else
                {
                    sb.Append(str[i]);
                    i++;
                }
            }

            return sb.ToString();
        }
    }

    public static class ObjectExtension
    {
        public static PyTypeObject GetPyType(this object self, VM vm)
        {
            Utils.Assert(!(self is Type));
            if (self is PyTypeObject)
            {
                return typeof(Type).GetPyType(vm);
            }
            if (self is PyDynamic t)
            {
                return t.type;
            }
            return self.GetType().GetPyType(vm);
        }

        public static PyTypeObject GetPyType(this Type self, VM vm)
        {
            if (vm.allTypes.TryGetValue(self, out PyTypeObject type))
            {
                return type;
            }
            else
            {
                throw new InternalException($"Type {self} is not registered. Use vm.RegisterType to register it first.");
            }
        }
    }

}