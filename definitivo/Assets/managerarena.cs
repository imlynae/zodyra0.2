using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ArenaManager : MonoBehaviour
{
    [Header("Configuracoes de Tempo")]
    public float initialSafeTime = 30f;
    public float totalGameTime = 80f;
    public float ringShrinkDuration = 20f;
    public float delayBetweenRings = 5f;

    [Header("UI Reference")]
    public TextMeshProUGUI timerText;
    public List<RingController> rings;

    [Header("Debug")]
    [SerializeField] private int currentRingIndex = -1;
    [SerializeField] private float gameTimer;
    [SerializeField] private bool isGameActive = true;
    [SerializeField] private float timeRemaining;
    [SerializeField] private string currentPhase = "Fase Inicial";

    void Start()
    {
        foreach (RingController ring in rings)
        {
            ring.OnShrinkComplete += HandleRingShrinkComplete;
        }

        gameTimer = 0f;
        timeRemaining = totalGameTime;
        currentRingIndex = -1;
        isGameActive = true;
        currentPhase = "Fase Inicial";

        Debug.Log(" Cronograma: " + initialSafeTime + "s inicial + delays de " + delayBetweenRings + "s entre aneis");
        UpdateTimerUI();
    }

    void Update()
    {
        if (!isGameActive) return;

        gameTimer += Time.deltaTime;
        timeRemaining = totalGameTime - gameTimer;

        UpdateTimerUI();

        // Inicia primeiro anel apos tempo seguro
        if (currentRingIndex == -1 && gameTimer >= initialSafeTime)
        {
            currentRingIndex = 0;
            currentPhase = "Anel 1 Encolhendo";
            if (rings[0].CanShrink)
            {
                rings[0].StartShrinking();
                Debug.Log(" ANEL 1 INICIADO! " + ringShrinkDuration + "s de encolhimento");
            }
        }

        // Fim de jogo
        if (timeRemaining <= 0f && isGameActive)
        {
            EndGame();
        }
    }

    void HandleRingShrinkComplete()
    {
        if (!isGameActive) return;

        Debug.Log(" Anel " + (currentRingIndex + 1) + " completou em " + gameTimer.ToString("F1") + "s");

        currentRingIndex++;

        if (currentRingIndex == 1 && rings[1].CanShrink)
        {
            // AGORA COM DELAY
            StartCoroutine(StartNextRingWithDelay());
        }
        else if (currentRingIndex == 2)
        {
            currentPhase = "Fase Final - Area Segura";
            Debug.Log(" AREA SEGURA ESTABELECIDA! " + timeRemaining.ToString("F0") + "s de batalha final");
        }
    }

    // METODO NOVO PARA DELAY ENTRE ANEIS
    IEnumerator StartNextRingWithDelay()
    {
        currentPhase = "Aguardando proximo anel... (" + delayBetweenRings + "s)";
        Debug.Log(" Aguardando " + delayBetweenRings + "s antes do proximo encolhimento");

        // Feedback visual no timer durante a espera
        if (timerText != null)
            timerText.text = " " + Mathf.CeilToInt(timeRemaining) + "s";

        yield return new WaitForSeconds(delayBetweenRings);

        currentPhase = "Anel 2 Encolhendo";
        rings[1].StartShrinking();
        Debug.Log(" ANEL 2 INICIADO apos " + delayBetweenRings + "s de espera");
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);

            string phaseIndicator = "";
            if (currentPhase.Contains("Aguardando")) phaseIndicator = "-";
            else if (currentPhase.Contains("Encolhendo")) phaseIndicator = "-";
            else if (currentPhase.Contains("Final")) phaseIndicator = "-";
            else phaseIndicator = "-";

            timerText.text = phaseIndicator + " " + seconds + "s";

            if (seconds <= 10)
            {
                timerText.color = Color.red;
                timerText.fontSize = 52;
            }
            else if (seconds <= 30)
            {
                timerText.color = Color.yellow;
                timerText.fontSize = 42;
            }
            else if (currentPhase.Contains("Aguardando"))
            {
                timerText.color = Color.blue;
                timerText.fontSize = 38;
            }
            else if (currentPhase.Contains("Encolhendo"))
            {
                timerText.color = Color.cyan;
                timerText.fontSize = 38;
            }
            else
            {
                timerText.color = Color.white;
                timerText.fontSize = 36;
            }
        }
    }

    void EndGame()
    {
        isGameActive = false;
        currentPhase = "Fim de Jogo";

        StopAllCoroutines();

        foreach (RingController ring in rings)
        {
            if (ring.IsShrinking)
                ring.StopShrinking();
        }

        Debug.Log(" FIM DE JOGO aos " + gameTimer.ToString("F1") + "s");

        if (timerText != null)
        {
            timerText.text = "FIM!";
            timerText.color = Color.magenta;
        }

        StartCoroutine(EndGameSequence());
    }

    IEnumerator EndGameSequence()
    {
        yield return new WaitForSeconds(3f);
    }

    public string GetTimeline()
    {
        return "Timeline com Delays:\n" +
               "0-" + initialSafeTime + "s: Fase inicial\n" +
               initialSafeTime + "-" + (initialSafeTime + ringShrinkDuration) + "s: Anel 1 encolhe\n" +
               (initialSafeTime + ringShrinkDuration) + "-" + (initialSafeTime + ringShrinkDuration + delayBetweenRings) + "s: Espera de " + delayBetweenRings + "s\n" +
               (initialSafeTime + ringShrinkDuration + delayBetweenRings) + "-" + (initialSafeTime + ringShrinkDuration + delayBetweenRings + ringShrinkDuration) + "s: Anel 2 encolhe\n" +
               "Restante: Batalha final";
    }

    public void ResetArena()
    {
        StopAllCoroutines();
        currentRingIndex = -1;
        gameTimer = 0f;
        timeRemaining = totalGameTime;
        isGameActive = true;
        currentPhase = "Fase Inicial";

        foreach (RingController ring in rings)
        {
            ring.ResetRing();
        }

        UpdateTimerUI();
        Debug.Log("Arena resetada!");
    }
}