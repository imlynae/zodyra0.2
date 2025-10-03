using UnityEngine;

public class RingController : MonoBehaviour
{
    [Header("Configurações do Anel")]
    public float shrinkDuration = 30f;
    public float targetScale = 0.7f;

    [Header("Materiais e Efeitos (Opcionais)")]
    public Material normalMaterial;
    public Material shrinkingMaterial;
    public ParticleSystem shrinkParticles;
    public AudioClip shrinkSound;

    [Header("Estado do Anel")]
    [SerializeField] private Vector3 initialScale;
    [SerializeField] private bool isShrinking = false;
    [SerializeField] private float shrinkTimer;
    [SerializeField] private float currentProgress;

    // Componentes internos
    private Renderer ringRenderer;
    private AudioSource audioSource;

    // Propriedades públicas para acesso externo
    public Vector3 InitialScale => initialScale;
    public bool IsShrinking => isShrinking;
    public float ShrinkProgress => currentProgress;
    public float ShrinkTimer => shrinkTimer;
    public float TimeRemaining => isShrinking ? (shrinkDuration - shrinkTimer) : 0f;
    public bool CanShrink => shrinkDuration > 0.1f;

    // Eventos para outros scripts
    public System.Action OnShrinkStart;
    public System.Action OnShrinkComplete;
    public System.Action<float> OnShrinkProgress;

    void Start()
    {
        // Guarda a escala inicial
        initialScale = transform.localScale;

        // Obtém componentes
        ringRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        // Se não tem AudioSource, adiciona um
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Configura audio source
        if (audioSource != null)
        {
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.volume = 0.7f;
        }

        Debug.Log($"{gameObject.name} inicializado. Scale: {initialScale}, Target: {targetScale}, Duration: {shrinkDuration}s");
    }

    void Update()
    {
        // Se não tem duração definida ou não está encolhendo, ignora
        if (shrinkDuration < 0.1f || !isShrinking) return;

        // Incrementa o timer
        shrinkTimer += Time.deltaTime;

        // Calcula a porcentagem de conclusão (de 0 a 1)
        currentProgress = Mathf.Clamp01(shrinkTimer / shrinkDuration);

        // Interpola suavemente a escala do initialScale até o targetScale
        float newScaleX = Mathf.SmoothStep(initialScale.x, targetScale, currentProgress);
        float newScaleZ = Mathf.SmoothStep(initialScale.z, targetScale, currentProgress);

        // Aplica a nova escala (mantém a escala Y original para espessura)
        transform.localScale = new Vector3(newScaleX, transform.localScale.y, newScaleZ);

        // Notifica sobre o progresso
        OnShrinkProgress?.Invoke(currentProgress);

        // Verifica se terminou de encolher
        if (currentProgress >= 1f)
        {
            CompleteShrinking();
        }
    }

    // Método para iniciar a diminuição deste anel
    public void StartShrinking()
    {
        if (!isShrinking && shrinkDuration > 0.1f)
        {
            isShrinking = true;
            shrinkTimer = 0f;
            currentProgress = 0f;

            // Feedback visual/auditivo
            PlayShrinkEffects();

            // Evento de início
            OnShrinkStart?.Invoke();

            Debug.Log($"🔻 {gameObject.name} iniciou encolhimento: {initialScale.x} → {targetScale} em {shrinkDuration}s");
        }
        else if (shrinkDuration < 0.1f)
        {
            Debug.LogWarning($"⚠️ {gameObject.name} não pode encolher (shrinkDuration muito baixo: {shrinkDuration})");
        }
    }

    // Método para parar o encolhimento (útil para pausas)
    public void StopShrinking()
    {
        if (isShrinking)
        {
            isShrinking = false;
            Debug.Log($"⏸️ {gameObject.name} parou de encolher. Progresso: {currentProgress * 100:F0}%");
        }
    }

    // Método para continuar o encolhimento após parar
    public void ResumeShrinking()
    {
        if (!isShrinking && currentProgress < 1f && shrinkDuration > 0.1f)
        {
            isShrinking = true;
            Debug.Log($"▶️ {gameObject.name} continuando encolhimento...");
        }
    }

    // Finaliza o encolhimento
    private void CompleteShrinking()
    {
        isShrinking = false;

        // Garante escala final exata
        transform.localScale = new Vector3(targetScale, transform.localScale.y, targetScale);

        // Feedback visual de conclusão
        if (ringRenderer != null && normalMaterial != null)
            ringRenderer.material = normalMaterial;

        Debug.Log($"✅ {gameObject.name} terminou de encolher. Escala final: {transform.localScale.x:F2}");

        // Evento de conclusão
        OnShrinkComplete?.Invoke();
    }

    // Método para resetar o anel ao estado inicial
    public void ResetRing()
    {
        isShrinking = false;
        shrinkTimer = 0f;
        currentProgress = 0f;
        transform.localScale = initialScale;

        // Reset visual
        if (ringRenderer != null && normalMaterial != null)
            ringRenderer.material = normalMaterial;

        Debug.Log($"🔄 {gameObject.name} resetado para escala inicial: {initialScale}");
    }

    // Configurações dinâmicas (úteis para balanceamento em tempo real)
    public void SetShrinkDuration(float newDuration)
    {
        if (newDuration > 0)
        {
            float progressBefore = currentProgress;
            shrinkDuration = newDuration;
            shrinkTimer = progressBefore * newDuration;
            Debug.Log($"⏱️ {gameObject.name} nova duração: {newDuration}s");
        }
    }

    public void SetTargetScale(float newTargetScale)
    {
        if (newTargetScale > 0 && newTargetScale <= initialScale.x)
        {
            targetScale = newTargetScale;
            Debug.Log($"🎯 {gameObject.name} novo target: {newTargetScale}");
        }
    }

    // Feedback visual e auditivo
    private void PlayShrinkEffects()
    {
        // Muda material para indicar que está encolhendo
        if (ringRenderer != null && shrinkingMaterial != null)
            ringRenderer.material = shrinkingMaterial;

        // Toca som de encolhimento
        if (audioSource != null && shrinkSound != null)
        {
            audioSource.clip = shrinkSound;
            audioSource.Play();
        }

        // Ativa partículas
        if (shrinkParticles != null)
            shrinkParticles.Play();
    }

    // Método para debug rápido
    public string GetRingStatus()
    {
        string status = $"{gameObject.name}: ";

        if (shrinkDuration < 0.1f)
        {
            status += "🔒 FIXO (não encolhe)";
        }
        else if (!isShrinking && currentProgress >= 1f)
        {
            status += $"✅ FINALIZADO ({transform.localScale.x:F2})";
        }
        else if (!isShrinking)
        {
            status += "⏸️ PARADO";
        }
        else
        {
            status += $"🔻 ENCOLHENDO {currentProgress * 100:F0}% ({TimeRemaining:F1}s restantes)";
        }

        return status;
    }

    // Gizmos para visualização no Editor
    void OnDrawGizmosSelected()
    {
        // Desenha esfera na posição do anel (útil para debug)
        Gizmos.color = isShrinking ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Mostra escala atual como texto (apenas no Editor)
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, $"Scale: {transform.localScale.x:F2}\nProgress: {currentProgress * 100:F0}%");
#endif
    }
}