using UnityEngine;
using System;
using System.IO;

[Serializable]
public sealed class SaveData
{
    public int beads;

    public int[] upgradeLevels;
}

public static class SaveManager 
{

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "save.json");

    public static SaveData Load()
    {
        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null || data.beads < 0)
                throw new InvalidDataException("세이브의 비즈 데이터를 읽을 수 없습니다.");

            return data;
        }
        catch (FileNotFoundException)
        {
            // 첫 실행에는 저장 파일이 없으므로 비즈 0개로 시작
            return new SaveData();
        }
        catch (DirectoryNotFoundException)
        {
            return new SaveData();
        }
    }

    public static void Save(SaveData data)
    {
        string path = SavePath;
        string temporaryPath = path + ".tmp";

        Directory.CreateDirectory(Application.persistentDataPath);

        // 임시 파일 쓰기가 끝난 뒤 실제 저장 파일을 교체해요.
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(temporaryPath, json);

        if (File.Exists(path))
            File.Replace(temporaryPath, path, null);
        else
            File.Move(temporaryPath, path);
    }
    
}
