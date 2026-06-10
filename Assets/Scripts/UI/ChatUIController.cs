using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIController : MonoBehaviour
{
    private static readonly Color BackgroundColor = new Color(0.055f, 0.071f, 0.090f, 1f);
    private static readonly Color PanelColor = new Color(0.105f, 0.121f, 0.145f, 1f);
    private static readonly Color PanelColorAlt = new Color(0.135f, 0.151f, 0.176f, 1f);
    private static readonly Color TextColor = new Color(0.925f, 0.925f, 0.900f, 1f);
    private static readonly Color MutedTextColor = new Color(0.690f, 0.710f, 0.730f, 1f);
    private static readonly Color GoldColor = new Color(0.760f, 0.600f, 0.310f, 1f);
    private static readonly Color WineColor = new Color(0.360f, 0.080f, 0.115f, 1f);
    private static readonly Color ButtonColor = new Color(0.175f, 0.195f, 0.230f, 1f);
    private static readonly Color ButtonSelectedColor = new Color(0.420f, 0.110f, 0.140f, 1f);
    private const int MaxVisibleChatMessages = 8;

    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Button claraButton;
    [SerializeField] private Button antonButton;
    [SerializeField] private Button miraButton;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button resetCurrentMemoryButton;
    [SerializeField] private Button resetAllMemoriesButton;
    [SerializeField] private Button toggleDebugButton;
    [SerializeField] private Button dummyModeButton;
    [SerializeField] private Button apiModeButton;
    [SerializeField] private InputField inputField;
    [SerializeField] private InputField testCaseInputField;
    [SerializeField] private Text chatHistoryText;
    [SerializeField] private Text activeNpcText;
    [SerializeField] private Text debugText;
    [SerializeField] private Toggle hasFoundBrokenKeyToggle;
    [SerializeField] private Toggle hasFoundBurnedLetterToggle;
    [SerializeField] private Toggle hasFoundDebtNoteToggle;
    [SerializeField] private Toggle hasAnalyzedWineToggle;
    [SerializeField] private Toggle hasQuestionedClaraAlibiToggle;
    [SerializeField] private Toggle hasAskedMiraAboutNightToggle;
    [SerializeField] private Toggle caseSolvedToggle;
    [SerializeField] private Toggle autoStateToggle;

    private RectTransform debugPanel;
    private ScrollRect chatScrollRect;
    private RectTransform chatContentRect;
    private Image claraCardImage;
    private Image antonCardImage;
    private Image miraCardImage;
    private Image dummyModeButtonImage;
    private Image apiModeButtonImage;
    private readonly Dictionary<string, List<VisibleChatMessage>> visibleChatHistories = new Dictionary<string, List<VisibleChatMessage>>();
    private string activeNpcId = "clara";
    private bool debugVisible = true;
    private bool syncingStateToggles;

    private enum ChatMessageType
    {
        System,
        Player,
        Npc
    }

    private class VisibleChatMessage
    {
        public ChatMessageType type;
        public string speaker;
        public string text;
        public bool isTyping;
    }

    private void Awake()
    {
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
        }

        if (dialogueManager == null)
        {
            dialogueManager = gameObject.AddComponent<DialogueManager>();
        }

        EnsureUiExists();
        RegisterListeners();
        activeNpcId = GetCurrentNpcId();
        activeNpcText.text = "Aktiver NPC: " + GetNpcDisplayName(activeNpcId);
        RenderCurrentNpcChat();
        RefreshNpcSelectionVisuals(GetNpcDisplayName(activeNpcId));
        SyncStateTogglesFromManager();
        RefreshDebugSummary("Bereit.");
    }

    private void OnDestroy()
    {
        if (dialogueManager != null)
        {
            dialogueManager.DialogueTurnCompleted -= HandleDialogueTurnCompleted;
            dialogueManager.NpcSelected -= HandleNpcSelected;
            dialogueManager.StateChanged -= HandleStateChanged;
            dialogueManager.MemoryChanged -= HandleMemoryChanged;
            dialogueManager.ResponseModeChanged -= HandleResponseModeChanged;
        }
    }

    private void RegisterListeners()
    {
        claraButton.onClick.AddListener(() => dialogueManager.SelectNpc("clara"));
        antonButton.onClick.AddListener(() => dialogueManager.SelectNpc("anton"));
        miraButton.onClick.AddListener(() => dialogueManager.SelectNpc("mira"));
        sendButton.onClick.AddListener(SendCurrentInput);
        resetCurrentMemoryButton.onClick.AddListener(dialogueManager.ResetCurrentNpcMemory);
        resetAllMemoriesButton.onClick.AddListener(dialogueManager.ResetAllMemories);
        toggleDebugButton.onClick.AddListener(ToggleDebugPanel);
        dummyModeButton.onClick.AddListener(() => dialogueManager.SetResponseMode(ResponseMode.Dummy));
        apiModeButton.onClick.AddListener(() => dialogueManager.SetResponseMode(ResponseMode.Api));
        inputField.onSubmit.AddListener(_ => SendCurrentInput());

        RegisterStateToggle(hasFoundBrokenKeyToggle, "hasFoundBrokenKey");
        RegisterStateToggle(hasFoundBurnedLetterToggle, "hasFoundBurnedLetter");
        RegisterStateToggle(hasFoundDebtNoteToggle, "hasFoundDebtNote");
        RegisterStateToggle(hasAnalyzedWineToggle, "hasAnalyzedWine");
        RegisterStateToggle(hasQuestionedClaraAlibiToggle, "hasQuestionedClaraAlibi");
        RegisterStateToggle(hasAskedMiraAboutNightToggle, "hasAskedMiraAboutNight");
        RegisterStateToggle(caseSolvedToggle, "caseSolved");
        autoStateToggle.onValueChanged.AddListener(value =>
        {
            if (syncingStateToggles)
            {
                return;
            }

            dialogueManager.SetAutoStateProgression(value);
        });

        dialogueManager.DialogueTurnCompleted += HandleDialogueTurnCompleted;
        dialogueManager.NpcSelected += HandleNpcSelected;
        dialogueManager.StateChanged += HandleStateChanged;
        dialogueManager.MemoryChanged += HandleMemoryChanged;
        dialogueManager.ResponseModeChanged += HandleResponseModeChanged;
    }

    private void SendCurrentInput()
    {
        string input = inputField.text;
        string npcId = GetCurrentNpcId();
        string npcName = GetNpcDisplayName(npcId);

        if (dialogueManager.IsResponseInProgress || HasPendingTypingIndicator())
        {
            AddSystemMessage(npcId, GetPendingTypingNpcName() + " schreibt noch. Bitte warte kurz.");
            RenderCurrentNpcChat();
            return;
        }

        activeNpcId = npcId;
        if (!string.IsNullOrWhiteSpace(input))
        {
            AddChatMessage(npcId, ChatMessageType.Player, "Spieler", input, false);
        }

        AddTypingIndicator(npcId, npcName);
        RenderCurrentNpcChat();
        dialogueManager.SendPlayerInput(input, GetCurrentTestCaseId());
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    private void HandleNpcSelected(string displayName)
    {
        activeNpcId = GetCurrentNpcId();
        activeNpcText.text = "Aktiver NPC: " + displayName;
        RefreshNpcSelectionVisuals(displayName);
        RenderCurrentNpcChat();
        RefreshDebugSummary("NPC gewechselt.");
    }

    private void HandleDialogueTurnCompleted(DialogueTurnResult result)
    {
        RemoveTypingIndicator(result.npcId);
        AddChatMessage(result.npcId, ChatMessageType.Npc, result.npcDisplayName, result.npcResponse, false);
        RenderCurrentNpcChatIfActive(result.npcId);

        debugText.text =
            "NPC: " + result.npcDisplayName + "\n" +
            "Response Mode: " + result.responseMode + "\n" +
            "Prompt-Version: " + result.promptVersion + "\n" +
            "Testfall-ID: " + result.testCaseId + "\n\n" +
            "State:\n" + result.stateSummary + "\n\n" +
            "Erlaubtes Wissen:\n" + result.allowedKnowledgeSummary + "\n\n" +
            "Constraints:\n" + result.constraintsSummary + "\n\n" +
            "Prompt:\n" + result.prompt;
    }

    private void HandleStateChanged(string stateSummary)
    {
        SyncStateTogglesFromManager();
        RefreshDebugSummary("State geändert.");
    }

    private void HandleResponseModeChanged(ResponseMode mode)
    {
        SyncStateTogglesFromManager();
        RefreshDebugSummary("Response Mode geändert.");
    }

    private void HandleMemoryChanged(string message)
    {
        if (message.StartsWith("Alle"))
        {
            foreach (string npcId in GetKnownNpcIds())
            {
                AddSystemMessage(npcId, "Alle NPC-Memorys wurden zurückgesetzt.");
            }
        }
        else
        {
            string npcId = GetCurrentNpcId();
            AddSystemMessage(npcId, "Memory für " + GetNpcDisplayName(npcId) + " wurde zurückgesetzt.");
        }

        RenderCurrentNpcChat();
        RefreshDebugSummary(message);
    }

    private void RefreshChatTextVisibility()
    {
        if (chatHistoryText == null)
        {
            return;
        }

        chatHistoryText.gameObject.SetActive(true);
        chatHistoryText.enabled = true;
        chatHistoryText.raycastTarget = false;
        chatHistoryText.fontSize = 22;
        chatHistoryText.alignment = TextAnchor.LowerLeft;
        chatHistoryText.color = TextColor;
        chatHistoryText.horizontalOverflow = HorizontalWrapMode.Wrap;
        chatHistoryText.verticalOverflow = VerticalWrapMode.Overflow;
        chatHistoryText.supportRichText = true;

        if (chatScrollRect == null)
        {
            chatScrollRect = chatHistoryText.GetComponentInParent<ScrollRect>();
        }

        if (chatScrollRect == null || chatScrollRect.content == null || chatScrollRect.viewport == null)
        {
            RectTransform fallbackTextRect = chatHistoryText.rectTransform;
            fallbackTextRect.anchorMin = Vector2.zero;
            fallbackTextRect.anchorMax = Vector2.one;
            fallbackTextRect.offsetMin = new Vector2(24f, 24f);
            fallbackTextRect.offsetMax = new Vector2(-24f, -24f);
            fallbackTextRect.pivot = new Vector2(0f, 0f);
            Canvas.ForceUpdateCanvases();
            return;
        }

        chatContentRect = chatScrollRect.content;
        RectTransform textRect = chatHistoryText.rectTransform;
        RectTransform viewportRect = chatScrollRect.viewport;

        chatScrollRect.horizontal = false;
        chatScrollRect.vertical = true;
        chatScrollRect.movementType = ScrollRect.MovementType.Clamped;
        chatScrollRect.viewport = viewportRect;
        chatScrollRect.content = chatContentRect;

        chatContentRect.anchorMin = new Vector2(0f, 1f);
        chatContentRect.anchorMax = new Vector2(1f, 1f);
        chatContentRect.pivot = new Vector2(0f, 1f);
        chatContentRect.anchoredPosition = Vector2.zero;

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 24f);
        textRect.offsetMax = new Vector2(-24f, -24f);
        textRect.pivot = new Vector2(0f, 0f);
        chatHistoryText.transform.SetAsLastSibling();

        Canvas.ForceUpdateCanvases();

        float viewportHeight = Mathf.Max(0f, viewportRect.rect.height);
        float contentHeight = Mathf.Max(viewportHeight, chatHistoryText.preferredHeight + 48f);
        chatContentRect.sizeDelta = new Vector2(0f, contentHeight);

        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    private void RenderCurrentNpcChat()
    {
        activeNpcId = string.IsNullOrWhiteSpace(activeNpcId) ? GetCurrentNpcId() : activeNpcId;
        List<VisibleChatMessage> history = GetOrCreateChatHistory(activeNpcId);
        int startIndex = Mathf.Max(0, history.Count - MaxVisibleChatMessages);
        StringBuilder builder = new StringBuilder();

        if (startIndex > 0)
        {
            AppendSystemLine(builder, "Ältere Nachrichten sind in dieser Ansicht ausgeblendet.");
        }

        for (int i = startIndex; i < history.Count; i++)
        {
            if (builder.Length > 0)
            {
                builder.Append("\n\n");
            }

            AppendFormattedMessage(builder, history[i]);
        }

        chatHistoryText.text = builder.ToString();
        RefreshChatTextVisibility();
    }

    private void RenderCurrentNpcChatIfActive(string npcId)
    {
        if (npcId == activeNpcId)
        {
            RenderCurrentNpcChat();
        }
    }

    private void AddChatMessage(string npcId, ChatMessageType type, string speaker, string text, bool isTyping)
    {
        List<VisibleChatMessage> history = GetOrCreateChatHistory(npcId);
        history.Add(new VisibleChatMessage
        {
            type = type,
            speaker = speaker,
            text = text,
            isTyping = isTyping
        });
    }

    private void AddSystemMessage(string npcId, string text)
    {
        AddChatMessage(npcId, ChatMessageType.System, "System", text, false);
    }

    private void AddTypingIndicator(string npcId, string npcDisplayName)
    {
        RemoveTypingIndicator(npcId);
        AddChatMessage(npcId, ChatMessageType.System, "System", npcDisplayName + " schreibt gerade...", true);
    }

    private void RemoveTypingIndicator(string npcId)
    {
        List<VisibleChatMessage> history = GetOrCreateChatHistory(npcId);
        history.RemoveAll(message => message.isTyping);
    }

    private bool HasPendingTypingIndicator()
    {
        foreach (List<VisibleChatMessage> history in visibleChatHistories.Values)
        {
            if (history.Exists(message => message.isTyping))
            {
                return true;
            }
        }

        return false;
    }

    private string GetPendingTypingNpcName()
    {
        foreach (KeyValuePair<string, List<VisibleChatMessage>> entry in visibleChatHistories)
        {
            if (entry.Value.Exists(message => message.isTyping))
            {
                return GetNpcDisplayName(entry.Key);
            }
        }

        return GetNpcDisplayName(GetCurrentNpcId());
    }

    private List<VisibleChatMessage> GetOrCreateChatHistory(string npcId)
    {
        string safeNpcId = string.IsNullOrWhiteSpace(npcId) ? "clara" : npcId;
        if (!visibleChatHistories.TryGetValue(safeNpcId, out List<VisibleChatMessage> history))
        {
            history = new List<VisibleChatMessage>();
            visibleChatHistories.Add(safeNpcId, history);
            history.Add(new VisibleChatMessage
            {
                type = ChatMessageType.System,
                speaker = "System",
                text = "Du sprichst jetzt mit " + GetNpcDisplayName(safeNpcId) + ". Stelle eine Frage zum Fall Viktor Stein.",
                isTyping = false
            });
        }

        return history;
    }

    private string GetCurrentNpcId()
    {
        if (dialogueManager != null && !string.IsNullOrWhiteSpace(dialogueManager.CurrentNpcId))
        {
            return dialogueManager.CurrentNpcId;
        }

        return string.IsNullOrWhiteSpace(activeNpcId) ? "clara" : activeNpcId;
    }

    private string GetNpcDisplayName(string npcId)
    {
        if (dialogueManager != null && dialogueManager.profiles.TryGetValue(npcId, out NpcProfile profile))
        {
            return profile.displayName;
        }

        switch (npcId)
        {
            case "anton":
                return "Anton Stein";
            case "mira":
                return "Mira Feld";
            default:
                return "Clara Weber";
        }
    }

    private IEnumerable<string> GetKnownNpcIds()
    {
        if (dialogueManager != null && dialogueManager.profiles.Count > 0)
        {
            return dialogueManager.profiles.Keys;
        }

        return new[] { "clara", "anton", "mira" };
    }

    private static void AppendFormattedMessage(StringBuilder builder, VisibleChatMessage message)
    {
        switch (message.type)
        {
            case ChatMessageType.Player:
                builder.Append("<color=#C9A24E><b>Spieler:</b></color>\n");
                builder.Append(EscapeRichText(message.text));
                break;
            case ChatMessageType.Npc:
                builder.Append("<color=#E9E7DF><b>");
                builder.Append(EscapeRichText(message.speaker));
                builder.Append(":</b></color>\n");
                builder.Append(EscapeRichText(message.text));
                break;
            default:
                AppendSystemLine(builder, message.text);
                break;
        }
    }

    private static void AppendSystemLine(StringBuilder builder, string text)
    {
        builder.Append("<color=#9FB1C2><i>");
        builder.Append(EscapeRichText(text));
        builder.Append("</i></color>");
    }

    private static string EscapeRichText(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace("<", "‹").Replace(">", "›");
    }

    private void RefreshDebugSummary(string reason)
    {
        if (debugText == null || dialogueManager == null)
        {
            return;
        }

        NpcProfile profile = dialogueManager.GetCurrentProfile();
        string npcName = profile == null ? "Unbekannt" : profile.displayName;
        string knowledge = profile == null ? "-" : string.Join(", ", profile.allowedKnowledge);
        string constraints = profile == null ? "-" : string.Join(", ", profile.constraints);

        debugText.text =
            "Status: " + reason + "\n" +
            "NPC: " + npcName + "\n" +
            "Response Mode: " + dialogueManager.CurrentResponseMode + "\n" +
            "Prompt-Version: " + DialogueManager.PromptVersion + "\n" +
            "Testfall-ID: " + GetCurrentTestCaseId() + "\n\n" +
            "Auto-State: " + (dialogueManager.EnableAutoStateProgression ? "aktiv" : "aus") + "\n\n" +
            "Aktive State-Flags:\n" + dialogueManager.GetDebugStateSummary() + "\n\n" +
            "Erlaubtes Wissen:\n" + knowledge + "\n\n" +
            "Constraints:\n" + constraints;
    }

    private string GetCurrentTestCaseId()
    {
        if (testCaseInputField == null || string.IsNullOrWhiteSpace(testCaseInputField.text))
        {
            return "manual";
        }

        return testCaseInputField.text.Trim();
    }

    private void RegisterStateToggle(Toggle toggle, string flagName)
    {
        toggle.onValueChanged.AddListener(value =>
        {
            if (syncingStateToggles)
            {
                return;
            }

            dialogueManager.SetStateFlag(flagName, value);
        });
    }

    private void SyncStateTogglesFromManager()
    {
        if (dialogueManager == null || hasFoundBrokenKeyToggle == null)
        {
            return;
        }

        syncingStateToggles = true;
        GameState state = dialogueManager.State;
        hasFoundBrokenKeyToggle.isOn = state.hasFoundBrokenKey;
        hasFoundBurnedLetterToggle.isOn = state.hasFoundBurnedLetter;
        hasFoundDebtNoteToggle.isOn = state.hasFoundDebtNote;
        hasAnalyzedWineToggle.isOn = state.hasAnalyzedWine;
        hasQuestionedClaraAlibiToggle.isOn = state.hasQuestionedClaraAlibi;
        hasAskedMiraAboutNightToggle.isOn = state.hasAskedMiraAboutNight;
        caseSolvedToggle.isOn = state.caseSolved;
        autoStateToggle.isOn = dialogueManager.EnableAutoStateProgression;
        syncingStateToggles = false;
        RefreshResponseModeButtons();
    }

    private void RefreshResponseModeButtons()
    {
        if (dummyModeButtonImage == null || apiModeButtonImage == null)
        {
            return;
        }

        bool isDummy = dialogueManager.CurrentResponseMode == ResponseMode.Dummy;
        dummyModeButtonImage.color = isDummy ? ButtonSelectedColor : ButtonColor;
        apiModeButtonImage.color = isDummy ? ButtonColor : ButtonSelectedColor;
    }

    private void ToggleDebugPanel()
    {
        debugVisible = !debugVisible;
        debugPanel.gameObject.SetActive(debugVisible);
        SetButtonLabel(toggleDebugButton, debugVisible ? "Debug ausblenden" : "Debug anzeigen");
    }

    private void RefreshNpcSelectionVisuals(string displayName)
    {
        SetCardSelected(claraCardImage, displayName == "Clara Weber");
        SetCardSelected(antonCardImage, displayName == "Anton Stein");
        SetCardSelected(miraCardImage, displayName == "Mira Feld");
    }

    private static void SetCardSelected(Image image, bool selected)
    {
        if (image == null)
        {
            return;
        }

        image.color = selected ? ButtonSelectedColor : ButtonColor;
    }

    private bool HasCompleteUiReferences()
    {
        return chatHistoryText != null
            && activeNpcText != null
            && debugText != null
            && inputField != null
            && testCaseInputField != null
            && claraButton != null
            && antonButton != null
            && miraButton != null
            && sendButton != null
            && resetCurrentMemoryButton != null
            && resetAllMemoriesButton != null
            && toggleDebugButton != null
            && dummyModeButton != null
            && apiModeButton != null
            && hasFoundBrokenKeyToggle != null
            && hasFoundBurnedLetterToggle != null
            && hasFoundDebtNoteToggle != null
            && hasAnalyzedWineToggle != null
            && hasQuestionedClaraAlibiToggle != null
            && hasAskedMiraAboutNightToggle != null
            && caseSolvedToggle != null
            && autoStateToggle != null
            && debugPanel != null
            && chatScrollRect != null
            && chatContentRect != null
            && claraCardImage != null
            && antonCardImage != null
            && miraCardImage != null
            && dummyModeButtonImage != null
            && apiModeButtonImage != null;
    }

    private bool HasScrollableChatArea()
    {
        if (chatHistoryText == null || chatHistoryText.transform.parent == null)
        {
            return false;
        }

        chatScrollRect = chatHistoryText.GetComponentInParent<ScrollRect>();
        chatContentRect = chatScrollRect == null ? null : chatScrollRect.content;
        return chatScrollRect != null
            && chatScrollRect.viewport != null
            && chatContentRect != null
            && chatHistoryText.transform.parent == chatContentRect.transform;
    }

    private void EnsureUiExists()
    {
        if (HasCompleteUiReferences() && HasScrollableChatArea())
        {
            RefreshChatTextVisibility();
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Dialogue Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        Transform existingRoot = canvas.transform.Find("DialogueRoot");
        if (existingRoot != null)
        {
            Destroy(existingRoot.gameObject);
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
        }

        RectTransform root = CreatePanel("DialogueRoot", canvas.transform, BackgroundColor);
        Stretch(root, 0f, 0f, 1f, 1f, Vector2.zero, Vector2.zero);

        RectTransform titlePanel = CreatePanel("TitlePanel", root, new Color(0.070f, 0.087f, 0.112f, 1f));
        Stretch(titlePanel, 0f, 1f, 1f, 1f, new Vector2(28f, -104f), new Vector2(-28f, -24f));

        Text title = CreateText("Title", titlePanel, "KI-NPC Krimi-Prototyp – Haus Lindenfels", 34, FontStyle.Bold, TextAnchor.MiddleLeft);
        Stretch(title.rectTransform, 0f, 0f, 1f, 1f, new Vector2(28f, 0f), new Vector2(-28f, 0f));
        title.color = TextColor;

        RectTransform npcPanel = CreatePanel("NpcPanel", root, PanelColor);
        Stretch(npcPanel, 0f, 0f, 0f, 1f, new Vector2(28f, 118f), new Vector2(390f, -128f));

        Text npcTitle = CreateText("NpcPanelTitle", npcPanel, "NPC-Auswahl", 25, FontStyle.Bold, TextAnchor.MiddleLeft);
        Stretch(npcTitle.rectTransform, 0f, 1f, 1f, 1f, new Vector2(24f, -66f), new Vector2(-24f, -20f));

        claraButton = CreateNpcCard("ClaraCard", npcPanel, "Clara Weber", "Haushälterin", out claraCardImage);
        Stretch(claraButton.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(20f, -180f), new Vector2(-20f, -82f));

        antonButton = CreateNpcCard("AntonCard", npcPanel, "Anton Stein", "Neffe des Opfers", out antonCardImage);
        Stretch(antonButton.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(20f, -294f), new Vector2(-20f, -196f));

        miraButton = CreateNpcCard("MiraCard", npcPanel, "Mira Feld", "Nachbarin", out miraCardImage);
        Stretch(miraButton.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(20f, -408f), new Vector2(-20f, -310f));

        activeNpcText = CreateText("ActiveNpcText", npcPanel, "Aktiver NPC: Clara Weber", 22, FontStyle.Bold, TextAnchor.UpperLeft);
        Stretch(activeNpcText.rectTransform, 0f, 1f, 1f, 1f, new Vector2(24f, -490f), new Vector2(-24f, -430f));
        activeNpcText.color = GoldColor;

        Text npcHint = CreateText("NpcHint", npcPanel, "Die Auswahl bestimmt Profil, Wissen, Constraints und Antwortstil.", 18, FontStyle.Normal, TextAnchor.UpperLeft);
        Stretch(npcHint.rectTransform, 0f, 0f, 1f, 1f, new Vector2(24f, 24f), new Vector2(-24f, -520f));
        npcHint.color = MutedTextColor;

        RectTransform chatPanel = CreatePanel("ChatPanel", root, PanelColor);
        Stretch(chatPanel, 0f, 0f, 1f, 1f, new Vector2(420f, 118f), new Vector2(-460f, -128f));

        Text chatTitle = CreateText("ChatTitle", chatPanel, "Dialog", 25, FontStyle.Bold, TextAnchor.MiddleLeft);
        Stretch(chatTitle.rectTransform, 0f, 1f, 1f, 1f, new Vector2(26f, -62f), new Vector2(-26f, -18f));

        RectTransform chatBody = CreatePanel("ChatBody", chatPanel, new Color(0.075f, 0.088f, 0.110f, 1f));
        Stretch(chatBody, 0f, 0f, 1f, 1f, new Vector2(22f, 22f), new Vector2(-22f, -92f));

        chatScrollRect = CreateChatScrollArea("ChatScrollRect", chatBody, out chatHistoryText);
        chatContentRect = chatScrollRect.content;
        Stretch(chatScrollRect.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f, Vector2.zero, Vector2.zero);
        chatHistoryText.text = "<color=#9FB1C2><b>System:</b></color>\nWähle einen NPC aus und stelle eine Frage zum Fall Viktor Stein.";
        RefreshChatTextVisibility();

        debugPanel = CreatePanel("DebugPanel", root, PanelColor);
        Stretch(debugPanel, 1f, 0f, 1f, 1f, new Vector2(-430f, 118f), new Vector2(-28f, -128f));

        Text debugTitle = CreateText("DebugTitle", debugPanel, "Debug / Evaluation", 25, FontStyle.Bold, TextAnchor.MiddleLeft);
        Stretch(debugTitle.rectTransform, 0f, 1f, 1f, 1f, new Vector2(22f, -60f), new Vector2(-22f, -18f));

        Text testCaseLabel = CreateText("TestCaseLabel", debugPanel, "Testfall-ID", 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        Stretch(testCaseLabel.rectTransform, 0f, 1f, 1f, 1f, new Vector2(22f, -106f), new Vector2(-22f, -72f));

        testCaseInputField = CreateInputField("TestCaseInput", debugPanel, "manual", 20);
        testCaseInputField.text = "manual";
        Stretch(testCaseInputField.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -158f), new Vector2(-22f, -112f));

        Text responseModeLabel = CreateText("ResponseModeLabel", debugPanel, "Response Mode", 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        Stretch(responseModeLabel.rectTransform, 0f, 1f, 1f, 1f, new Vector2(22f, -204f), new Vector2(-22f, -170f));

        dummyModeButton = CreateButton("ResponseModeDummyButton", debugPanel, "Dummy", 18, ButtonSelectedColor);
        dummyModeButtonImage = dummyModeButton.GetComponent<Image>();
        Stretch(dummyModeButton.GetComponent<RectTransform>(), 0f, 1f, 0.5f, 1f, new Vector2(22f, -256f), new Vector2(-6f, -210f));

        apiModeButton = CreateButton("ResponseModeApiButton", debugPanel, "API", 18, ButtonColor);
        apiModeButtonImage = apiModeButton.GetComponent<Image>();
        Stretch(apiModeButton.GetComponent<RectTransform>(), 0.5f, 1f, 1f, 1f, new Vector2(6f, -256f), new Vector2(-22f, -210f));

        Text stateTitle = CreateText("StateTitle", debugPanel, "State-Flags", 20, FontStyle.Bold, TextAnchor.MiddleLeft);
        Stretch(stateTitle.rectTransform, 0f, 1f, 1f, 1f, new Vector2(22f, -306f), new Vector2(-22f, -270f));

        hasFoundBrokenKeyToggle = CreateToggle("BrokenKeyToggle", debugPanel, "Abgebrochener Schlüssel", 18);
        Stretch(hasFoundBrokenKeyToggle.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -346f), new Vector2(-22f, -314f));

        hasFoundBurnedLetterToggle = CreateToggle("BurnedLetterToggle", debugPanel, "Verbrannter Brief", 18);
        Stretch(hasFoundBurnedLetterToggle.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -384f), new Vector2(-22f, -352f));

        hasFoundDebtNoteToggle = CreateToggle("DebtNoteToggle", debugPanel, "Schuldennotiz", 18);
        Stretch(hasFoundDebtNoteToggle.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -422f), new Vector2(-22f, -390f));

        hasAnalyzedWineToggle = CreateToggle("AnalyzedWineToggle", debugPanel, "Wein analysiert", 18);
        Stretch(hasAnalyzedWineToggle.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -460f), new Vector2(-22f, -428f));

        hasQuestionedClaraAlibiToggle = CreateToggle("ClaraAlibiToggle", debugPanel, "Claras Alibi befragt", 18);
        Stretch(hasQuestionedClaraAlibiToggle.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -498f), new Vector2(-22f, -466f));

        hasAskedMiraAboutNightToggle = CreateToggle("MiraNightToggle", debugPanel, "Mira zur Nacht befragt", 18);
        Stretch(hasAskedMiraAboutNightToggle.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -536f), new Vector2(-22f, -504f));

        caseSolvedToggle = CreateToggle("CaseSolvedToggle", debugPanel, "Fall gelöst", 18);
        Stretch(caseSolvedToggle.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -574f), new Vector2(-22f, -542f));

        autoStateToggle = CreateToggle("AutoStateToggle", debugPanel, "Auto-State", 18);
        Stretch(autoStateToggle.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -612f), new Vector2(-22f, -580f));

        resetCurrentMemoryButton = CreateButton("ResetCurrentMemoryButton", debugPanel, "Memory zurücksetzen", 19, WineColor);
        Stretch(resetCurrentMemoryButton.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -674f), new Vector2(-22f, -628f));

        resetAllMemoriesButton = CreateButton("ResetAllMemoriesButton", debugPanel, "Alle Memorys zurücksetzen", 19, ButtonColor);
        Stretch(resetAllMemoriesButton.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(22f, -728f), new Vector2(-22f, -682f));

        ScrollRect debugScroll = CreateScrollArea("DebugScroll", debugPanel, out debugText, 16);
        Stretch(debugScroll.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f, new Vector2(22f, 22f), new Vector2(-22f, -754f));
        debugText.supportRichText = true;

        RectTransform inputPanel = CreatePanel("InputPanel", root, PanelColorAlt);
        Stretch(inputPanel, 0f, 0f, 1f, 0f, new Vector2(28f, 28f), new Vector2(-28f, 100f));

        toggleDebugButton = CreateButton("ToggleDebugButton", inputPanel, "Debug ausblenden", 18, ButtonColor);
        Stretch(toggleDebugButton.GetComponent<RectTransform>(), 0f, 0f, 0f, 1f, new Vector2(22f, 14f), new Vector2(210f, -14f));

        inputField = CreateInputField("QuestionInput", inputPanel, "Frage zum Fall Viktor Stein eingeben...", 22);
        Stretch(inputField.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f, new Vector2(230f, 14f), new Vector2(-180f, -14f));

        sendButton = CreateButton("SendButton", inputPanel, "Senden", 22, WineColor);
        Stretch(sendButton.GetComponent<RectTransform>(), 1f, 0f, 1f, 1f, new Vector2(-160f, 14f), new Vector2(-22f, -14f));
    }

    private static RectTransform CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        return panel.GetComponent<RectTransform>();
    }

    private static Text CreateText(string name, Transform parent, string value, int size, FontStyle style, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = TextColor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = true;
        return text;
    }

    private static Button CreateNpcCard(string name, Transform parent, string displayName, string role, out Image cardImage)
    {
        GameObject cardObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        cardObject.transform.SetParent(parent, false);
        cardImage = cardObject.GetComponent<Image>();
        cardImage.color = ButtonColor;

        Button button = cardObject.GetComponent<Button>();
        button.colors = BuildColorBlock(ButtonColor);

        Text nameText = CreateText("Name", cardObject.transform, displayName, 23, FontStyle.Bold, TextAnchor.UpperLeft);
        Stretch(nameText.rectTransform, 0f, 0f, 1f, 1f, new Vector2(20f, 42f), new Vector2(-20f, -12f));

        Text roleText = CreateText("Role", cardObject.transform, role, 17, FontStyle.Normal, TextAnchor.LowerLeft);
        roleText.color = MutedTextColor;
        Stretch(roleText.rectTransform, 0f, 0f, 1f, 1f, new Vector2(20f, 14f), new Vector2(-20f, -50f));

        return button;
    }

    private static Button CreateButton(string name, Transform parent, string label, int fontSize, Color color)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.colors = BuildColorBlock(color);

        Text text = CreateText("Text", buttonObject.transform, label, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform, 0f, 0f, 1f, 1f, new Vector2(8f, 0f), new Vector2(-8f, 0f));
        return button;
    }

    private static InputField CreateInputField(string name, Transform parent, string placeholder, int fontSize)
    {
        GameObject inputObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        inputObject.transform.SetParent(parent, false);
        inputObject.GetComponent<Image>().color = new Color(0.895f, 0.900f, 0.890f, 1f);

        Text text = CreateText("Text", inputObject.transform, string.Empty, fontSize, FontStyle.Normal, TextAnchor.MiddleLeft);
        text.color = new Color(0.050f, 0.055f, 0.065f, 1f);
        Stretch(text.rectTransform, 0f, 0f, 1f, 1f, new Vector2(16f, 0f), new Vector2(-16f, 0f));

        Text placeholderText = CreateText("Placeholder", inputObject.transform, placeholder, fontSize, FontStyle.Italic, TextAnchor.MiddleLeft);
        placeholderText.color = new Color(0.300f, 0.320f, 0.350f, 0.72f);
        Stretch(placeholderText.rectTransform, 0f, 0f, 1f, 1f, new Vector2(16f, 0f), new Vector2(-16f, 0f));

        InputField field = inputObject.GetComponent<InputField>();
        field.textComponent = text;
        field.placeholder = placeholderText;
        field.lineType = InputField.LineType.SingleLine;
        return field;
    }

    private static Toggle CreateToggle(string name, Transform parent, string label, int fontSize)
    {
        GameObject toggleObject = new GameObject(name, typeof(RectTransform), typeof(Toggle));
        toggleObject.transform.SetParent(parent, false);

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(toggleObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0f, 0.5f);
        backgroundRect.sizeDelta = new Vector2(24f, 24f);
        backgroundRect.anchoredPosition = new Vector2(12f, 0f);
        backgroundObject.GetComponent<Image>().color = new Color(0.870f, 0.875f, 0.865f, 1f);

        GameObject checkmarkObject = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkmarkObject.transform.SetParent(backgroundObject.transform, false);
        RectTransform checkmarkRect = checkmarkObject.GetComponent<RectTransform>();
        Stretch(checkmarkRect, 0f, 0f, 1f, 1f, new Vector2(5f, 5f), new Vector2(-5f, -5f));
        checkmarkObject.GetComponent<Image>().color = GoldColor;

        Text text = CreateText("Label", toggleObject.transform, label, fontSize, FontStyle.Normal, TextAnchor.MiddleLeft);
        Stretch(text.rectTransform, 0f, 0f, 1f, 1f, new Vector2(36f, 0f), Vector2.zero);

        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.targetGraphic = backgroundObject.GetComponent<Image>();
        toggle.graphic = checkmarkObject.GetComponent<Image>();
        toggle.isOn = false;
        return toggle;
    }

    private static ScrollRect CreateScrollArea(string name, Transform parent, out Text contentText, int fontSize)
    {
        GameObject scrollObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(Mask));
        scrollObject.transform.SetParent(parent, false);
        scrollObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        scrollObject.GetComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(Text));
        contentObject.transform.SetParent(scrollObject.transform, false);
        contentText = contentObject.GetComponent<Text>();
        contentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        contentText.fontSize = fontSize;
        contentText.alignment = TextAnchor.UpperLeft;
        contentText.color = TextColor;
        contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
        contentText.verticalOverflow = VerticalWrapMode.Overflow;
        contentText.supportRichText = true;

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 2000f);

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        return scrollRect;
    }

    private static ScrollRect CreateChatScrollArea(string name, Transform parent, out Text contentText)
    {
        GameObject scrollObject = new GameObject(name, typeof(RectTransform), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        viewportObject.transform.SetParent(scrollObject.transform, false);
        viewportObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        Stretch(viewportRect, 0f, 0f, 1f, 1f, Vector2.zero, Vector2.zero);

        GameObject contentObject = new GameObject("Content", typeof(RectTransform));
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        contentText = CreateText("ChatHistoryText", contentObject.transform, string.Empty, 22, FontStyle.Normal, TextAnchor.LowerLeft);
        contentText.raycastTarget = false;
        contentText.verticalOverflow = VerticalWrapMode.Overflow;

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 40f;
        return scrollRect;
    }

    private static ColorBlock BuildColorBlock(Color normalColor)
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = normalColor;
        colors.highlightedColor = Color.Lerp(normalColor, GoldColor, 0.25f);
        colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.25f);
        colors.selectedColor = Color.Lerp(normalColor, GoldColor, 0.18f);
        colors.disabledColor = new Color(0.20f, 0.20f, 0.20f, 0.45f);
        colors.colorMultiplier = 1f;
        return colors;
    }

    private static void SetButtonLabel(Button button, string label)
    {
        Text text = button.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = label;
        }
    }

    private static void Stretch(RectTransform rect, float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = new Vector2(anchorMinX, anchorMinY);
        rect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
