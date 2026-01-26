using System;
using System.Collections.Generic;

[Serializable]
public class QuestionData
{
    public string questionText;
    public string[] answers = new string[4];
    public int correctAnswerIndex;
}

[Serializable]
public class QuestionBankData
{
    public List<QuestionData> questions = new List<QuestionData>();
}
