using UnityEngine;
using UnityEngine.UI;

public class ChatUIController : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Button claraButton;
    [SerializeField] private Button antonButton;
    [SerializeField] private Button miraButton;
    [SerializeField] private Button sendButton;
    [SerializeField] private InputField inputField;
    [SerializeField] private Text chatHistoryText;
    [SerializeField] private Text activeNpcText;
    [SerializeField] private Text debugText;

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
    }

    private void OnDestroy()
    {
        if (dialogueManager != null)
        {
            dialogueManager.DialogueTurnCompleted -= HandleDialogueTurnCompleted;
            dialogueManager.NpcSelected -= HandleNpcSelected;
        }
    }

    private void RegisterListeners()
    {
        claraButton.onClick.AddListener(() => dialogueManager.SelectNpc("clara"));
        antonButton.onClick.AddListener(() => dialogueManager.SelectNpc("anton"));
        miraButton.onClick.AddListener(() => dialogueManager.SelectNpc("mira"));
        sendButton.onClick.AddListener(SendCurrentInput);
        inputField.onSubmit.AddListener(_ => SendCurrentInput());

        dialogueManager.DialogueTurnCompleted += HandleDialogueTurnCompleted;
        dialogueManager.NpcSelected += HandleNpcSelected;
    }

    private void SendCurrentInput()
    {
        string input = inputField.text;
        dialogueManager.SendPlayerInput(input);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    private void HandleNpcSelected(string displayName)
    {
        activeNpcText.text = "Aktiver NPC: " + displayName;
    }

    private void HandleDialogueTurnCompleted(DialogueTurnResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.playerInput))
        {
            chatHistoryText.text += "\n\nDu: " + result.playerInput;
        }

        chatHistoryText.text += "\n" + result.npcDisplayName + ": " + result.npcResponse;

        debugText.text =
            "NPC: " + result.npcDisplayName + "\n" +
            "State: " + result.stateSummary + "\n" +
            "Erlaubtes Wissen: " + result.allowedKnowledgeSummary + "\n" +
            "Constraints: " + result.constraintsSummary + "\n\n" +
            "Prompt:\n" + result.prompt;
    }

    private void EnsureUiExists()
    {
        if (chatHistoryText != null && inputField != null && sendButton != null && activeNpcText != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Dialogue Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
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

        RectTransform root = CreatePanel("DialogueRoot", canvas.transform, new Color(0.08f, 0.09f, 0.11f, 0.95f));
        Stretch(root, 0f, 0f, 1f, 1f, Vector2.zero, Vector2.zero);

        RectTransform sidebar = CreatePanel("NpcSelectionPanel", root, new Color(0.13f, 0.14f, 0.17f, 1f));
        Stretch(sidebar, 0f, 0f, 0f, 1f, new Vector2(20f, 20f), new Vector2(260f, -20f));

        Text title = CreateText("NpcTitle", sidebar, "NPC-Auswahl", 22, FontStyle.Bold, TextAnchor.MiddleLeft);
        Stretch(title.rectTransform, 0f, 1f, 1f, 1f, new Vector2(16f, -58f), new Vector2(-16f, -18f));

        claraButton = CreateButton("ClaraButton", sidebar, "Clara Weber");
        Stretch(claraButton.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(16f, -120f), new Vector2(-16f, -74f));

        antonButton = CreateButton("AntonButton", sidebar, "Anton Stein");
        Stretch(antonButton.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(16f, -176f), new Vector2(-16f, -130f));

        miraButton = CreateButton("MiraButton", sidebar, "Mira Feld");
        Stretch(miraButton.GetComponent<RectTransform>(), 0f, 1f, 1f, 1f, new Vector2(16f, -232f), new Vector2(-16f, -186f));

        activeNpcText = CreateText("ActiveNpcText", sidebar, "Aktiver NPC: Clara Weber", 17, FontStyle.Normal, TextAnchor.UpperLeft);
        Stretch(activeNpcText.rectTransform, 0f, 1f, 1f, 1f, new Vector2(16f, -330f), new Vector2(-16f, -250f));

        RectTransform chatPanel = CreatePanel("ChatPanel", root, new Color(0.10f, 0.11f, 0.13f, 1f));
        Stretch(chatPanel, 0f, 0f, 1f, 1f, new Vector2(290f, 160f), new Vector2(-20f, -20f));

        ScrollRect chatScroll = CreateScrollArea("ChatScroll", chatPanel, out chatHistoryText);
        Stretch(chatScroll.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f, new Vector2(14f, 14f), new Vector2(-14f, -14f));
        chatHistoryText.text = "Waehle einen NPC und stelle eine Frage.";

        RectTransform inputPanel = CreatePanel("InputPanel", root, new Color(0.12f, 0.13f, 0.15f, 1f));
        Stretch(inputPanel, 0f, 0f, 1f, 0f, new Vector2(290f, 80f), new Vector2(-20f, 145f));

        inputField = CreateInputField("QuestionInput", inputPanel, "Frage eingeben...");
        Stretch(inputField.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f, new Vector2(14f, 14f), new Vector2(-136f, -14f));

        sendButton = CreateButton("SendButton", inputPanel, "Senden");
        Stretch(sendButton.GetComponent<RectTransform>(), 1f, 0f, 1f, 1f, new Vector2(-120f, 14f), new Vector2(-14f, -14f));

        RectTransform debugPanel = CreatePanel("DebugPanel", root, new Color(0.07f, 0.08f, 0.10f, 1f));
        Stretch(debugPanel, 0f, 0f, 1f, 0f, new Vector2(290f, 20f), new Vector2(-20f, 70f));

        ScrollRect debugScroll = CreateScrollArea("DebugScroll", debugPanel, out debugText);
        Stretch(debugScroll.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f, new Vector2(10f, 8f), new Vector2(-10f, -8f));
        debugText.fontSize = 13;
        debugText.text = "Debug-Ausgabe erscheint nach dem ersten Senden.";
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
        text.color = new Color(0.94f, 0.95f, 0.96f, 1f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.22f, 0.24f, 0.29f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.22f, 0.24f, 0.29f, 1f);
        colors.highlightedColor = new Color(0.30f, 0.33f, 0.39f, 1f);
        colors.pressedColor = new Color(0.16f, 0.18f, 0.22f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        Text text = CreateText("Text", buttonObject.transform, label, 17, FontStyle.Bold, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform, 0f, 0f, 1f, 1f, Vector2.zero, Vector2.zero);
        return button;
    }

    private static InputField CreateInputField(string name, Transform parent, string placeholder)
    {
        GameObject inputObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        inputObject.transform.SetParent(parent, false);
        inputObject.GetComponent<Image>().color = new Color(0.92f, 0.93f, 0.95f, 1f);

        Text text = CreateText("Text", inputObject.transform, string.Empty, 17, FontStyle.Normal, TextAnchor.MiddleLeft);
        text.color = new Color(0.05f, 0.06f, 0.07f, 1f);
        Stretch(text.rectTransform, 0f, 0f, 1f, 1f, new Vector2(12f, 0f), new Vector2(-12f, 0f));

        Text placeholderText = CreateText("Placeholder", inputObject.transform, placeholder, 17, FontStyle.Italic, TextAnchor.MiddleLeft);
        placeholderText.color = new Color(0.35f, 0.37f, 0.42f, 0.75f);
        Stretch(placeholderText.rectTransform, 0f, 0f, 1f, 1f, new Vector2(12f, 0f), new Vector2(-12f, 0f));

        InputField field = inputObject.GetComponent<InputField>();
        field.textComponent = text;
        field.placeholder = placeholderText;
        field.lineType = InputField.LineType.SingleLine;
        return field;
    }

    private static ScrollRect CreateScrollArea(string name, Transform parent, out Text contentText)
    {
        GameObject scrollObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(Mask));
        scrollObject.transform.SetParent(parent, false);
        scrollObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        scrollObject.GetComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(Text));
        contentObject.transform.SetParent(scrollObject.transform, false);
        contentText = contentObject.GetComponent<Text>();
        contentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        contentText.fontSize = 15;
        contentText.alignment = TextAnchor.UpperLeft;
        contentText.color = new Color(0.93f, 0.94f, 0.96f, 1f);
        contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
        contentText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        Stretch(contentRect, 0f, 1f, 1f, 1f, new Vector2(0f, -1200f), Vector2.zero);

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        return scrollRect;
    }

    private static void Stretch(RectTransform rect, float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = new Vector2(anchorMinX, anchorMinY);
        rect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
