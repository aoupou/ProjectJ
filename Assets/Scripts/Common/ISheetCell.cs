// 구글 시트 칸 하나를 직접 읽고 쓰는 형식 (예: AttackCell "(-1,1):3", WeaponEffect "Poison:2")
// 이걸 붙인 struct는 시트 가져오기에서 칸 형식으로 쓸 수 있다. List면 | 로 여러 개
public interface ISheetCell
{
    // 시트 글자 → 값. 못 읽으면 false + 이유
    bool TryParse(string text, out string error);

    // 값 → 시트 글자 (시트 양식 만들 때)
    string ToCellText();
}
