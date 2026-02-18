using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionBankUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panel;
    public GameObject mainMenuPanel;

    [Header("Inputs (Legacy InputField)")]
    public InputField questionInput;
    public InputField aInput;
    public InputField bInput;
    public InputField cInput;
    public InputField dInput;

    [Header("Correct (TMP Dropdown)")]
    public TMP_Dropdown correctDropdown;

    [Header("Navigation")]
    public Scrollbar questionScrollbar;

    private QuestionBankData bank;
    private int currentIndex = -1;

    void Start()
    {
        bank = QuestionBankStorage.Load();

        if (panel != null) panel.SetActive(false);

        SetupScrollbar();
        ShowIndex(bank.questions.Count > 0 ? 0 : -1);
    }

    public void OpenPanel()
    {
        bank = QuestionBankStorage.Load();

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (panel != null) panel.SetActive(true);

        SetupScrollbar();
        ShowIndex(bank.questions.Count > 0 ? 0 : -1);
    }

    public void AddQuestion()
    {
        if (bank == null) bank = new QuestionBankData();

        string qText = questionInput.text.Trim();
        string A = aInput.text.Trim();
        string B = bInput.text.Trim();
        string C = cInput.text.Trim();
        string D = dInput.text.Trim();

        if (string.IsNullOrWhiteSpace(qText)) return;
        if (string.IsNullOrWhiteSpace(A) || string.IsNullOrWhiteSpace(B) ||
            string.IsNullOrWhiteSpace(C) || string.IsNullOrWhiteSpace(D))
            return;

        var q = new QuestionData
        {
            questionText = qText,
            answers = new string[4] { A, B, C, D },
            correctAnswerIndex = correctDropdown != null ? correctDropdown.value : 0
        };

        bank.questions.Add(q);

        SetupScrollbar();
        ShowIndex(bank.questions.Count - 1);

        ClearFieldsForNew();
    }

    public void SaveAndBackToMenu()
    {
        if (bank == null) bank = new QuestionBankData();

        if (bank.questions.Count > 0 && currentIndex >= 0 && currentIndex < bank.questions.Count)
        {
            WriteFieldsIntoQuestion(bank.questions[currentIndex]);
        }

        QuestionBankStorage.Save(bank);

        if (panel != null) panel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void OnScrollbarChanged(float value)
    {
        if (bank == null || bank.questions.Count == 0) return;

        int count = bank.questions.Count;
        int idx = Mathf.RoundToInt(value * (count - 1));
        ShowIndex(idx);
    }

    private void SetupScrollbar()
    {
        if (questionScrollbar == null) return;

        questionScrollbar.onValueChanged.RemoveListener(OnScrollbarChanged);

        if (bank == null || bank.questions.Count <= 1)
        {
            questionScrollbar.numberOfSteps = 0;
            questionScrollbar.size = 1f;
            questionScrollbar.SetValueWithoutNotify(0f);
            questionScrollbar.onValueChanged.AddListener(OnScrollbarChanged);
            return;
        }

        int count = bank.questions.Count;

        questionScrollbar.numberOfSteps = count;
        questionScrollbar.size = Mathf.Clamp01(1f / count);
        questionScrollbar.onValueChanged.AddListener(OnScrollbarChanged);
    }

    private void ShowIndex(int idx)
    {
        currentIndex = idx;

        if (bank == null || bank.questions.Count == 0 || idx < 0 || idx >= bank.questions.Count)
        {
            ClearFieldsForNew();
            if (questionScrollbar != null) questionScrollbar.SetValueWithoutNotify(0f);
            return;
        }

        if (questionScrollbar != null && bank.questions.Count > 1)
        {
            float v = (float)idx / (bank.questions.Count - 1);
            questionScrollbar.SetValueWithoutNotify(v);
        }

        var q = bank.questions[idx];

        questionInput.text = q.questionText;
        aInput.text = q.answers[0];
        bInput.text = q.answers[1];
        cInput.text = q.answers[2];
        dInput.text = q.answers[3];

        if (correctDropdown != null)
            correctDropdown.value = Mathf.Clamp(q.correctAnswerIndex, 0, 3);
    }

    private void ClearFieldsForNew()
    {
        questionInput.text = "";
        aInput.text = "";
        bInput.text = "";
        cInput.text = "";
        dInput.text = "";

        if (correctDropdown != null)
            correctDropdown.value = 0;
    }

    private void WriteFieldsIntoQuestion(QuestionData q)
    {
        q.questionText = questionInput.text.Trim();
        q.answers[0] = aInput.text.Trim();
        q.answers[1] = bInput.text.Trim();
        q.answers[2] = cInput.text.Trim();
        q.answers[3] = dInput.text.Trim();

        q.correctAnswerIndex = correctDropdown != null ? correctDropdown.value : 0;
    }
}
