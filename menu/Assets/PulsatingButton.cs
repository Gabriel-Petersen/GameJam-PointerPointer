using UnityEngine;
using System.Collections; // Necessário para usar as Coroutines

public class PulsatingButton : MonoBehaviour
{
    // Escala inicial do botão
    public Vector3 initialScale = new Vector3(1f, 1f, 1f);
    // Escala máxima que o botão alcançará no pulso
    public Vector3 maxScale = new Vector3(1.1f, 1.1f, 1.1f);
    // Duração de cada pulso (ida e volta)
    public float pulseDuration = 1.0f;

    void Start()
    {
        // Garante que a escala inicial seja definida ao iniciar
        transform.localScale = initialScale;
        // Inicia a coroutine de pulsação
        StartCoroutine(Pulse());
    }

    IEnumerator Pulse()
    {
        while (true) // Loop infinito para que o pulso continue
        {
            // Aumenta a escala do botão
            yield return ScaleOverTime(initialScale, maxScale, pulseDuration / 2);

            // Diminui a escala do botão de volta
            yield return ScaleOverTime(maxScale, initialScale, pulseDuration / 2);
        }
    }

    // Coroutine para escalar um objeto suavemente ao longo do tempo
    IEnumerator ScaleOverTime(Vector3 fromScale, Vector3 toScale, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime; // Incrementa o tempo
            // Calcula a proporção de tempo decorrido
            float progress = timer / duration;
            // Interpola linearmente entre a escala inicial e final
            transform.localScale = Vector3.Lerp(fromScale, toScale, progress);
            yield return null; // Espera um frame antes de continuar
        }
        // Garante que a escala final seja exatamente a desejada
        transform.localScale = toScale;
    }
}