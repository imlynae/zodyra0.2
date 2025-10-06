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

    private Renderer ringRenderer;
    private AudioSource audioSource;

    public Vector3 InitialScale => initialScale;
    public bool IsShrinking => isShrinking;
    public float ShrinkProgress => currentProgress;
    public float ShrinkTimer => shrinkTimer;
    public float TimeRemaining => isShrinking ? (shrinkDuration - shrinkTimer) : 0f;
    public bool CanShrink => shrinkDuration > 0.1f;

    public System.Action OnShrinkStart;
    public System.Action OnShrinkComplete;
    public System.Action<float> OnShrinkProgress;

    void Start()
    {
        initialScale = transform.localScale;
        ringRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.spatialBlend = 1f; 
            audioSource.volume = 0.7f;
        }

        Debug.Log($"{gameObject.name} inicializado. Scale: {initialScale}, Target: {targetScale}, Duration: {shrinkDuration}s");
    }

    void Update()
    {
        if (shrinkDuration < 0.1f || !isShrinking) return;

        shrinkTimer += Time.deltaTime;

        currentProgress = Mathf.Clamp01(shrinkTimer / shrinkDuration);
        float newScaleX = Mathf.SmoothStep(initialScale.x, targetScale, currentProgress);
        float newScaleZ = Mathf.SmoothStep(initialScale.z, targetScale, currentProgress);
        transform.localScale = new Vector3(newScaleX, transform.localScale.y, newScaleZ);
        OnShrinkProgress?.Invoke(currentProgress);
        if (currentProgress >= 1f)
        {
            CompleteShrinking();
        }
    }

    public void StartShrinking()
    {
        if (!isShrinking && shrinkDuration > 0.1f)
        {
            isShrinking = true;
            shrinkTimer = 0f;
            currentProgress = 0f;

            PlayShrinkEffects();


            OnShrinkStart?.Invoke();

            Debug.Log($" {gameObject.name} iniciou encolhimento: {initialScale.x} → {targetScale} em {shrinkDuration}s");
        }
        else if (shrinkDuration < 0.1f)
        {
            Debug.LogWarning($"{gameObject.name} não pode encolher (shrinkDuration muito baixo: {shrinkDuration})");
        }
    }
    public void StopShrinking()
    {
        if (isShrinking)
        {
            isShrinking = false;
            Debug.Log($" {gameObject.name} parou de encolher. Progresso: {currentProgress * 100:F0}%");
        }
    }
    public void ResumeShrinking()
    {
        if (!isShrinking && currentProgress < 1f && shrinkDuration > 0.1f)
        {
            isShrinking = true;
            Debug.Log($" {gameObject.name} continuando encolhimento...");
        }
    }

    private void CompleteShrinking()
    {
        isShrinking = false;

        transform.localScale = new Vector3(targetScale, transform.localScale.y, targetScale);

        if (ringRenderer != null && normalMaterial != null)
            ringRenderer.material = normalMaterial;

        Debug.Log($" {gameObject.name} terminou de encolher. Escala final: {transform.localScale.x:F2}");

        OnShrinkComplete?.Invoke();
    }

    public void ResetRing()
    {
        isShrinking = false;
        shrinkTimer = 0f;
        currentProgress = 0f;
        transform.localScale = initialScale;

        if (ringRenderer != null && normalMaterial != null)
            ringRenderer.material = normalMaterial;

        Debug.Log($"🔄 {gameObject.name} resetado para escala inicial: {initialScale}");
    }

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
            Debug.Log($"{gameObject.name} novo target: {newTargetScale}");
        }
    }

    private void PlayShrinkEffects()
    {
        if (ringRenderer != null && shrinkingMaterial != null)
            ringRenderer.material = shrinkingMaterial;

        if (audioSource != null && shrinkSound != null)
        {
            audioSource.clip = shrinkSound;
            audioSource.Play();
        }

        if (shrinkParticles != null)
            shrinkParticles.Play();
    }

    public string GetRingStatus()
    {
        string status = $"{gameObject.name}: ";

        if (shrinkDuration < 0.1f)
        {
            status += " FIXO (não encolhe)";
        }
        else if (!isShrinking && currentProgress >= 1f)
        {
            status += $" FINALIZADO ({transform.localScale.x:F2})";
        }
        else if (!isShrinking)
        {
            status += " PARADO";
        }
        else
        {
            status += $" ENCOLHENDO {currentProgress * 100:F0}% ({TimeRemaining:F1}s restantes)";
        }

        return status;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isShrinking ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, $"Scale: {transform.localScale.x:F2}\nProgress: {currentProgress * 100:F0}%");
#endif
    }
}