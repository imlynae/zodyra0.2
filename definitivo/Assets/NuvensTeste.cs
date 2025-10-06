using UnityEngine;

public class CloudShrinkAuto : MonoBehaviour
{
    [Header("Configurações de Encolhimento")]
    [Tooltip("Multiplicador de velocidade em relação ao anel (1 = igual, 1.3 = 30% mais rápido)")]
    public float shrinkSpeedMultiplier = 1.3f;

    [Tooltip("Escala mínima antes de desaparecer")]
    public float scaleMultiplier = 0.05f;

    [Tooltip("Destruir quando o anel terminar de encolher?")]
    public bool destroyWhenRingDone = true;

    [Tooltip("Distância máxima para detectar o anel abaixo")]
    public float maxDetectionDistance = 100f;

    private RingController ring;
    private Vector3 initialScale;
    private bool hasDestroyed;

    void Start()
    {
        initialScale = transform.localScale;
        ring = FindRingBelow();
    }

    void Update()
    {
        if (ring == null) return;

        // Acelera o progresso da nuvem
        float acceleratedProgress = Mathf.Clamp01(ring.ShrinkProgress * shrinkSpeedMultiplier);

        // Aplica uma curva suave (ease-out)
        float t = Mathf.SmoothStep(0f, 1f, acceleratedProgress);

        // Interpola a escala
        float newScale = Mathf.Lerp(1f, scaleMultiplier, t);
        transform.localScale = initialScale * newScale;

        // Destroi quando o anel termina (opcional)
        if (destroyWhenRingDone && !ring.IsShrinking && ring.ShrinkProgress >= 1f && !hasDestroyed)
        {
            hasDestroyed = true;
            Destroy(gameObject);
        }
    }

    private RingController FindRingBelow()
    {
        RaycastHit[] hits = Physics.RaycastAll(
            transform.position + Vector3.up * 10f,
            Vector3.down,
            maxDetectionDistance
        );

        float closestDist = float.MaxValue;
        RingController closestRing = null;

        foreach (var hit in hits)
        {
            var ringCandidate = hit.collider.GetComponent<RingController>();
            if (ringCandidate != null && hit.distance < closestDist)
            {
                closestDist = hit.distance;
                closestRing = ringCandidate;
            }
        }

        if (closestRing != null)
        {
            Debug.Log($"{name} vinculado automaticamente a {closestRing.name}");
        }
        else
        {
            Debug.LogWarning($"{name} não encontrou anel abaixo!");
        }

        return closestRing;
    }

    // Caso você queira vincular manualmente via código (ex: ao instanciar)
    public void AssignRing(RingController r)
    {
        ring = r;
    }
}
