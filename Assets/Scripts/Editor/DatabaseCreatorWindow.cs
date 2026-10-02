using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

// 새 데이터베이스(데이터 종류)를 만드는 창. 상단 메뉴 ProjectJ → 데이터베이스 → 새 데이터베이스 만들기
// 이름과 칸을 적고 만들기를 누르면 아래를 자동으로 만든다 (예: 영문 이름 Weapon)
//  - Assets/Scripts/Weapon/WeaponData.cs      데이터 하나의 모양 (칸 목록)
//  - Assets/Scripts/Weapon/WeaponDatabase.cs  게임에서 WeaponDatabase.Get("key")로 찾기
//  - Assets/Resources/Weapons                  데이터 에셋이 저장되는 폴더
//  - 구글 시트 링크 칸 (GoogleSheetSettings의 Other Sheets)
// 그다음 데이터 하나하나는 ProjectJ → 데이터베이스 → 새 데이터 만들기 또는 구글 시트로 추가
// "기존 데이터베이스에 칸 추가" 탭: 이미 있는 데이터(예: WeaponData, ItemData) 코드 맨 아래에 칸을 넣어준다
public class DatabaseCreatorWindow : EditorWindow
{
    // 칸 형식 하나 (형식 목록의 한 줄)
    private class TypeOption
    {
        public string Label;   // 목록에 보이는 이름 ('/'는 하위 메뉴)
        public string Code;    // 코드에 쓰는 형식 이름
        public Type Type;
        public bool Multiline; // string을 여러 줄 입력칸으로
    }

    [Serializable]
    private class Field
    {
        public string name = "";
        public string type = "string";
        public bool multiline;
        public bool list;
        public string defaultValue = "";
        public string description = "";
    }

    [SerializeField] private string className = "";
    [SerializeField] private string displayName = "";
    [SerializeField] private List<Field> fields = new List<Field>();
    [SerializeField] private int mode;
    [SerializeField] private string targetType; // 칸을 추가할 데이터 (FullName)

    private static readonly string[] Modes = { "새 데이터베이스 만들기", "기존 데이터베이스에 칸 추가" };
    private string sourcePath; // targetType이 적힌 파일 (못 찾으면 null)
    private string sourceFor;  // sourcePath를 찾은 targetType

    private ReorderableList list;
    private Vector2 scroll;
    private List<TypeOption> options;
    private string[] optionLabels;

    private static readonly Regex Identifier = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");
    private static readonly Regex Number = new Regex(@"-?\d+(\.\d+)?([eE][-+]?\d+)?");

    // 칸 이름으로 못 쓰는 것: C# 예약어 + ScriptableObject가 이미 가진 이름 + key(파일 이름으로 씀)
    private static readonly HashSet<string> ReservedFieldNames = new HashSet<string> { "name", "hideFlags", "key" };

    private static readonly HashSet<string> Keywords = new HashSet<string>
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
        "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
        "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
    };

    [MenuItem("ProjectJ/데이터베이스/새 데이터베이스 만들기", priority = 1)]
    public static void Open()
    {
        DatabaseCreatorWindow window = GetWindow<DatabaseCreatorWindow>("새 데이터베이스");
        window.minSize = new Vector2(640, 380);
    }

    private void OnEnable()
    {
        if (fields.Count == 0)
            ResetFields();

        BuildOptions();

        list = new ReorderableList(fields, typeof(Field), true, true, true, true);
        list.drawHeaderCallback = DrawHeader;
        list.drawElementCallback = DrawField;
        list.onAddCallback = l => fields.Add(new Field());
    }

    // 다른 데이터베이스나 enum이 새로 생기면 형식 목록에 반영
    private void OnProjectChange()
    {
        BuildOptions();
        Repaint();
    }

    private void ResetFields()
    {
        fields.Clear();
        fields.Add(new Field { name = "displayName", type = "string", description = "이름" });
    }

    // ───────── 형식 목록 ─────────

    private void BuildOptions()
    {
        options = new List<TypeOption>();

        void Add(string label, Type type, string code = null, bool multiline = false)
        {
            options.Add(new TypeOption { Label = label, Type = type, Code = code ?? CodeName(type), Multiline = multiline });
        }

        Add("string", typeof(string), "string");
        Add("string (여러 줄)", typeof(string), "string", true);
        Add("int", typeof(int), "int");
        Add("float", typeof(float), "float");
        Add("bool", typeof(bool), "bool");
        Add("long", typeof(long), "long");
        Add("double", typeof(double), "double");

        Add("Unity/Vector2", typeof(Vector2));
        Add("Unity/Vector3", typeof(Vector3));
        Add("Unity/Vector2Int", typeof(Vector2Int));
        Add("Unity/Vector3Int", typeof(Vector3Int));
        Add("Unity/Color", typeof(Color));
        Add("Unity/AnimationCurve", typeof(AnimationCurve));

        Add("에셋/Sprite", typeof(Sprite));
        Add("에셋/Texture2D", typeof(Texture2D));
        Add("에셋/GameObject (프리팹)", typeof(GameObject));
        Add("에셋/AudioClip", typeof(AudioClip));
        Add("에셋/Material", typeof(Material));
        Add("에셋/AnimationClip", typeof(AnimationClip));
        Add("에셋/SpriteAnimationData", typeof(SpriteAnimationData));
        Add("에셋/SpriteAnimationCollection", typeof(SpriteAnimationCollection));

        // 다른 데이터베이스의 데이터 (예: 무기 데이터에서 아이템 하나 연결)
        foreach (Type type in TypeCache.GetTypesWithAttribute<GameDatabaseAttribute>().OrderBy(t => t.Name))
        {
            if (typeof(ScriptableObject).IsAssignableFrom(type) && !type.IsAbstract)
                Add($"데이터/{type.Name} ({Info(type).DisplayName})", type);
        }

        // 프로젝트에 있는 enum (예: ItemEffectType)
        foreach (Type type in ProjectEnums())
            Add("enum/" + CodeName(type), type);

        optionLabels = options.Select(o => o.Label).ToArray();
    }

    // EnumCreatorWindow에서도 같이 씀
    internal static bool IsKeyword(string word)
    {
        return Keywords.Contains(word);
    }

    internal static IEnumerable<Type> ProjectEnums()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.StartsWith("Assembly-CSharp") && !a.GetName().Name.Contains("Editor"))
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsEnum && (t.IsPublic || t.IsNestedPublic))
            .OrderBy(t => t.FullName);
    }

    // 코드에 쓰는 형식 이름 (UnityEngine은 using이 있으니 생략, 안에 들어간 형식은 Outer.Inner)
    private static string CodeName(Type type)
    {
        string name = type.FullName.Replace('+', '.');
        return type.Namespace == "UnityEngine" ? name.Substring("UnityEngine.".Length) : name;
    }

    private int OptionIndex(Field field)
    {
        return options.FindIndex(o => o.Code == field.type && o.Multiline == (field.multiline && o.Type == typeof(string)));
    }

    private TypeOption OptionOf(Field field)
    {
        int index = OptionIndex(field);
        return index >= 0 ? options[index] : null;
    }

    // 기본값을 적을 수 있는 형식
    private static bool SupportsDefault(Type type)
    {
        return type == typeof(string) || type == typeof(int) || type == typeof(float) || type == typeof(bool)
               || type == typeof(long) || type == typeof(double)
               || type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector2Int) || type == typeof(Vector3Int)
               || type == typeof(Color) || type.IsEnum;
    }

    private static string DefaultHint(Type type)
    {
        if (type == typeof(Vector2) || type == typeof(Vector2Int)) return "x, y";
        if (type == typeof(Vector3) || type == typeof(Vector3Int)) return "x, y, z";
        if (type == typeof(Color)) return "#RRGGBB";
        if (type == typeof(bool)) return "true / false";
        if (type.IsEnum) return Enum.GetNames(type).FirstOrDefault() ?? "";
        return "";
    }

    // ───────── 화면 ─────────

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        int newMode = GUILayout.Toolbar(mode, Modes);

        if (newMode != mode)
        {
            mode = newMode;

            if (mode == 0)
                ResetFields();
            else
            {
                fields.Clear();
                fields.Add(new Field());
            }

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
        EditorGUILayout.HelpBox(
            "데이터 종류(예: 무기)를 새로 만든다. 칸을 정하고 만들기를 누르면 코드와 폴더가 자동으로 생긴다.\n" +
            "그다음 데이터 하나하나는 ProjectJ → 데이터베이스 → 새 데이터 만들기 또는 구글 시트로 추가하면 된다.\n" +
            "List에 체크하면 여러 개를 담는 칸이 된다 (시트에서는 a | b | c 처럼 | 로 구분).",
            MessageType.Info);

        className = EditorGUILayout.TextField(
            new GUIContent("영문 이름", "코드에 쓰이는 이름. 대문자로 시작 (예: Weapon → WeaponData, WeaponDatabase)"),
            className).Trim();
        displayName = EditorGUILayout.TextField(
            new GUIContent("한글 이름", "메뉴와 구글 시트 탭 이름 (예: 무기)"), displayName).Trim();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("칸  (key = 에셋 파일 이름이라 따로 안 만들어도 됨)", EditorStyles.boldLabel);
        list.DoLayoutList();

        if (className.Length > 0)
        {
            EditorGUILayout.LabelField("만들어지는 것", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"  {ScriptFolder}/{className}Data.cs");
            EditorGUILayout.LabelField($"  {ScriptFolder}/{className}Database.cs");
            EditorGUILayout.LabelField($"  {DataFolder}/");
        }

        List<string> errors = Validate();

        foreach (string error in errors)
            EditorGUILayout.HelpBox(error, MessageType.Warning);

        using (new EditorGUI.DisabledScope(errors.Count > 0))
        {
            if (GUILayout.Button("만들기", GUILayout.Height(30)))
                Create();
        }
    }

    private void DrawAppend()
    {
        Type[] types = DatabaseTypes();

        if (types.Length == 0)
        {
            EditorGUILayout.HelpBox("데이터베이스가 아직 없어요. 먼저 새 데이터베이스를 만들어주세요.", MessageType.Info);
            return;
        }

        int index = Array.FindIndex(types, t => t.FullName == targetType);

        if (index < 0)
            index = 0;

        index = EditorGUILayout.Popup("데이터베이스", index, types.Select(t => $"{Info(t).DisplayName} ({t.Name})").ToArray());
        Type type = types[index];
        targetType = type.FullName;

        if (sourceFor != targetType)
        {
            sourcePath = FindClassSource(type);
            sourceFor = targetType;
        }

        EditorGUILayout.LabelField("파일", sourcePath ?? "못 찾음");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("지금 있는 칸", EditorStyles.boldLabel);

        foreach (FieldInfo field in ExistingFields(type))
            EditorGUILayout.LabelField($"  {field.Name}", TypeLabel(field.FieldType));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("추가할 칸  (맨 아래에 추가됨, 기존 데이터는 기본값으로 채워짐)", EditorStyles.boldLabel);
        list.DoLayoutList();

        List<string> errors = new List<string>();

        if (sourcePath == null)
            errors.Add("이 데이터가 적힌 파일을 Assets 안에서 못 찾았어요");

        if (fields.Count == 0)
            errors.Add("칸을 하나 이상 추가해주세요");

        HashSet<string> existing = new HashSet<string>(
            type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(f => f.Name));
        ValidateFields(errors, existing, type.Name);

        foreach (string error in errors)
            EditorGUILayout.HelpBox(error, MessageType.Warning);

        using (new EditorGUI.DisabledScope(errors.Count > 0))
        {
            if (GUILayout.Button("추가", GUILayout.Height(30)))
                AppendFields(type);
        }
    }

    private static Type[] DatabaseTypes()
    {
        return TypeCache.GetTypesWithAttribute<GameDatabaseAttribute>()
            .Where(t => typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract)
            .OrderBy(t => Info(t).DisplayName)
            .ToArray();
    }

    // Inspector에 보이는 칸
    private static IEnumerable<FieldInfo> ExistingFields(Type type)
    {
        return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(f => (f.IsPublic && !f.IsDefined(typeof(NonSerializedAttribute), true)) || f.IsDefined(typeof(SerializeField), true))
            .Where(f => !f.IsDefined(typeof(HideInInspector), true));
    }

    private static string TypeLabel(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return $"List<{TypeLabel(type.GetGenericArguments()[0])}>";

        switch (type.Name)
        {
            case "String": return "string";
            case "Int32": return "int";
            case "Single": return "float";
            case "Boolean": return "bool";
            case "Int64": return "long";
            case "Double": return "double";
            default: return CodeName(type);
        }
    }

    private static Rect[] Columns(Rect rect)
    {
        float[] widths = { 0.22f, 0.24f, 0.08f, 0.17f, 0.29f };
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
        EditorGUI.LabelField(c[0], "칸 이름 (영문)");
        EditorGUI.LabelField(c[1], "형식");
        EditorGUI.LabelField(c[2], "List");
        EditorGUI.LabelField(c[3], "기본값");
        EditorGUI.LabelField(c[4], "설명 (시트 / 툴팁에 표시)");
    }

    private void DrawField(Rect rect, int index, bool active, bool focused)
    {
        Field field = fields[index];
        Rect[] c = Columns(rect);

        field.name = EditorGUI.TextField(c[0], field.name).Trim();

        int current = OptionIndex(field);
        int picked = EditorGUI.Popup(c[1], current, optionLabels);

        if (picked != current && picked >= 0)
        {
            field.type = options[picked].Code;
            field.multiline = options[picked].Multiline;
            field.defaultValue = "";
        }

        TypeOption option = OptionOf(field);
        field.list = EditorGUI.Toggle(c[2], field.list);

        if (option == null || field.list || !SupportsDefault(option.Type))
        {
            field.defaultValue = "";
            using (new EditorGUI.DisabledScope(true))
                EditorGUI.TextField(c[3], "없음");
        }
        else
        {
            field.defaultValue = EditorGUI.TextField(c[3], field.defaultValue);

            // 비어 있으면 어떻게 적는지 흐리게 보여줌
            string hint = DefaultHint(option.Type);

            if (field.defaultValue.Length == 0 && hint.Length > 0 && Event.current.type == EventType.Repaint)
                EditorStyles.centeredGreyMiniLabel.Draw(c[3], hint, false, false, false, false);
        }

        field.description = EditorGUI.TextField(c[4], field.description);
    }

    // ───────── 검사 ─────────

    private string ScriptFolder => $"Assets/Scripts/{className}";
    private string DataFolder => $"Assets/Resources/{FolderName}";

    // Weapon → Weapons, Boss → Bosses, Enemy → Enemies
    private string FolderName
    {
        get
        {
            if (Regex.IsMatch(className, "(s|x|sh|ch)$"))
                return className + "es";

            if (Regex.IsMatch(className, "[^aeiouAEIOU]y$"))
                return className.Substring(0, className.Length - 1) + "ies";

            return className + "s";
        }
    }

    private List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (className.Length == 0)
            errors.Add("영문 이름을 적어주세요 (예: Weapon)");
        else if (!Identifier.IsMatch(className) || !char.IsUpper(className[0]))
            errors.Add("영문 이름은 대문자로 시작하고 영문 / 숫자만 쓸 수 있어요 (예: Weapon)");
        else if (TypeExists(className + "Data") || TypeExists(className + "Database") || Directory.Exists(ScriptFolder))
            errors.Add($"'{className}'은(는) 이미 있는 이름이에요");

        if (displayName.Length == 0)
            errors.Add("한글 이름을 적어주세요 (예: 무기)");
        else if (TypeCache.GetTypesWithAttribute<GameDatabaseAttribute>().Any(t => Info(t).DisplayName == displayName))
            errors.Add($"'{displayName}' 데이터베이스가 이미 있어요");

        if (fields.Count == 0)
            errors.Add("칸을 하나 이상 추가해주세요");

        ValidateFields(errors, new HashSet<string>(), className + "Data");
        return errors;
    }

    // 칸 목록 검사. existing = 이미 있는 칸 이름, typeName = 칸 이름으로 못 쓰는 클래스 이름
    private void ValidateFields(List<string> errors, HashSet<string> existing, string typeName)
    {
        HashSet<string> names = new HashSet<string>(existing);

        foreach (Field field in fields)
        {
            if (field.name.Length == 0)
            {
                errors.Add("칸 이름이 비어 있는 줄이 있어요");
                continue;
            }

            if (!Identifier.IsMatch(field.name))
                errors.Add($"칸 이름 '{field.name}': 영문 / 숫자 / _만 쓸 수 있고 숫자로 시작하면 안 돼요");
            else if (ReservedFieldNames.Contains(field.name) || IsKeyword(field.name) || field.name == typeName)
                errors.Add($"칸 이름 '{field.name}'은(는) 쓸 수 없는 이름이에요");
            else if (existing.Contains(field.name))
                errors.Add($"칸 '{field.name}'은(는) 이미 있어요");
            else if (!names.Add(field.name))
                errors.Add($"칸 이름 '{field.name}'이(가) 두 번 있어요");

            TypeOption option = OptionOf(field);

            if (option == null)
                errors.Add($"'{field.name}' 형식 '{field.type}'을(를) 찾을 수 없어요. 형식을 다시 골라주세요");
            else if (field.defaultValue.Trim().Length > 0 && DefaultLiteral(option.Type, field.defaultValue) == null)
                errors.Add($"'{field.name}' 기본값 '{field.defaultValue}'은(는) {option.Code}(으)로 못 읽어요"
                           + (DefaultHint(option.Type).Length > 0 ? $" (예: {DefaultHint(option.Type)})" : ""));
        }
    }

    private static GameDatabaseAttribute Info(Type type)
    {
        return (GameDatabaseAttribute)Attribute.GetCustomAttribute(type, typeof(GameDatabaseAttribute));
    }

    internal static bool TypeExists(string typeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetType(typeName, false) != null);
    }

    // 기본값을 코드에 쓸 모양으로 (못 읽으면 null)
    private static string DefaultLiteral(Type type, string text)
    {
        string value = text.Trim();
        CultureInfo inv = CultureInfo.InvariantCulture;

        if (type == typeof(string))
            return "\"" + Escape(text) + "\"";

        if (type == typeof(int))
            return int.TryParse(value, NumberStyles.Integer, inv, out int i) ? i.ToString(inv) : null;

        if (type == typeof(long))
            return long.TryParse(value, NumberStyles.Integer, inv, out long l) ? l.ToString(inv) + "L" : null;

        if (type == typeof(float))
            return float.TryParse(value, NumberStyles.Float, inv, out float f) ? Float(f) : null;

        if (type == typeof(double))
            return double.TryParse(value, NumberStyles.Float, inv, out double d) ? d.ToString("R", inv) + "d" : null;

        if (type == typeof(bool))
        {
            switch (value.ToUpperInvariant())
            {
                case "TRUE": case "O": case "1": case "켜기": return "true";
                case "FALSE": case "X": case "0": case "끄기": return "false";
                default: return null;
            }
        }

        if (type == typeof(Vector2) || type == typeof(Vector3))
        {
            float[] n = Numbers(value);
            int count = type == typeof(Vector2) ? 2 : 3;
            return n != null && n.Length == count
                ? $"new {type.Name}({string.Join(", ", n.Select(Float))})"
                : null;
        }

        if (type == typeof(Vector2Int) || type == typeof(Vector3Int))
        {
            float[] n = Numbers(value);
            int count = type == typeof(Vector2Int) ? 2 : 3;
            return n != null && n.Length == count && n.All(x => x == Mathf.Round(x))
                ? $"new {type.Name}({string.Join(", ", n.Select(x => ((int)x).ToString(inv)))})"
                : null;
        }

        if (type == typeof(Color))
        {
            string html = value.StartsWith("#") ? value : "#" + value;
            return ColorUtility.TryParseHtmlString(html, out Color c)
                ? $"new Color({Float(c.r)}, {Float(c.g)}, {Float(c.b)}, {Float(c.a)})"
                : null;
        }

        if (type.IsEnum)
        {
            // 코드 이름(HealHP) 또는 [InspectorName] 이름(HP 회복)
            foreach (string name in Enum.GetNames(type))
            {
                InspectorNameAttribute label = type.GetField(name).GetCustomAttributes(typeof(InspectorNameAttribute), false)
                    .FirstOrDefault() as InspectorNameAttribute;

                if (value.Equals(name, StringComparison.OrdinalIgnoreCase) || (label != null && value == label.displayName))
                    return $"{CodeName(type)}.{name}";
            }

            return null;
        }

        return null;
    }

    private static string Float(float value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture) + "f";
    }

    private static float[] Numbers(string text)
    {
        MatchCollection matches = Number.Matches(text);

        if (matches.Count == 0)
            return null;

        return matches.Cast<Match>().Select(m => float.Parse(m.Value, CultureInfo.InvariantCulture)).ToArray();
    }

    internal static string Escape(string text)
    {
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
    }

    // ───────── 만들기 ─────────

    private void Create()
    {
        Directory.CreateDirectory(ScriptFolder);
        File.WriteAllText($"{ScriptFolder}/{className}Data.cs", BuildDataScript());
        File.WriteAllText($"{ScriptFolder}/{className}Database.cs", BuildDatabaseScript());

        if (!AssetDatabase.IsValidFolder(DataFolder))
            AssetDatabase.CreateFolder("Assets/Resources", FolderName);

        GoogleSheetSettings.GetOrCreate().AddOtherSheet(className + "Data");

        string message =
            $"'{displayName}' 데이터베이스를 만들었어요.\n\n" +
            $"코드 컴파일이 끝나면 ProjectJ → 데이터베이스 → 새 데이터 만들기에서 '{displayName}'을(를) 고를 수 있어요.\n\n" +
            $"구글 시트로 관리하려면: ProjectJ → 구글 시트 → 링크 설정 → Other Sheets의 {className}Data에 탭 주소 붙여넣기\n" +
            "(시트 양식은 ProjectJ → 구글 시트 → 시트 양식 CSV 내보내기)";

        className = "";
        displayName = "";
        ResetFields();
        GUI.FocusControl(null);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("새 데이터베이스", message, "확인");
    }

    private string BuildDataScript()
    {
        StringBuilder code = new StringBuilder();

        if (fields.Any(f => f.list))
            code.Append("using System.Collections.Generic;\n");

        code.Append("using UnityEngine;\n\n");
        code.Append($"// {displayName} 하나의 정보 (ProjectJ → 데이터베이스 → 새 데이터베이스 만들기로 생성)\n");
        code.Append($"// 추가: ProjectJ → 데이터베이스 → 새 데이터 만들기 → {displayName}\n");
        code.Append($"//   또는 구글 시트 \"{displayName}\" 탭에 한 줄 추가 → ProjectJ → 구글 시트 → 전부 가져오기\n");
        code.Append($"// 저장 위치: {DataFolder} (에셋 파일 이름 = key). 게임에서 찾기: {className}Database.Get(\"key\")\n");
        code.Append("// 칸을 늘리려면 아래에 public 변수를 추가하면 된다 (구글 시트 열 이름 = 변수 이름)\n");
        code.Append($"[GameDatabase(\"{Escape(displayName)}\", \"{FolderName}\")]\n");
        code.Append($"public class {className}Data : ScriptableObject\n{{\n");

        foreach (Field field in fields)
            code.Append(FieldCode(field, "    "));

        code.Append("}\n");
        return code.ToString();
    }

    // 칸 하나의 코드 (툴팁 / 여러 줄 표시 / 변수 선언). 줄바꿈은 \n
    private string FieldCode(Field field, string indent)
    {
        TypeOption option = OptionOf(field);
        StringBuilder code = new StringBuilder();

        if (field.description.Trim().Length > 0)
            code.Append($"{indent}[Tooltip(\"{Escape(field.description.Trim())}\")]\n");

        if (option.Multiline)
            code.Append($"{indent}[TextArea]\n");

        string type = field.list ? $"List<{option.Code}>" : option.Code;
        string value = "";

        if (field.list)
            value = $" = new List<{option.Code}>()";
        else if (field.defaultValue.Trim().Length > 0)
            value = " = " + DefaultLiteral(option.Type, field.defaultValue);
        else if (option.Type == typeof(AnimationCurve))
            value = " = AnimationCurve.Linear(0f, 0f, 1f, 1f)";
        else if (option.Type == typeof(Color))
            value = " = Color.white"; // 기본 (0,0,0,0)은 투명이라 안 보임

        code.Append($"{indent}public {type} {field.name}{value};\n");
        return code.ToString();
    }

    // ───────── 기존 데이터베이스에 칸 추가 ─────────

    // 클래스가 적힌 .cs 파일 찾기
    private static string FindClassSource(Type type)
    {
        Regex pattern = new Regex($@"\bclass\s+{Regex.Escape(type.Name)}\b");

        foreach (string path in Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories))
        {
            if (pattern.IsMatch(File.ReadAllText(path)))
                return path.Replace('\\', '/');
        }

        return null;
    }

    private void AppendFields(Type type)
    {
        byte[] bytes = File.ReadAllBytes(sourcePath);
        bool bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        string text = File.ReadAllText(sourcePath);
        string newline = text.Contains("\r\n") ? "\r\n" : "\n";

        Match header = Regex.Match(text, $@"\bclass\s+{Regex.Escape(type.Name)}\b[^{{;]*\{{");
        int bodyStart = header.Success ? header.Index + header.Length : -1;
        int close = bodyStart >= 0 ? EnumCreatorWindow.FindClosingBrace(text, bodyStart) : -1;

        if (close < 0)
        {
            EditorUtility.DisplayDialog("칸 추가", $"{sourcePath}에서 {type.Name}의 범위를 못 찾았어요. 직접 추가해주세요.", "확인");
            return;
        }

        // 들여쓰기 = class 줄 들여쓰기 + 4칸
        int headerLineStart = text.LastIndexOf('\n', header.Index) + 1;
        string headerLine = text.Substring(headerLineStart, header.Index - headerLineStart);
        string indent = headerLine.Substring(0, headerLine.Length - headerLine.TrimStart().Length) + "    ";

        string added = newline + string.Concat(fields.Select(f => FieldCode(f, indent))).Replace("\n", newline);

        // '}'가 줄 맨 앞(들여쓰기만 있음)이면 그 줄 앞에 넣고, 아니면 '}' 바로 앞에 줄을 바꿔서 넣음
        int closeLineStart = text.LastIndexOf('\n', close) + 1;
        bool braceOnOwnLine = text.Substring(closeLineStart, close - closeLineStart).Trim().Length == 0;
        text = braceOnOwnLine ? text.Insert(closeLineStart, added) : text.Insert(close, newline + added);

        if (fields.Any(f => f.list) && !Regex.IsMatch(text, @"using\s+System\.Collections\.Generic\s*;"))
            text = "using System.Collections.Generic;" + newline + text;

        File.WriteAllText(sourcePath, text, new UTF8Encoding(bom));

        string message = $"{type.Name}에 칸 {fields.Count}개를 추가했어요.\n({sourcePath})\n\n" +
                         "기존 데이터는 기본값으로 채워져요. 구글 시트로 관리하면 시트에 같은 이름의 열만 추가하면 돼요.";
        fields.Clear();
        fields.Add(new Field());
        sourceFor = null;
        GUI.FocusControl(null);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("칸 추가", message, "확인");
    }

    private string BuildDatabaseScript()
    {
        const string template =
@"using System.Collections.Generic;
using UnityEngine;

// Resources/#FOLDER# 폴더의 #DATA#를 전부 불러와서 key(에셋 파일 이름)로 찾아준다
// 사용: #DATA# data = #DB#.Get(""key"");
//       foreach (#DATA# data in #DB#.All) { ... }
public static class #DB#
{
    private static Dictionary<string, #DATA#> all;

    // 도메인 리로드가 꺼져 있어서 플레이 시작마다 직접 초기화 (ItemDatabase와 같은 이유)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        all = null;
    }

    private static void Load()
    {
        if (all != null)
            return;

        all = new Dictionary<string, #DATA#>();

        foreach (#DATA# data in Resources.LoadAll<#DATA#>(""#FOLDER#""))
            all[data.name] = data;
    }

    public static #DATA# Get(string key)
    {
        Load();

        if (string.IsNullOrEmpty(key))
            return null;

        all.TryGetValue(key, out #DATA# data);
        return data;
    }

    public static IEnumerable<#DATA#> All
    {
        get
        {
            Load();
            return all.Values;
        }
    }
}
";
        return template
            .Replace("\r\n", "\n")
            .Replace("#DATA#", className + "Data")
            .Replace("#DB#", className + "Database")
            .Replace("#FOLDER#", FolderName);
    }
}
