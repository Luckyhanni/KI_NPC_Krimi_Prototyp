using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueTurnResult
{
    public string npcId;
    public string npcDisplayName;
    public string playerInput;
    public string npcResponse;
    public string prompt;
    public string stateSummary;
    public string allowedKnowledgeSummary;
    public string constraintsSummary;
}

[Serializable]
public class DialogueTurnResultEvent : UnityEvent<DialogueTurnResult>
{
}

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private string currentNpcId = "clara";
    [SerializeField] private GameState gameState = new GameState();

    public readonly Dictionary<string, NpcProfile> profiles = new Dictionary<string, NpcProfile>();
    public readonly Dictionary<string, NpcMemory> memories = new Dictionary<string, NpcMemory>();

    public UnityEvent<string> onNpcSelected = new UnityEvent<string>();
    public DialogueTurnResultEvent onDialogueTurnCompleted = new DialogueTurnResultEvent();

    public event Action<string> NpcSelected;
    public event Action<DialogueTurnResult> DialogueTurnCompleted;

    private PromptBuilder promptBuilder;
    private DummyDialogueResponder dummyResponder;
    private DialogueLogger dialogueLogger;

    public string CurrentNpcId => currentNpcId;
    public GameState State => gameState;

    private void Awake()
    {
        promptBuilder = new PromptBuilder();
        dummyResponder = new DummyDialogueResponder();
        dialogueLogger = new DialogueLogger();
        CreateNpcProfiles();
        EnsureMemories();
    }

    private void Start()
    {
        SelectNpc(currentNpcId);
    }

    public NpcProfile GetCurrentProfile()
    {
        if (profiles.TryGetValue(currentNpcId, out NpcProfile profile))
        {
            return profile;
        }

        return null;
    }

    public void SelectNpc(string npcId)
    {
        if (!profiles.ContainsKey(npcId))
        {
            Debug.LogWarning("Unbekannte NPC-ID: " + npcId);
            return;
        }

        currentNpcId = npcId;
        NpcProfile profile = profiles[currentNpcId];
        string label = profile.displayName;

        Debug.Log("Aktiver NPC: " + label + " (" + currentNpcId + ")");
        onNpcSelected.Invoke(label);
        NpcSelected?.Invoke(label);
    }

    public DialogueTurnResult SendPlayerInput(string input)
    {
        NpcProfile profile = GetCurrentProfile();
        if (profile == null)
        {
            Debug.LogError("Kein aktives NPC-Profil gefunden.");
            return null;
        }

        NpcMemory memory = memories[currentNpcId];
        string safeInput = input ?? string.Empty;
        string prompt = promptBuilder.BuildPrompt(profile, gameState, memory, safeInput);
        string response = dummyResponder.GenerateDummyResponse(profile, safeInput, gameState);

        if (!string.IsNullOrWhiteSpace(safeInput))
        {
            memory.AddTurn(safeInput, response);
            ApplySimpleStateProgression(currentNpcId, safeInput);
        }

        DialogueTurnResult result = new DialogueTurnResult
        {
            npcId = profile.id,
            npcDisplayName = profile.displayName,
            playerInput = safeInput,
            npcResponse = response,
            prompt = prompt,
            stateSummary = gameState.GetActiveStateSummary(),
            allowedKnowledgeSummary = string.Join(", ", profile.allowedKnowledge),
            constraintsSummary = string.Join(", ", profile.constraints)
        };

        dialogueLogger.LogTurn(profile, gameState, safeInput, prompt, response);
        onDialogueTurnCompleted.Invoke(result);
        DialogueTurnCompleted?.Invoke(result);

        return result;
    }

    private void CreateNpcProfiles()
    {
        profiles.Clear();

        profiles.Add("clara", new NpcProfile
        {
            id = "clara",
            displayName = "Clara Weber",
            role = "Haushaelterin, Taeterfigur mit internem Taeterwissen",
            personality = "Hoeflich, kontrolliert, pflichtbewusst, innerlich angespannt",
            motivation = "Die eigene Beteiligung verschleiern und den Haushalt nach aussen geordnet wirken lassen",
            speechStyle = "Formell, ruhig, ausweichend, mit vorsichtigen Korrekturen",
            allowedKnowledge = new List<string>
            {
                "Die Hintertuer wurde am Abend benutzt.",
                "Es gab einen Drohbrief.",
                "Viktor hatte kurz vor seinem Tod Streit mit mehreren Personen."
            },
            blockedKnowledge = new List<string>
            {
                "Eigene Beteiligung",
                "Fallaufloesung vor caseSolved"
            },
            evasionStrategies = new List<string>
            {
                "Nicht direkt luegen, aber auf Haushaltsroutinen und Missverstaendnisse ausweichen.",
                "Bei Schuldfragen ruhig bleiben und Anschuldigungen als voreilig darstellen."
            },
            constraints = new List<string>
            {
                "Keine Taeterdetails preisgeben, solange caseSolved false ist.",
                "Keine Informationen ausserhalb des erlaubten Wissens behaupten.",
                "Antworten kurz und im Charakter halten."
            }
        });

        profiles.Add("anton", new NpcProfile
        {
            id = "anton",
            displayName = "Anton Stein",
            role = "Neffe, falsche Faehrte",
            personality = "Nervoes, gereizt, defensiv, schnell gekraenkt",
            motivation = "Die eigenen Schulden herunterspielen und nicht als Hauptverdaechtiger gelten",
            speechStyle = "Unruhig, abwehrend, kurze Rechtfertigungen",
            allowedKnowledge = new List<string>
            {
                "Anton hatte Schulden.",
                "Anton hatte Streit mit Viktor.",
                "Viktor setzte Familienmitglieder unter Druck."
            },
            blockedKnowledge = new List<string>
            {
                "Taeteridentitaet"
            },
            evasionStrategies = new List<string>
            {
                "Verdacht als ungerecht darstellen.",
                "Auf andere moegliche Konflikte im Haus verweisen."
            },
            constraints = new List<string>
            {
                "Keine sichere Taeteridentitaet nennen.",
                "Schulden nicht vollstaendig abstreiten.",
                "Defensiv, aber nicht allwissend antworten."
            }
        });

        profiles.Add("mira", new NpcProfile
        {
            id = "mira",
            displayName = "Mira Feld",
            role = "Nachbarin und Zeugin",
            personality = "Vorsichtig, beobachtend, unsicher, detailorientiert",
            motivation = "Helfen, ohne jemanden falsch zu beschuldigen",
            speechStyle = "Zurueckhaltend, abwaegend, mit Unsicherheitsmarkern",
            allowedKnowledge = new List<string>
            {
                "In der Tatnacht brannte Licht im Arbeitszimmer.",
                "Mira hoerte Geraeusche in der Nacht.",
                "Mira beobachtete nur Ausschnitte von aussen."
            },
            blockedKnowledge = new List<string>
            {
                "Sichere Taeteridentitaet"
            },
            evasionStrategies = new List<string>
            {
                "Unsicherheit klar benennen.",
                "Nur Beobachtungen, keine Schlussfolgerungen als Fakten formulieren."
            },
            constraints = new List<string>
            {
                "Keine sichere Taeteridentitaet behaupten.",
                "Beobachtungen vorsichtig formulieren.",
                "Keine Ereignisse schildern, die Mira nicht gesehen oder gehoert haben kann."
            }
        });
    }

    private void EnsureMemories()
    {
        foreach (string npcId in profiles.Keys)
        {
            if (!memories.ContainsKey(npcId))
            {
                memories.Add(npcId, new NpcMemory(npcId));
            }
        }
    }

    private void ApplySimpleStateProgression(string npcId, string input)
    {
        string normalized = input.ToLowerInvariant();

        if (npcId == "clara" && ContainsAny(normalized, "alibi"))
        {
            gameState.hasQuestionedClaraAlibi = true;
        }

        if (npcId == "mira" && ContainsAny(normalized, "nacht", "gesehen", "licht", "geraeusch", "geräusch"))
        {
            gameState.hasAskedMiraAboutNight = true;
        }

        if (ContainsAny(normalized, "schluessel", "schlüssel", "hintertuer", "hintertür"))
        {
            gameState.hasFoundBrokenKey = true;
        }

        if (ContainsAny(normalized, "brief", "drohbrief"))
        {
            gameState.hasFoundBurnedLetter = true;
        }

        if (ContainsAny(normalized, "schulden", "schuldschein"))
        {
            gameState.hasFoundDebtNote = true;
        }

        if (ContainsAny(normalized, "wein", "analyse", "analysiert"))
        {
            gameState.hasAnalyzedWine = true;
        }

        if (ContainsAny(normalized, "ich konfrontiere", "konfrontiere"))
        {
            gameState.confrontedNpcId = npcId;
        }
    }

    private static bool ContainsAny(string input, params string[] needles)
    {
        foreach (string needle in needles)
        {
            if (input.Contains(needle))
            {
                return true;
            }
        }

        return false;
    }
}
