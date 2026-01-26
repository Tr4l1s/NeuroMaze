using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestionBankUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Inputs")]
    public TMP_InputField questionInput;
    public TMP_InputField aInput;
    public TMP_InputField bInput;
    public TMP_InputField cInput;
    public TMP_InputField dInput;
    public TMP_Dropdown correctDropdown;

    [Header("List UI")]
    public Transform listContent;           // ScrollView/Viewport/Content
    public GameObject questionRowPrefab;    // QuestionItemRow prefab

    private QuestionBankData bank;

    void Start()
    {
        bank = QuestionBankStorage.Load();
        if (panel != null) panel.SetActive(false);
    }

    public void OpenPanel()
    {
        bank = QuestionBankStorage.Load();
        if (panel != null) panel.SetActive(true);
        RefreshList();
    }

    public void ClosePanel()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void AddQuestion()
    {
        if (string.IsNullOrWhiteSpace(questionInput.text)) return;
        if (string.IsNullOrWhiteSpace(aInput.text)) return;
        if (string.IsNullOrWhiteSpace(bInput.text)) return;
        if (string.IsNullOrWhiteSpace(cInput.text)) return;
        if (string.IsNullOrWhiteSpace(dInput.text)) return;

        var q = new QuestionData();
        q.questionText = questionInput.text.Trim();
        q.answers = new string[4]
        {
            aInput.text.Trim(),
            bInput.text.Trim(),
            cInput.text.Trim(),
            dInput.text.Trim()
        };
        q.correctAnswerIndex = correctDropdown.value;

        bank.questions.Add(q);

        questionInput.text = "";
        aInput.text = "";
        bInput.text = "";
        cInput.text = "";
        dInput.text = "";
        correctDropdown.value = 0;

        RefreshList();
    }

    public void SaveQuestions()
    {
        QuestionBankStorage.Save(bank);
        Debug.Log("Kaydedildi. Toplam soru: " + bank.questions.Count);
    }

    private void RefreshList()
    {
        if (listContent == null || questionRowPrefab == null) return;

        // content temizle
        for (int i = listContent.childCount - 1; i >= 0; i--)
            Destroy(listContent.GetChild(i).gameObject);

        // yeniden oluþtur
        for (int i = 0; i < bank.questions.Count; i++)
        {
            int index = i; // closure fix
            var q = bank.questions[i];

            GameObject row = Instantiate(questionRowPrefab, listContent);

            // Row içinden referanslarý bul
            // Row’da bir TMP_Text ve bir Button olduðunu varsayýyoruz
            TMP_Text txt = row.GetComponentInChildren<TMP_Text>();
            Button btn = row.GetComponentInChildren<Button>();

            if (txt != null)
                txt.text = $"{index + 1}. {Shorten(q.questionText, 60)}";

            if (btn != null)
                btn.onClick.AddListener(() => DeleteQuestion(index));
        }
    }

    private void DeleteQuestion(int index)
    {
        if (index < 0 || index >= bank.questions.Count) return;

        bank.questions.RemoveAt(index);
        RefreshList();
    }

    private string Shorten(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }
}
