using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Text;

namespace PocketPython
{
    public enum CompileMode
    {
        EXEC_MODE,
        EVAL_MODE,
        REPL_MODE,
        JSON_MODE,
        CELL_MODE
    }

    public class CodeObjectDeserializer
    {
        public string[] tokens;
        public int pos;
        public string source;

        string current => tokens[pos];

        public CodeObjectDeserializer(string buffer, string source)
        {
            this.source = source;
            this.tokens = buffer.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            this.pos = 0;
        }

        void AssertHeader(char h)
        {
            if (current[0] != h)
            {
                throw new InternalException("Expected header '" + h + "' but got '" + current[0] + "'");
            }
        }

        public void Advance()
        {
            // Debug.Log(current);
            pos++;
        }

        public string ReadStr()
        {
            // example s"ab\n123c"
            AssertHeader('s');
            string x = current.Substring(2, current.Length - 3);
            x = x.Unescape();
            Advance();
            return x;
        }

        public int ReadInt()
        {
            // example i123
            AssertHeader('i');
            int x = int.Parse(current.Substring(1));
            Advance();
            return x;
        }

        public StrName ReadName()
        {
            // example n123
            AssertHeader('n');
            int x = int.Parse(current.Substring(1));
            Advance();
            return new StrName(x);
        }

        public object ReadObject()
        {
            switch (current[0])
            {
                case 's': return ReadStr();
                case 'i': return ReadInt();
                case 'n': return ReadName();
                case 'f': return ReadFloat();
                case 'b': return ReadBool();
                case 'x': return ReadBytes();
                case 'N': Advance(); return VM.None;
                case 'E': Advance(); return VM.Ellipsis;
                default: throw new InternalException("Unknown object type: " + current[0]);
            }
        }

        public float ReadFloat()
        {
            // example f123.456
            AssertHeader('f');
            float x = float.Parse(current.Substring(1));
            Advance();
            return x;
        }

        public bool ReadBool()
        {
            // example b1 or b0
            AssertHeader('b');
            bool x = current[1] == '1';
            Advance();
            return x;
        }

        public byte[] ReadBytes()
        {
            // example x1234567890abcdef
            AssertHeader('x');
            string x = current.Substring(1);
            byte[] bytes = new byte[x.Length / 2];
            for (int i = 0; i < x.Length; i += 2)
            {
                bytes[i / 2] = byte.Parse(x.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            Advance();
            return bytes;
        }

        public T ReadStruct<T>()
        {
            // https://stackoverflow.com/questions/6335153/casting-a-byte-array-to-a-managed-structure
            byte[] bytes = ReadBytes();
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return stuff;
        }

        public void VerifyVersion()
        {
            string ver = ReadStr();
            Utils.Assert(ver == Version.Frontend, $"Version mismatch: {ver} != {Version.Frontend}");
        }

        public void ConsumeBeginMark()
        {
            AssertHeader('[');
            Advance();
        }

        public void ConsumeEndMark()
        {
            AssertHeader(']');
            Advance();
        }

        public void ConsumeLeftParen()
        {
            AssertHeader('(');
            Advance();
        }

        public void ConsumeRightParen()
        {
            AssertHeader(')');
            Advance();
        }

        public bool MatchEndMark()
        {
            bool ok = current == "]";
            if (ok) Advance();
            return ok;
        }

        public CodeObject ReadCode()
        {
            CodeObject co = new CodeObject();
            co.source = source;
            ConsumeLeftParen();

            ConsumeBeginMark();
            co.filename = ReadStr();
            co.mode = (CompileMode)ReadInt();
            ConsumeEndMark();

            co.name = ReadStr();
            co.isGenerator = ReadBool();

            ConsumeBeginMark();
            while (!MatchEndMark())
            {
                Bytecode bc = ReadStruct<Bytecode>();
                co.codes.Add(bc);
            }

            ConsumeBeginMark();
            while (!MatchEndMark())
            {
                int line = ReadInt();
                co.lines.Add(line);
            }

            Utils.Assert(co.lines.Count == co.codes.Count);

            ConsumeBeginMark();
            while (!MatchEndMark())
            {
                object o = ReadObject();
                co.consts.Add(o);
            }

            ConsumeBeginMark();
            while (!MatchEndMark())
            {
                StrName name = ReadName();
                co.varnames.Add(name);
            }

            ConsumeBeginMark();
            while (!MatchEndMark())
            {
                CodeBlock block = ReadStruct<CodeBlock>();
                co.blocks.Add(block);
            }

            ConsumeBeginMark();
            while (!MatchEndMark())
            {
                StrName name = ReadName();
                int pos = ReadInt();
                co.labels[name] = pos;
            }

            ConsumeBeginMark();
            while (!MatchEndMark())
            {
                FuncDecl decl = new FuncDecl();
                decl.code = ReadCode();
                ConsumeBeginMark();
                while (!MatchEndMark())
                {
                    int arg = ReadInt();
                    decl.args.Add(arg);
                }

                ConsumeBeginMark();
                while (!MatchEndMark())
                {
                    int key = ReadInt();
                    object value = ReadObject();
                    decl.kwargs.Add(new FuncDecl.KwArg(key, value));
                }

                decl.starredArg = ReadInt();
                decl.starredKwarg = ReadInt();
                decl.nested = ReadBool();

                co.funcDecls.Add(decl);
            }

            ConsumeRightParen();
            return co;
        }

        public CodeObject Deserialize()
        {
            VerifyVersion();
            CodeObject co = ReadCode();
            while (pos < tokens.Length)
            {
                StrName key = ReadName();
                string value = ReadStr();
                CodeObject.nameMapping[key] = value;
            }
            return co;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Bytecode
    {
        public ushort op;
        public ushort block;
        public int arg;
    }

    public struct StrName
    {
        public int index;

        public StrName(int index)
        {
            this.index = index;
        }
    }

    public enum CodeBlockType
    {
        NO_BLOCK,
        FOR_LOOP,
        WHILE_LOOP,
        CONTEXT_MANAGER,
        TRY_EXCEPT,
    };

    public class FuncDecl
    {
        public struct KwArg
        {
            public int key;
            public object value;

            public KwArg(int key, object value)
            {
                this.key = key;
                this.value = value;
            }
        };

        public CodeObject code;
        public List<int> args = new List<int>();
        public List<KwArg> kwargs = new List<KwArg>();
        public int starredArg;
        public int starredKwarg;
        public bool nested;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CodeBlock
    {
        public CodeBlockType type; // how many bytes???
        public int parent;
        public int for_loop_depth;
        public int start;
        public int end;
    }

    public class CodeObject
    {
        public static Dictionary<StrName, string> nameMapping = new Dictionary<StrName, string>();

        public string source;      // SourceData.source
        public string filename;    // SourceData.filename
        public CompileMode mode;   // SourceData.mode

        public string name;
        public bool isGenerator;

        public List<Bytecode> codes = new List<Bytecode>();
        public List<int> lines = new List<int>();
        public List<object> consts = new List<object>();
        public List<StrName> varnames = new List<StrName>();
        public List<CodeBlock> blocks = new List<CodeBlock>();
        public Dictionary<StrName, int> labels = new Dictionary<StrName, int>();
        public List<FuncDecl> funcDecls = new List<FuncDecl>();

        private string[] cachedLines;
        public string GetLine(int ip)
        {
            Utils.Assert(source != null);
            int i = lines[ip];
            if (cachedLines == null)
            {
                cachedLines = source.Split("\n".ToCharArray(), StringSplitOptions.None);
            }
            return cachedLines[i - 1];
        }

        public static CodeObject FromBytes(string buffer, string source)
        {
            CodeObjectDeserializer deserializer = new CodeObjectDeserializer(buffer, source);
            return deserializer.Deserialize();
        }

        public string Disassemble()
        {
            StringBuilder sb = new StringBuilder();
            int prevLine = -1;
            for (int i = 0; i < codes.Count; i++)
            {
                Bytecode bc = codes[i];
                string line = lines[i].ToString();
                if (lines[i] == prevLine)
                    line = "";
                else
                {
                    if (prevLine != -1) sb.Append("\n");
                    prevLine = lines[i];
                }

                string pointer = "   ";
                sb.Append(line.PadRight(8) + pointer + i.ToString().PadRight(3));
                string opName = ((Opcode)bc.op).ToString();
                sb.Append(" " + opName.PadRight(25) + " ");
                sb.Append(bc.arg.ToString());
                if (i != codes.Count - 1) sb.Append('\n');
            }

            foreach (FuncDecl decl in funcDecls)
            {
                sb.Append("\n\n" + "Disassembly of " + decl.code.name + ":\n");
                sb.Append(decl.code.Disassemble());
            }
            sb.Append("\n");
            return sb.ToString();
        }
    }

}