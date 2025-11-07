
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Importe esta biblioteca para gerenciar cenas

public class MainMenuManager : MonoBehaviour
{

    // --- FUNÇÕES DOS BOTÕES DO MENU ---

    // Esta função será ligada ao botão "JOGAR"
    public void CarregarCenaJogo()
    {
        // Coloque o nome exato da sua cena de jogo principal
        // Ex: "Level_1", "Fase_1", "GameScene", etc.
        SceneManager.LoadScene("Gameplay"); 
    }

    // Esta função será ligada ao botão "OPCOES"
    public void CarregarCenaOpcoes()
    {
        // Coloque o nome exato da sua cena de Opções
        SceneManager.LoadScene("Lore");
    }

    // Esta função será ligada ao botão "TUTORIAL"
    public void CarregarCenaTutorial()
    {
        // Coloque o nome exato da sua cena de Tutorial
        SceneManager.LoadScene("Tutorial");
    }

    // Esta função será ligada ao botão "CREDITOS"
    public void CarregarCenaCreditos()
    {
        // Coloque o nome exato da sua cena de Créditos
        SceneManager.LoadScene("Creditos");
    }

    // --- FUNÇÃO EXTRA (MUITO ÚTIL) ---

    // Você pode usar esta função em um botão "Sair"
    // (Talvez dentro do menu de Opções)
    public void SairDoJogo()
    {
        // No editor da Unity, isso apenas mostrará um log.
        // No jogo compilado (PC), ele fechará o aplicativo.
        Debug.Log("SAINDO DO JOGO...");
        Application.Quit();
    }
}
