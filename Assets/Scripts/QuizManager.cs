using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class Question
{
    [TextArea] public string questionText;
    public string[] answers=new string[4];
    public int correctAnswerIndex;
}
public class QuizManager : MonoBehaviour
{
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts;
    public List<Question> questions=new List<Question>();
    public GameObject[] hideWhenQuizActive;
    public bool quizActive;
    public int correctAnswerCount;
    int currentQuestionIndex=-1;
    void Start() {if(quizPanel!=null) quizPanel.SetActive(false);SetGameplayUIVisible(true);}
    public void StartQuiz()
    {
        var bank=QuestionBankStorage.Load();
        if(bank!=null && bank.questions!=null && bank.questions.Count>0)
        {
            questions.Clear();
            foreach(var q in bank.questions) questions.Add(new Question {questionText=q.questionText,answers=q.answers,correctAnswerIndex=q.correctAnswerIndex});
        }
        questions.RemoveAll(q=>q==null || string.IsNullOrWhiteSpace(q.questionText) || q.answers==null || q.answers.Length<4 ||
            q.correctAnswerIndex<0 || q.correctAnswerIndex>3 || System.Array.Exists(q.answers,a=>string.IsNullOrWhiteSpace(a)));
        if(questions.Count==0)
        {
            questions.Add(new Question {questionText="3 + 4 kaç eder?",answers=new[]{"5","6","7","8"},correctAnswerIndex=2});
            questions.Add(new Question {questionText="10 - 3 kaç eder?",answers=new[]{"7","6","8","9"},correctAnswerIndex=0});
            questions.Add(new Question {questionText="2, 4, 6, ? Sıradaki sayı nedir?",answers=new[]{"7","8","9","10"},correctAnswerIndex=1});
        }
        correctAnswerCount=0;currentQuestionIndex=-1;quizActive=true;
        if(quizPanel!=null) quizPanel.SetActive(true);
        SetGameplayUIVisible(false);LoadRandomQuestion();
    }
    public void EndQuiz() {quizActive=false;if(quizPanel!=null) quizPanel.SetActive(false);SetGameplayUIVisible(true);}
    public void OnAnswerClicked(int answerIndex)
    {
        if(!quizActive || currentQuestionIndex<0 || answerIndex<0 || answerIndex>3) return;
        if(answerIndex==questions[currentQuestionIndex].correctAnswerIndex) correctAnswerCount++;
        LoadRandomQuestion();
    }
    void LoadRandomQuestion()
    {
        int next=questions.Count==1?0:Random.Range(0,questions.Count);
        if(questions.Count>1 && next==currentQuestionIndex) next=(next+1)%questions.Count;
        currentQuestionIndex=next;var q=questions[next];
        if(questionText!=null) questionText.text=q.questionText;
        if(answerTexts!=null) for(int i=0;i<answerTexts.Length;i++) if(answerTexts[i]!=null) answerTexts[i].text=i<q.answers.Length?q.answers[i]:"";
    }
    void SetGameplayUIVisible(bool visible)
    {
        if(hideWhenQuizActive!=null) foreach(var item in hideWhenQuizActive) if(item!=null) item.SetActive(visible);
    }
}
