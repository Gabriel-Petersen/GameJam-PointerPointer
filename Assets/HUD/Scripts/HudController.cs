using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    private PlayerController player;
    [SerializeField] private Slider evilnessSlider;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;
    [SerializeField] private TextMeshProUGUI scoreTxt;
    [SerializeField] private TextMeshProUGUI speedTxt;
    [SerializeField] private TextMeshProUGUI tipoMaldade;

    private Camera mainCam;

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        speedTxt.gameObject.SetActive(false);
        evilnessSlider.maxValue = player.MaxEvil;
        mainCam = Camera.main;
    }

    void Update()
    {
        fill.color = gradient.Evaluate(evilnessSlider.normalizedValue);
        mainCam.transform.position = new(player.transform.position.x, player.transform.position.y, -10);
        evilnessSlider.value = player.totalEvilness;
        scoreTxt.text = $"Grana roubada: {player.totalScore} PatoCoin$";
        
        float baseSpeed = player.MoveSpeed - player.speedBonus;
        
        if (baseSpeed <= 0.001f)
        {
            speedTxt.gameObject.SetActive(false);
            return; 
        }

        if (player.speedBonus != 0)
        {
            speedTxt.gameObject.SetActive(true);
            float multiplier = player.MoveSpeed / baseSpeed;
            string multiplierText = multiplier.ToString("F2");

            if (player.speedBonus > 0)
            {
                speedTxt.color = Color.green;
                speedTxt.text = $"BÔNUS TURBO: {multiplierText}x";
            }
            else
            {
                speedTxt.color = Color.red;
                speedTxt.text = $"PESO MORTO: {multiplierText}x";
            }
        }
        else
        {
            speedTxt.gameObject.SetActive(false);
        }

        float perc = (float)player.totalEvilness / player.MaxEvil;
        if (perc < 0.1f)
            tipoMaldade.text = "Bonzinho";
        else if (perc < 0.3f)
            tipoMaldade.text = "Meio maligno...";
        else if (perc < 0.7f)
            tipoMaldade.text = "Meio maldoso";
        else
            tipoMaldade.text = "Tá na maldade!";
    }
}
