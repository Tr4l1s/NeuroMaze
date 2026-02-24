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

    [Header("Quiz Açýlýnca Gizlenecek UI'lar")]
    public GameObject[] hideWhenQuizActive;

    [Header("Durum")]
    public bool quizActive = false;
    public int correctAnswerCount = 0;

    private int currentQuestionIndex = -1;

    void Start()
    {
        if (quizPanel != null)
            quizPanel.SetActive(false);

        SetGameplayUIVisible(true);
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

        SetGameplayUIVisible(false);

        LoadRandomQuestion();
    }

    public void EndQuiz()
    {
        quizActive = false;

        if (quizPanel != null)
            quizPanel.SetActive(false);

        SetGameplayUIVisible(true);

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

        int safety = 50;

        while (safety-- > 0)
        {
            int newIndex;

            if (questions.Count == 1)
                newIndex = 0;
            else
            {
                newIndex = Random.Range(0, questions.Count);
                while (newIndex == currentQuestionIndex)
                    newIndex = Random.Range(0, questions.Count);
            }

            Question candidate = questions[newIndex];

            if (candidate == null) continue;
            if (string.IsNullOrWhiteSpace(candidate.questionText)) continue;
            if (candidate.answers == null || candidate.answers.Length < 4) continue;

            bool anyEmpty =
                string.IsNullOrWhiteSpace(candidate.answers[0]) ||
                string.IsNullOrWhiteSpace(candidate.answers[1]) ||
                string.IsNullOrWhiteSpace(candidate.answers[2]) ||
                string.IsNullOrWhiteSpace(candidate.answers[3]);

            if (anyEmpty) continue;

            currentQuestionIndex = newIndex;

            if (questionText != null)
                questionText.text = candidate.questionText;

            for (int i = 0; i < answerTexts.Length; i++)
                answerTexts[i].text = (i < candidate.answers.Length) ? candidate.answers[i] : "";

            return;
        }

        Debug.LogWarning("QuizManager: Geçerli soru bulunamadý (hepsi boþ/eksik).");
        EndQuiz();
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

    private void SetGameplayUIVisible(bool visible)
    {
        if (hideWhenQuizActive == null) return;

        for (int i = 0; i < hideWhenQuizActive.Length; i++)
        {
            if (hideWhenQuizActive[i] != null)
                hideWhenQuizActive[i].SetActive(visible);
        }
    }
}