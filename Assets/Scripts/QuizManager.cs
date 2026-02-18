using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class Question
{
    [TextArea]
    public string questionText;

    public string[] answers = new string[4];

    public int correctAnswerIndex;
}

public class QuizManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts;

    [Header("Soru Listesi (Inspector)")]
    public List<Question> questions = new List<Question>();

    [Header("Durum")]
    public bool quizActive = false;
    public int correctAnswerCount = 0;

    private int currentQuestionIndex = -1;

    void Start()
    {
        if (quizPanel != null)
            quizPanel.SetActive(false);
    }

    public void StartQuiz()
    {
        LoadQuestionsFromBank();

        if (questions.Count == 0)
        {
            Debug.LogWarning("QuizManager: Soru yok! (JSON veya Inspector boþ)");
            return;
        }

        correctAnswerCount = 0;
        quizActive = true;

        if (quizPanel != null)
            quizPanel.SetActive(true);

        LoadRandomQuestion();
    }

    public void EndQuiz()
    {
        quizActive = false;

        if (quizPanel != null)
            quizPanel.SetActive(false);

        Debug.Log("Quiz bitti, doðru sayýsý: " + correctAnswerCount);
    }

    public void OnAnswerClicked(int answerIndex)
    {
        if (!quizActive || currentQuestionIndex < 0)
            return;

        Question q = questions[currentQuestionIndex];

        if (answerIndex == q.correctAnswerIndex)
        {
            correctAnswerCount++;
            Debug.Log("DOÐRU! Toplam doðru: " + correctAnswerCount);
        }
        else
        {
            Debug.Log("YANLIÞ!");
        }

        LoadRandomQuestion();
    }

    private void LoadRandomQuestion()
    {
        if (questions.Count == 0)
            return;

        if (questions.Count == 1)
            currentQuestionIndex = 0;
        else
        {
            int newIndex = Random.Range(0, questions.Count);

            while (newIndex == currentQuestionIndex)
            {
                newIndex = Random.Range(0, questions.Count);
            }

            currentQuestionIndex = newIndex;
        }

        Question q = questions[currentQuestionIndex];

        if (questionText != null)
            questionText.text = q.questionText;

        for (int i = 0; i < answerTexts.Length; i++)
        {
            if (i < q.answers.Length)
                answerTexts[i].text = q.answers[i];
            else
                answerTexts[i].text = "";
        }
    }

    private void LoadQuestionsFromBank()
    {
        var bank = QuestionBankStorage.Load();

        questions.Clear();

        foreach (var q in bank.questions)
        {
            Question newQ = new Question();
            newQ.questionText = q.questionText;
            newQ.answers = q.answers;
            newQ.correctAnswerIndex = q.correctAnswerIndex;

            questions.Add(newQ);
        }
    }
}
