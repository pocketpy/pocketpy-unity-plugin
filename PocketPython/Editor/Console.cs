using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace PocketPython
{
    public class EditorVMAttribute : Attribute
    {
    }

    public class Console : EditorWindow
    {
        public static Console instance { get; private set; }

        [MenuItem("Window/Python Console")]
        static void Init()
        {
            Console window = (Console)EditorWindow.GetWindow(typeof(Console));
            window.titleContent = new GUIContent("Python Console");
            window.Show();
        }

        VM vm;
        string input = "";
        Font font;

        struct HistoryItem
        {
            public int type;        // 0: input, 1: output, 2: error
            public string text;

            public HistoryItem(string text, int type)
            {
                this.text = text;
                this.type = type;
            }
        }
        List<HistoryItem> history = new List<HistoryItem>();

        static void ConsoleStdoutHandler(string message)
        {
            // remove \n as last if present
            if (message.Length > 0 && message[message.Length - 1] == '\n')
                message = message.Substring(0, message.Length - 1);
            Console.instance.history.Add(new HistoryItem(message, 1));
        }

        static void ConsoleStderrHandler(string message)
        {
            // remove \n as last if present
            if (message.Length > 0 && message[message.Length - 1] == '\n')
                message = message.Substring(0, message.Length - 1);
            Console.instance.history.Add(new HistoryItem(message, 2));
        }

        void Awake()
        {
            if (instance != null) throw new Exception("Only one Console allowed");
            instance = this;
            font = Resources.Load("PocketPython/SourceCodePro-Regular") as Font;
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (Type t in asm.GetTypes())
            {
                if (t.GetCustomAttributes(typeof(EditorVMAttribute), false).Length > 0)
                {
                    Utils.Assert(t.IsSubclassOf(typeof(VM)), "[EditorVM] decorated class must be a subclass of VM");
                    vm = Activator.CreateInstance(t) as VM;
                    Utils.Assert(vm != null, "Failed to create instance of VM subclass");
                    break;
                }
            }
            if (vm == null)
            {
                vm = new VM();
            }
            vm.stdout = ConsoleStdoutHandler;
            vm.stderr = ConsoleStderrHandler;
        }

        void SendInput()
        {
            input = input.Trim();
            history.Add(new HistoryItem(input, 0));
            vm.Exec(input, "<cell>", CompileMode.CELL_MODE, null);
            input = "";
            scrollPos.y = Mathf.Infinity;
        }

        Vector2 scrollPos = Vector2.zero;

        void OnGUI()
        {
            GUI.skin.font = font;
            const int fontSize = 14;
            var helpBoxStyle = GUI.skin.GetStyle("HelpBox");
            helpBoxStyle.fontSize = fontSize;
            var textAreaStyle = GUI.skin.GetStyle("TextArea");
            textAreaStyle.padding = new RectOffset(4, 4, 2, 4);
            textAreaStyle.fontSize = fontSize;
            var buttonStyle = GUI.skin.GetStyle("Button");
            buttonStyle.fontSize = fontSize;
            var labelStyle = GUI.skin.GetStyle("Label");
            labelStyle.fontSize = fontSize;
            labelStyle.wordWrap = true;
            labelStyle.alignment = TextAnchor.UpperLeft;

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            foreach (HistoryItem s in history)
            {
                switch (s.type)
                {
                    case 0:
                        GUILayout.Space(4);
                        EditorGUILayout.HelpBox(s.text, MessageType.None);
                        break;
                    case 1:
                        EditorGUILayout.LabelField(s.text, labelStyle);
                        break;
                    case 2:
                        EditorGUILayout.HelpBox(s.text, MessageType.Error);
                        break;
                }
            }
            GUILayout.EndScrollView();

            GUI.SetNextControlName("input");
            input = EditorGUILayout.TextArea(input, textAreaStyle);
            GUILayout.Space(2);
            if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.Return)
            {
                Event.current.Use();
                SendInput();
            }
            if (GUILayout.Button("Run (CTRL+ENTER)")) SendInput();

            EditorGUI.FocusTextInControl("input");
        }
    }
}
