using UnityEngine;
using UnityEngine.UI; // Necessário para componentes de UI como Image
using UnityEngine.EventSystems; // Necessário para as interfaces de eventos

// 1. Adicionamos as interfaces IPointerEnterHandler e IPointerExitHandler
public class TrocaSpriteHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 2. Crie campos para arrastar seus sprites no Inspector
    [Header("Sprites do Botão")]
    [SerializeField] private Sprite spriteNormal;
    [SerializeField] private Sprite spriteDestacado; // O seu sprite amarelo

    // 3. Referência para o componente Image do próprio botão
    private Image botaoImage;

    // Awake é chamado antes do Start
    void Awake()
    {
        // Pega o componente Image deste objeto
        botaoImage = GetComponent<Image>();
        
        // Garante que o botão comece com o sprite normal
        if (botaoImage != null && spriteNormal != null)
        {
            botaoImage.sprite = spriteNormal;
        }
    }

    // 4. Esta função é chamada quando o cursor do mouse ENTRA no botão
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Se temos um sprite destacado, mude para ele
        if (botaoImage != null && spriteDestacado != null)
        {
            botaoImage.sprite = spriteDestacado;
        }
    }

    // 5. Esta função é chamada quando o cursor do mouse SAI do botão
    public void OnPointerExit(PointerEventData eventData)
    {
        // Se temos um sprite normal, volte para ele
        if (botaoImage != null && spriteNormal != null)
        {
            botaoImage.sprite = spriteNormal;
        }
    }
}