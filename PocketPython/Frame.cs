using System.Collections.Generic;

namespace PocketPython
{
    public class Frame
    {
        public int ip = -1;
        public int nextIp = 0;
        public CodeObject co;
        public Dictionary<string, object> locals = new Dictionary<string, object>();
        public Dictionary<string, object> globals { get { return module.attr; } }
        public ValueStack s = new ValueStack();

        public PyModule module;

        public Frame(CodeObject co, PyModule module)
        {
            this.co = co;
            this.module = module;
        }

        public Bytecode NextBytecode()
        {
            ip = nextIp;
            nextIp += 1;
            return co.codes[ip];
        }

        public void JumpAbs(int arg)
        {
            nextIp = arg;
        }

        public string GetCurrentLine(out int lineno)
        {
            int safeIp = ip;
            if (safeIp < 0) safeIp = 0;
            lineno = co.lines[safeIp];
            string line = co.GetLine(safeIp);
            return line;
        }

        private int ExitBlock(int i)
        {
            if (co.blocks[i].type == CodeBlockType.FOR_LOOP) s.Pop();
            return co.blocks[i].parent;
        }

        public void JumpAbsBreak(int target)
        {
            Bytecode prev = co.codes[ip];
            int i = prev.block;
            nextIp = target;
            if (nextIp >= co.codes.Count)
            {
                while (i >= 0) i = ExitBlock(i);
            }
            else
            {
                Bytecode next = co.codes[target];
                while (i >= 0 && next.block != i)
                {
                    i = ExitBlock(i);
                }
                Utils.Assert(i == next.block);
            }
        }
    }

}