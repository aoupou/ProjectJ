using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// 게임 전체 폰트를 손글씨 폰트 '개구(Gaegu)'로 통일한다
// 개구는 SIL OFL 라이선스 (게임에 넣어서 배포 / 상업적 사용 가능, 폰트 단독 판매만 금지)
//
// 씬에 이미 있는 글자들은 기존 폰트(LiberationSans)를 직접 가리키고 있어서,
// 씬 파일을 고치지 않고 씬이 열릴 때마다 모든 글자의 폰트를 바꿔준다
public static class GameFont
{
    public const string SourcePath = "Fonts/Gaegu-Regular";    // Resources 안의 폰트 파일(.ttf)
    public const string AssetPath = "Fonts/Gaegu-Regular SDF"; // Resources 안의 TMP 폰트 에셋 (에디터가 자동 생성)

    private static TMP_FontAsset font;
    private static bool loaded;

    // 이 프로젝트는 플레이 시작 시 도메인 리로드가 꺼져 있어서 static 값을 직접 초기화
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        font = null;
        loaded = false;
    }

    public static TMP_FontAsset Font
    {
        get
        {
            if (!loaded || font == null)
            {
                loaded = true;
                font = Resources.Load<TMP_FontAsset>(AssetPath);

                // 에셋이 아직 없으면 폰트 파일로 실행 중에 만듦
                if (font == null)
                {
                    Font source = Resources.Load<Font>(SourcePath);

                    if (source != null)
                        font = TMP_FontAsset.CreateFontAsset(source);
                }

                if (font == null)
                    Debug.LogWarning("[폰트] 개구 폰트를 불러오지 못해서 기본 폰트 사용");
            }

            return font != null ? font : TMP_Settings.defaultFontAsset;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TMP_FontAsset gameFont = Font;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.font != gameFont)
                    text.font = gameFont;
            }
        }
    }
}
