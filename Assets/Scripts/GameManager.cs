using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    string supabaseUrl = "https://jenevwbrobsgzfzlehpm.supabase.co"; //COMPLETAR
    string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImplbmV2d2Jyb2JzZ3pmemxlaHBtIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MTk5NTA2MzUsImV4cCI6MjAzNTUyNjYzNX0.1n5hSVIr6kJC6XmiWiTpp4jnrLFeitIsPWeV0sPHC_Y"; //COMPLETAR
    Supabase.Client clientSupabase;
    [SerializeField] TextMeshProUGUI _puntosText;

    public List<question> responseList; //lista donde guardo la respuesta de la query hecha en la pantalla de selecci n de categor a

    public int currentTriviaIndex = 0;
    public int randomQuestionIndex = 0;

    public List<string> _answers = new List<string>();
    public bool queryCalled;

    public float score;

    private int _maxAttempts = 10;
    public int _numQuestionAnswered = 0;
    string _correctAnswer;

    public static GameManager Instance { get; private set; }

    private HashSet<int> shownQuestions = new HashSet<int>();
    void Awake()

    {
        // Configura la instancia
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Para mantener el objeto entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    clientSupabase = new Supabase.Client(supabaseUrl, supabaseKey);

    }


    void Start()
    {
        StartTrivia();
        queryCalled = false;
    }

    void StartTrivia()
    {
        // Cargar la trivia desde la base de datos
        //triviaManager.LoadTrivia(currentTriviaIndex);

        //print(responseList.Count);

    }

    public void CategoryAndQuestionQuery(bool isCalled)
    {
        isCalled = UIManagment.Instance.queryCalled;

        if (!isCalled)
        {
            if (shownQuestions.Count >= responseList.Count)
            {
                EndGame();
                return;
            }

            do
            {
                randomQuestionIndex = Random.Range(0, responseList.Count);
            } while (shownQuestions.Contains(randomQuestionIndex));

            shownQuestions.Add(randomQuestionIndex);

            _correctAnswer = responseList[randomQuestionIndex].CorrectOption;

            _answers.Clear();
            _answers.Add(responseList[randomQuestionIndex].Answer1);
            _answers.Add(responseList[randomQuestionIndex].Answer2);
            _answers.Add(responseList[randomQuestionIndex].Answer3);
            _answers.Shuffle();

            for (int i = 0; i < UIManagment.Instance._buttons.Length; i++)
            {
                UIManagment.Instance._buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = _answers[i];
                int index = i; // Captura el valor actual de i en una variable local
                UIManagment.Instance._buttons[i].onClick.AddListener(() => UIManagment.Instance.OnButtonClick(index));
            }

            UIManagment.Instance.queryCalled = true;
        }

    }
 
  public void EndGame()
    {
        score = UIManagment.Instance.score;
        Debug.Log("Has ganado");
        int usuarioId = SupabaseManager.CurrentUserId;
        int categoriaId = TriviaSelection.SelectedTriviaId;
        int puntajeFinal = UIManagment.Instance.score;

       

        // Llamar al método para guardar en Supabase
        GuardarIntentoEnSupabase(usuarioId, categoriaId, puntajeFinal);
       
        SceneManager.LoadScene("EscenaGanar");
        _puntosText.text = puntajeFinal.ToString("f0");  
    }
     public async void GuardarIntentoEnSupabase(int usuarioId, int categoriaId, int puntajeFinal)
    {
        // Consultar el último id utilizado (ID = index)
        var ultimoId = await clientSupabase
            .From<intentos>()
            .Select("id")
            .Order(intentos => intentos.id, Postgrest.Constants.Ordering.Descending) // Ordenar en orden descendente para obtener el último id
            .Get();

        int nuevoId = 1; // Valor predeterminado si la tabla está vacía

        if (ultimoId.Models.Count > 0)
        {
            nuevoId = ultimoId.Models[0].id + 1; // Incrementar el último id
        }

        // Crear un nuevo intento con los datos obtenidos
        var nuevoIntento = new intentos
        {
            id = nuevoId,
            id_usuario = usuarioId,
            categoria = categoriaId,
            puntaje = puntajeFinal
        };

        // Insertar el nuevo intento en Supabase
        var resultado = await clientSupabase
            .From<intentos>()
            .Insert(new[] { nuevoIntento });

        if (resultado.ResponseMessage.IsSuccessStatusCode)
        {
            Debug.Log("Intento guardado correctamente en Supabase.");
        }
        else
        {
            Debug.LogError("Error al guardar el intento en Supabase: " + resultado.ResponseMessage);
        }  
}
    
}