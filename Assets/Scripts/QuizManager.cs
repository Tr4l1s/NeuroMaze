using UnityEngine;
using TMPro;
using System.Collections.Generic;



[System.Serializable]
public class Question
{
    [TextArea]
    public string questionText;          // Soru metni

    public string[] answers = new string[4]; // 4 tane þýk (A,B,C,D)

    public int correctAnswerIndex;       // 0,1,2,3 -> hangi þýk doðru
}

public class QuizManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public GameObject quizPanel;              // QuizPanel objesi
    public TextMeshProUGUI questionText;      // QuestionText
    public TextMeshProUGUI[] answerTexts;     // 4 buton içindeki textler

    [Header("Soru Listesi")]
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
        if (questions.Count == 0)
        {
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

}
