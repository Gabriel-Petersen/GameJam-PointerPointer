using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class MenuReturn : MonoBehaviour
{
    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(Return); 
    }
    
    public void Return ()
    {
        SceneManager.LoadScene("Menu");
    }
}