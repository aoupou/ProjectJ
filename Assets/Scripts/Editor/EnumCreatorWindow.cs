using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

// enum을 새로 만들거나 기존 enum에 값을 추가하는 창. 상단 메뉴 ProjectJ → 데이터베이스 → enum 만들기 · 값 추가
// 만든 enum은 새 데이터베이스 만들기의 형식 목록(enum/...)에 나온다
// 한글 이름을 적으면 [InspectorName]으로 들어가서 Inspector와 구글 시트에 한글로 보임
// 주의: 유니티는 enum을 순서 번호로 저장하므로 기존 enum에는 항상 맨 아래에만 추가한다
//       (중간에 끼우거나 순서를 바꾸면 이미 만든 데이터의 값이 다른 걸로 바뀜)
public class EnumCreatorWindow : EditorWindow
{
    [Serializable]
    private class Entry
    {
        public string name = "";
        public string label = "";
        public string comment = "";
    }

    private const string NewEnumFolder = "Assets/Scripts/Common/Enums";
    private static readonly string[] Modes = { "새 enum 만들기", "기존 enum에 값 추가" };
    private static readonly Regex Identifier = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

    [SerializeField] private int mode;
    [SerializeField] private string enumName = "";
    [SerializeField] private string description = "";
    [SerializeField] private List<Entry> entries = new List<Entry>();
    [SerializeField] private string targetEnum; // 값을 추가할 enum (FullName)

    private ReorderableList list;
    private Vector2 scroll;
    private Type[] enums = new Type[0];
    private string sourcePath; // targetEnum이 적힌 파일 (못 찾으면 null)

    [MenuItem("ProjectJ/데이터베이스/enum 만들기 · 값 추가", priority = 2)]
    public static void Open()
    {
        EnumCreatorWindow window = GetWindow<EnumCreatorWindow>("enum");
        window.minSize = new Vector2(520, 360);
    }

    private void OnEnable()
    {
        if (entries.Count == 0)
            entries.Add(new Entry());

        list = new ReorderableList(entries, typeof(Entry), true, true, true, true);
        list.drawHeaderCallback = DrawHeader;
        list.drawElementCallback = DrawEntry;
        list.onAddCallback = l => entries.Add(new Entry());

        RefreshEnums();
    }

    private void OnProjectChange()
    {
        RefreshEnums();
        Repaint();
    }

    private void RefreshEnums()
    {
        enums = DatabaseCreatorWindow.ProjectEnums().ToArray();

        if (enums.Length > 0 && Array.FindIndex(enums, t => t.FullName == targetEnum) < 0)
            targetEnum = enums[0].FullName;

        sourcePath = FindSource(TargetType);
    }

    private Type TargetType => enums.FirstOrDefault(t => t.FullName == targetEnum);

    private string NewEnumPath => $"{NewEnumFolder}/{enumName}.cs";

    // ───────── 화면 ─────────

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        int newMode = GUILayout.Toolbar(mode, Modes);

        if (newMode != mode)
        {
            mode = newMode;
            GUI.FocusControl(null);
        }

        EditorGUILayout.Space();

        if (mode == 0)
            DrawCreate();
        else
            DrawAppend();

        EditorGUILayout.EndScrollView();
    }

    private void DrawCreate()
    {
        enumName = EditorGUILayout.TextField(
            new GUIContent("enum 이름", "대문자로 시작하는 영문 (예: WeaponType)"), enumName).Trim();
        description = EditorGUILayout.TextField(
            new GUIContent("설명", "코드 맨 위에 주석으로 들어감 (예: 무기 종류)"), description);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("값", EditorStyles.boldLabel);
        list.DoLayoutList();

        if (enumName.Length > 0)
            EditorGUILayout.LabelField($"만들어지는 파일: {NewEnumPath}");

        List<string> errors = ValidateEntries(new HashSet<string>());

        if (enumName.Length == 0)
            errors.Insert(0, "enum 이름을 적어주세요 (예: WeaponType)");
        else if (!Identifier.IsMatch(enumName) || !char.IsUpper(enumName[0]))
            errors.Insert(0, "enum 이름은 대문자로 시작하고 영문 / 숫자만 쓸 수 있어요 (예: WeaponType)");
        else if (DatabaseCreatorWindow.TypeExists(enumName) || File.Exists(NewEnumPath))
            errors.Insert(0, $"'{enumName}'은(는) 이미 있는 이름이에요");

        if (ShowErrorsAndButton(errors, "만들기"))
            CreateEnum();
    }

    private void DrawAppend()
    {
        if (enums.Length == 0)
        {
            EditorGUILayout.HelpBox("프로젝트에 enum이 아직 없어요. 먼저 새 enum을 만들어주세요.", MessageType.Info);
            return;
        }

        int index = Array.FindIndex(enums, t => t.FullName == targetEnum);
        int picked = EditorGUILayout.Popup("enum", index, enums.Select(t => t.FullName.Replace('+', '.')).ToArray());

        if (picked != index)
        {
            targetEnum = enums[picked].FullName;
            sourcePath = FindSource(enums[picked]);
        }

        Type type = TargetType;
        EditorGUILayout.LabelField("파일", sourcePath ?? "못 찾음");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("지금 있는 값", EditorStyles.boldLabel);

        foreach (string name in Enum.GetNames(type))
        {
            string label = InspectorLabel(type, name);
            EditorGUILayout.LabelField("  " + name + (label != null ? $"  ({label})" : ""));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("추가할 값  (맨 아래에 추가됨)", EditorStyles.boldLabel);
        list.DoLayoutList();

        List<string> errors = ValidateEntries(new HashSet<string>(Enum.GetNames(type)));

        if (sourcePath == null)
            errors.Insert(0, "이 enum이 적힌 파일을 Assets 안에서 못 찾았어요");

        if (ShowErrorsAndButton(errors, "추가"))
            AppendEntries(type);
    }

    private static bool ShowErrorsAndButton(List<string> errors, string button)
    {
        foreach (string error in errors)
            EditorGUILayout.HelpBox(error, MessageType.Warning);

        using (new EditorGUI.DisabledScope(errors.Count > 0))
            return GUILayout.Button(button, GUILayout.Height(30));
    }

    private static Rect[] Columns(Rect rect)
    {
        float[] widths = { 0.3f, 0.3f, 0.4f };
        Rect[] rects = new Rect[widths.Length];
        float x = rect.x;

        for (int i = 0; i < widths.Length; i++)
        {
            float width = rect.width * widths[i];
            rects[i] = new Rect(x, rect.y + 1, width - 4, EditorGUIUtility.singleLineHeight);
            x += width;
        }

        return rects;
    }

    private void DrawHeader(Rect rect)
    {
        rect.xMin += 14; // 드래그 손잡이 자리만큼 맞춤
        Rect[] c = Columns(rect);
        EditorGUI.LabelField(c[0], "값 이름 (영문)");
        EditorGUI.LabelField(c[1], "한글 이름 (선택)");
        EditorGUI.LabelField(c[2], "메모 (선택)");
    }

    private void DrawEntry(Rect rect, int index, bool active, bool focused)
    {
        Entry entry = entries[index];
        Rect[] c = Columns(rect);
        entry.name = EditorGUI.TextField(c[0], entry.name).Trim();
        entry.label = EditorGUI.TextField(c[1], entry.label);
        entry.comment = EditorGUI.TextField(c[2], entry.comment);
    }

    // ───────── 검사 ─────────

    private List<string> ValidateEntries(HashSet<string> existing)
    {
        List<string> errors = new List<string>();

        if (entries.Count == 0)
            errors.Add("값을 하나 이상 추가해주세요");

        HashSet<string> names = new HashSet<string>(existing);

        foreach (Entry entry in entries)
        {
            if (entry.name.Length == 0)
            {
                errors.Add("값 이름이 비어 있는 줄이 있어요");
                continue;
            }

            if (!Identifier.IsMatch(entry.name))
                errors.Add($"'{entry.name}': 영문 / 숫자 / _만 쓸 수 있고 숫자로 시작하면 안 돼요");
            else if (DatabaseCreatorWindow.IsKeyword(entry.name))
                errors.Add($"'{entry.name}'은(는) C# 예약어라 쓸 수 없어요");
            else if (!names.Add(entry.name))
                errors.Add($"'{entry.name}'이(가) 이미 있어요");
        }

        return errors;
    }

    // 값의 [InspectorName] (없으면 null)
    private static string InspectorLabel(Type type, string name)
    {
        InspectorNameAttribute attribute = type.GetField(name)?
            .GetCustomAttributes(typeof(InspectorNameAttribute), false)
            .FirstOrDefault() as InspectorNameAttribute;
        return attribute?.displayName;
    }

    // ───────── 만들기 / 추가 ─────────

    private static string EntryLine(Entry entry, string indent)
    {
        string label = entry.label.Trim();
        string comment = entry.comment.Trim();
        string attribute = label.Length > 0 ? $"[InspectorName(\"{DatabaseCreatorWindow.Escape(label)}\")] " : "";
        return $"{indent}{attribute}{entry.name},{(comment.Length > 0 ? " // " + comment.Replace("\n", " ") : "")}";
    }

    private void CreateEnum()
    {
        Directory.CreateDirectory(NewEnumFolder);

        StringBuilder code = new StringBuilder();
        code.Append("using UnityEngine;\n\n");

        if (description.Trim().Length > 0)
            code.Append($"// {description.Trim().Replace("\n", " ")}\n");

        code.Append("// 값 추가: ProjectJ → 데이터베이스 → enum 만들기 · 값 추가\n");
        code.Append("// (순서 번호로 저장되니 항상 맨 아래에 추가. 중간에 끼우거나 순서를 바꾸면 기존 데이터 값이 바뀜)\n");
        code.Append($"public enum {enumName}\n{{\n");

        foreach (Entry entry in entries)
            code.Append(EntryLine(entry, "    ") + "\n");

        code.Append("}\n");
        File.WriteAllText(NewEnumPath, code.ToString());

        string message = $"{NewEnumPath}를 만들었어요.\n\n컴파일이 끝나면 새 데이터베이스 만들기의 형식 목록 enum/{enumName}에서 고를 수 있어요.";
        targetEnum = enumName;
        ResetEntries();

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("enum 만들기", message, "확인");
    }

    private void AppendEntries(Type type)
    {
        byte[] bytes = File.ReadAllBytes(sourcePath);
        bool bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        string text = File.ReadAllText(sourcePath);
        string newline = text.Contains("\r\n") ? "\r\n" : "\n";

        Match header = Regex.Match(text, $@"\benum\s+{Regex.Escape(type.Name)}\b[^{{;]*\{{");
        int bodyStart = header.Success ? header.Index + header.Length : -1;
        int close = bodyStart >= 0 ? FindClosingBrace(text, bodyStart) : -1;

        if (close < 0)
        {
            EditorUtility.DisplayDialog("값 추가", $"{sourcePath}에서 enum {type.Name}의 범위를 못 찾았어요. 직접 추가해주세요.", "확인");
            return;
        }

        // 마지막 값 찾기 (쉼표가 없으면 붙여야 함, 들여쓰기도 맞춤)
        string body = text.Substring(bodyStart, close - bodyStart);
        string[] lines = body.Split('\n');
        int[] starts = new int[lines.Length];

        for (int i = 1; i < lines.Length; i++)
            starts[i] = starts[i - 1] + lines[i - 1].Length + 1;

        int commaAt = -1;
        string indent = null;

        for (int i = lines.Length - 1; i >= 0; i--)
        {
            string line = lines[i].TrimEnd('\r');
            string code = line.Substring(0, CodeLength(line)).TrimEnd();

            if (code.Trim().Length == 0)
                continue;

            indent = line.Substring(0, line.Length - line.TrimStart().Length);

            if (!code.EndsWith(",") && !code.EndsWith("{"))
                commaAt = bodyStart + starts[i] + code.Length;

            break;
        }

        if (indent == null)
        {
            // 값이 하나도 없는 enum: enum 줄 들여쓰기 + 4칸
            int headerLineStart = text.LastIndexOf('\n', header.Index) + 1;
            string headerLine = text.Substring(headerLineStart, header.Index - headerLineStart);
            indent = headerLine.Substring(0, headerLine.Length - headerLine.TrimStart().Length) + "    ";
        }

        string added = string.Concat(entries.Select(e => EntryLine(e, indent) + newline));

        // '}'가 줄 맨 앞(들여쓰기만 있음)이면 그 줄 앞에 넣고, 한 줄짜리 enum이면 '}' 바로 앞에 줄을 바꿔서 넣음
        int closeLineStart = text.LastIndexOf('\n', close) + 1;
        bool braceOnOwnLine = text.Substring(closeLineStart, close - closeLineStart).Trim().Length == 0;

        if (braceOnOwnLine)
            text = text.Insert(closeLineStart, added);
        else
            text = text.Insert(close, newline + added);

        if (commaAt >= 0)
            text = text.Insert(commaAt, ",");

        File.WriteAllText(sourcePath, text, new UTF8Encoding(bom));

        string message = $"{type.Name}에 {entries.Count}개를 추가했어요.\n({sourcePath})";
        ResetEntries();

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("값 추가", message, "확인");
    }

    private void ResetEntries()
    {
        enumName = "";
        description = "";
        entries.Clear();
        entries.Add(new Entry());
        GUI.FocusControl(null);
    }

    // ───────── 코드 읽기 ─────────

    // enum이 적힌 .cs 파일 찾기
    private static string FindSource(Type type)
    {
        if (type == null)
            return null;

        Regex pattern = new Regex($@"\benum\s+{Regex.Escape(type.Name)}\b");

        foreach (string path in Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories))
        {
            if (pattern.IsMatch(File.ReadAllText(path)))
                return path.Replace('\\', '/');
        }

        return null;
    }

    // 한 줄에서 // 주석 앞까지의 길이 (따옴표 안의 //는 무시)
    private static int CodeLength(string line)
    {
        bool inString = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inString)
            {
                if (c == '\\') i++;
                else if (c == '"') inString = false;
            }
            else if (c == '"')
            {
                inString = true;
            }
            else if (c == '/' && i + 1 < line.Length && line[i + 1] == '/')
            {
                return i;
            }
        }

        return line.Length;
    }

    // start 바로 앞의 '{'에 짝이 맞는 '}' 위치 (따옴표 / 주석 안은 무시). DatabaseCreatorWindow에서도 씀
    internal static int FindClosingBrace(string text, int start)
    {
        int depth = 1;
        bool inString = false, inLineComment = false, inBlockComment = false;

        for (int i = start; i < text.Length; i++)
        {
            char c = text[i];
            char next = i + 1 < text.Length ? text[i + 1] : '\0';

            if (inLineComment) { if (c == '\n') inLineComment = false; continue; }
            if (inBlockComment) { if (c == '*' && next == '/') { inBlockComment = false; i++; } continue; }
            if (inString) { if (c == '\\') i++; else if (c == '"') inString = false; continue; }

            if (c == '/' && next == '/') { inLineComment = true; i++; continue; }
            if (c == '/' && next == '*') { inBlockComment = true; i++; continue; }
            if (c == '"') { inString = true; continue; }

            if (c == '{') depth++;
            else if (c == '}' && --depth == 0) return i;
        }

        return -1;
    }
}
