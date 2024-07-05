using UnityEngine;
using Supabase;
using Supabase.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Postgrest.Models;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TriviaSelection : MonoBehaviour
{
    string supabaseUrl = "https://jenevwbrobsgzfzlehpm.supabase.co"; //COMPLETAR
    string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImplbmV2d2Jyb2JzZ3pmemxlaHBtIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MTk5NTA2MzUsImV4cCI6MjAzNTUyNjYzNX0.1n5hSVIr6kJC6XmiWiTpp4jnrLFeitIsPWeV0sPHC_Y"; //COMPLETAR

    Supabase.Client clientSupabase;
    public static int SelectedTriviaId { get; private set; } // Variable para almacenar el id de la trivia seleccionada
    public static TriviaSelection Instance { get; private set; } // Propiedad estática para acceder a la instancia única de TriviaSelection

    List<trivia> trivias = new List<trivia>();
    [SerializeField] TMP_Dropdown _dropdown;

    public DatabaseManager databaseManager;

    async void Start()
    {
        clientSupabase = new Supabase.Client(supabaseUrl, supabaseKey);

        await SelectTrivias();
        PopulateDropdown();
        Instance = this;
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
            //Debug.Log("Trivias seleccionadas: " + trivias.Count);
            //foreach (var trivia in trivias)
            //{
            //    Debug.Log("ID: " + trivia.id + ", Categor�a: " + trivia.category);
            //}
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

    public void OnStartButtonClicked()
    {
        int selectedIndex = _dropdown.value;
        string selectedTrivia = _dropdown.options[selectedIndex].text;
        SelectedTriviaId = trivias[selectedIndex].id;


        PlayerPrefs.SetInt("SelectedIndex", selectedIndex+1);
        PlayerPrefs.SetString("SelectedTrivia", selectedTrivia);


        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
