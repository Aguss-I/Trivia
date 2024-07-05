using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManagment : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _categoryText;
    [SerializeField] TextMeshProUGUI _questionText;
    [SerializeField] TextMeshProUGUI _textTimer;
    [SerializeField] TextMeshProUGUI _textScore;

    string _correctAnswer;

    public Button[] _buttons = new Button[3];

    [SerializeField] Button _backButton;
    [SerializeField] Button _volverButton;

    private List<string> _answers = new List<string>();

    public bool queryCalled;

    public float currentTime = 0f;

    public int score = 0;

    public float scoreTotal = 0f;

    public float startingTime = 10f;

    private Color _originalButtonColor;

    public static UIManagment Instance { get; private set; }

    private bool questionAnswered = false; // Variable para controlar si la pregunta ha sido respondida

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        queryCalled = false;
        _originalButtonColor = _buttons[0].GetComponent<Image>().color;
        currentTime = startingTime;
        score = 0;
        questionAnswered = false;
    }

    void Update()
    {
        _categoryText.text = PlayerPrefs.GetString("SelectedTrivia");
        
        if (GameManager.Instance != null && GameManager.Instance.responseList != null && GameManager.Instance.responseList.Count > 0)
        {
            _questionText.text = GameManager.Instance.responseList[GameManager.Instance.randomQuestionIndex].QuestionText;
        }
        else
        {
            Debug.LogError("GameManager.Instance o su responseList es null o está vacío.");
        }

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
        }
        else
        {
            currentTime = 0;
        }
        _textTimer.text = currentTime.ToString("f0");
        _textScore.text = score.ToString("f0");

        if (!queryCalled && GameManager.Instance != null)
        {
            GameManager.Instance.CategoryAndQuestionQuery(queryCalled);
            queryCalled = true;
        }

        if(currentTime <= 0 && !questionAnswered)
        {
            Destroy(GameManager.Instance);
            Destroy(UIManagment.Instance);
            SceneManager.LoadScene("EscenaPerder");
        }   
    }

    public void OnButtonClick(int buttonIndex)
    {
        if (questionAnswered) return; // Si la pregunta ya ha sido respondida, no hacer nada

        string selectedAnswer = _buttons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>().text;
        _correctAnswer = GameManager.Instance.responseList[GameManager.Instance.randomQuestionIndex].CorrectOption;

        questionAnswered = true; // Marcar que la pregunta ha sido respondida

        if (selectedAnswer == _correctAnswer)
        {
            Debug.Log("¡Respuesta correcta!");
            score += Mathf.RoundToInt(currentTime);
            currentTime = startingTime;
            ChangeButtonColor(buttonIndex, Color.green);
            Invoke("RestoreButtonColor", 2f);
            GameManager.Instance._answers.Clear();
            Invoke("NextQuestion", 2f);
        }
        else
        {
            Debug.Log("Respuesta incorrecta. Inténtalo de nuevo.");
            ChangeButtonColor(buttonIndex, Color.red);
            SceneManager.LoadScene("EscenaPerder");
            Invoke("RestoreButtonColor", 2f);
        }
    }

    private void ChangeButtonColor(int buttonIndex, Color color)
    {
        Image buttonImage = _buttons[buttonIndex].GetComponent<Image>();
        buttonImage.color = color;
    }

    private void RestoreButtonColor()
    {
        foreach (Button button in _buttons)
        {
            Image buttonImage = button.GetComponent<Image>();
            buttonImage.color = _originalButtonColor;
        }
    }

    private void NextQuestion()
    {
        queryCalled = false;
        currentTime = startingTime;
        questionAnswered = false; // Permitir que se pueda responder la siguiente pregunta
        GameManager.Instance.randomQuestionIndex = Random.Range(0, GameManager.Instance.responseList.Count); // Obtener una nueva pregunta
    }

    public void PreviousScene()
    {
        Destroy(GameManager.Instance);
        Destroy(UIManagment.Instance);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void Volver()
    {
        score = 0;
        currentTime = 0;
        Destroy(GameManager.Instance);
        Destroy(UIManagment.Instance);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
