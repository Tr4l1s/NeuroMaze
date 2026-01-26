using UnityEngine;
using System.IO;

public static class QuestionBankStorage
{
    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "question_bank.json");

    public static void Save(QuestionBankData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
    }

    public static QuestionBankData Load()
    {
        if (!File.Exists(FilePath))
            return new QuestionBankData();

        string json = File.ReadAllText(FilePath);
        var loaded = JsonUtility.FromJson<QuestionBankData>(json);
        return loaded ?? new QuestionBankData();
    }
}
