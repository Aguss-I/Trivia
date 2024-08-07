using UnityEngine;
using Supabase;
using System.Threading.Tasks;
using Supabase.Interfaces;
using Postgrest.Models;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class RankingManager : MonoBehaviour
{
    string supabaseUrl = "https://jenevwbrobsgzfzlehpm.supabase.co";
    string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImplbmV2d2Jyb2JzZ3pmemxlaHBtIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MTk5NTA2MzUsImV4cCI6MjAzNTUyNjYzNX0.1n5hSVIr6kJC6XmiWiTpp4jnrLFeitIsPWeV0sPHC_Y";
    public Supabase.Client clientSupabase;

    List<trivia> trivias = new List<trivia>();
    List<intentos> intentoList = new List<intentos>();
    List<usuarios> usuariosList = new List<usuarios>();

    [SerializeField] TMP_Dropdown _dropdown;

    [SerializeField] TMP_Text generalRankingText; // Referencia al objeto TextMeshPro para mostrar ranking general 
    [SerializeField] TMP_Text categoryRankingText;
    // Referencia al objeto TextMeshPro para mostrar ranking por categoría

    public static int SelectedTriviaId { get; private set; }
    public static RankingManager Instance { get; private set; }

    public DatabaseManager databaseManager;

    async void Start()
    {
        Instance = this;
        clientSupabase = new Supabase.Client(supabaseUrl, supabaseKey);

        await SelectTrivias();
        PopulateDropdown();

        await LoadIntentoData();
        await LoadUsuariosData();

        // Suscribirse al evento OnValueChanged del Dropdown
        _dropdown.onValueChanged.AddListener(OnCategoryDropdownValueChanged);

        // Mostrar el ranking general al inicio
        ShowGeneralRanking();
    }

    async Task SelectTrivias()
    {
        var response = await clientSupabase
            .From<trivia>()
            .Select("*")
            .Get();

        if (response != null)
        {
            trivias = response.Models;
        }
    }

    async Task LoadIntentoData()
    {
        var response = await clientSupabase
            .From<intentos>()
            .Select("*")
            .Get();

        if (response != null)
        {
            intentoList = response.Models.ToList();
        }
    }

    async Task LoadUsuariosData()
    {
        var response = await clientSupabase
            .From<usuarios>()
            .Select("*")
            .Get();

        if (response != null)
        {
            usuariosList = response.Models.ToList();
        }
    }

    void PopulateDropdown()
    {
        _dropdown.ClearOptions();

        List<string> categories = new List<string>();

        foreach (var trivia in trivias)
        {
            categories.Add(trivia.category);
        }

        _dropdown.AddOptions(categories);
    }

    void ShowGeneralRanking()
    {
        var sortedUsers = intentoList.OrderByDescending(x => x.puntaje).Take(7);;

    // Construir el texto para mostrar en el ranking general
    string generalRankingText = "Usuario          score\n";
    foreach (var intento in sortedUsers)
    {
        var user = usuariosList.FirstOrDefault(u => u.id == intento.id_usuario);
        if (user != null)
        {
            // Utilizar interpolación de cadenas para formatear correctamente
            generalRankingText += $"{user.username,-20} {intento.puntaje}\n"; // Ajustar el número según la longitud máxima esperada de nombres
        }
    }

        // Mostrar el texto en el UI
        this.generalRankingText.text = generalRankingText;
    }

    void ShowCategoryRanking(string category)
    {
        // Obtener la categoría seleccionada del Dropdown
        var selectedCategory = trivias.FirstOrDefault(t => t.category == category);

        if (selectedCategory != null)
        {
            // Filtrar los intentos por la categoría seleccionada y ordenar por score descendente
            var categoryUsers = intentoList.Where(x => x.categoria == selectedCategory.id).OrderByDescending(x => x.puntaje).Take(7);;

            // Construir el texto para mostrar en el ranking por categoría
          string categoryRankingText = "Usuario            score\n";
        foreach (var intento in categoryUsers)
        {
            var user = usuariosList.FirstOrDefault(u => u.id == intento.id_usuario);
            if (user != null)
            {
                // Ajustar la alineación usando espacios o tabulaciones según la longitud del nombre
                int spacesCount = 20 - user.username.Length; // Espacios para alinear
                categoryRankingText += $"{user.username}{new string(' ', spacesCount)}{intento.puntaje}\n";
            }
        }


            // Mostrar el texto en el UI
            this.categoryRankingText.text = categoryRankingText;
        }
    }

    // Método llamado cuando se cambia la selección en el Dropdown
    void OnCategoryDropdownValueChanged(int index)
    {
        string selectedCategory = _dropdown.options[index].text;
        ShowCategoryRanking(selectedCategory);
    }


     public void ChangeScene(string name)
    {
        SceneManager.LoadScene(name);
    }

}