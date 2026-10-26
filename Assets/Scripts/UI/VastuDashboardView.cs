using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Builds the Vastu tab from reusable uGUI controls while retaining the existing Vastu manager as its data source.
public class VastuDashboardView : MonoBehaviour
{
    static readonly Color Navy = new Color(0.075f, 0.118f, 0.145f, 1f);
    static readonly Color Teal = new Color(0.055f, 0.36f, 0.34f, 1f);
    static readonly Color Mint = new Color(0.82f, 0.91f, 0.87f, 1f);
    static readonly Color Cream = new Color(1f, 0.992f, 0.965f, 1f);
    static readonly Color Sand = new Color(0.93f, 0.86f, 0.73f, 1f);
    static readonly Color Ink = new Color(0.075f, 0.118f, 0.145f, 1f);
    static readonly Color Muted = new Color(0.30f, 0.38f, 0.40f, 1f);
    static Sprite roundedSprite;

    readonly List<LibraryItem> rooms = new List<LibraryItem>();
    readonly List<VastuMessage> renderedMessages = new List<VastuMessage>();

    LibraryItem selectedRoom;
    Transform roomChoices;
    Transform chatContent;
    TextMeshProUGUI roomSummary;
    TextMeshProUGUI scoreValue;
    TextMeshProUGUI scoreStatus;
    TextMeshProUGUI directionSummary;
    TMP_InputField questionInput;
    bool built;
    bool apiClientSubscribed;

    public void Build()
    {
        if (built)
            return;

        built = true;
        // VastuTab owns a legacy VerticalLayoutGroup; this full-screen dashboard must not be sized by it.
        LayoutElement rootLayout = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
        rootLayout.ignoreLayout = true;
        GetComponent<RectTransform>().anchorMin = Vector2.zero;
        GetComponent<RectTransform>().anchorMax = Vector2.one;
        GetComponent<RectTransform>().offsetMin = Vector2.zero;
        GetComponent<RectTransform>().offsetMax = Vector2.zero;

        Image backdrop = gameObject.AddComponent<Image>();
        backdrop.color = Navy;

        TextMeshProUGUI pageTitle = Text(transform, "PageTitle", "Vastu Insights", 29, FontStyles.Bold, Cream);
        pageTitle.alignment = TextAlignmentOptions.MidlineLeft;
        pageTitle.rectTransform.anchorMin = new Vector2(0f, 1f);
        pageTitle.rectTransform.anchorMax = new Vector2(1f, 1f);
        pageTitle.rectTransform.pivot = new Vector2(0.5f, 1f);
        pageTitle.rectTransform.offsetMin = new Vector2(24f, -62f);
        pageTitle.rectTransform.offsetMax = new Vector2(-24f, -14f);

        GameObject scroll = new GameObject("VastuScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
        scroll.transform.SetParent(transform, false);
        RectTransform scrollRectTransform = scroll.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = new Vector2(0f, -70f);

        GameObject content = Panel(scroll.transform, "Content", Navy);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 26, 120);
        layout.spacing = 16f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scroll.GetComponent<ScrollRect>();
        scrollRect.viewport = scroll.GetComponent<RectTransform>();
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        BuildHeader(content.transform);
        BuildRoomSelector(content.transform);
        BuildDirectionSelector(content.transform);
        BuildScore(content.transform);
        BuildActions(content.transform);
        BuildAssistant(content.transform);

        VastuAssistantManager.Instance?.EnsureWelcomeMessage();
        if (VastuAssistantManager.Instance != null)
            VastuAssistantManager.Instance.OnMessageAdded += OnMessageAdded;

        // VastuApiClient may not exist yet at Build() time (AppManager spawns it
        // after scene load).  TrySubscribeToApiClient() is retried from Update().
        TrySubscribeToApiClient();

        RefreshRooms();
        RefreshAssistant();
    }

    void Update()
    {
        // Retry each frame until VastuApiClient.Instance is available.
        if (built && !apiClientSubscribed)
            TrySubscribeToApiClient();
    }

    void TrySubscribeToApiClient()
    {
        if (apiClientSubscribed || VastuApiClient.Instance == null)
            return;
        VastuApiClient.Instance.OnAnalysisComplete += OnApiAnalysisComplete;
        apiClientSubscribed = true;
        Debug.Log("[VastuDashboardView] Subscribed to VastuApiClient.OnAnalysisComplete.");
    }

    void OnDestroy()
    {
        if (VastuAssistantManager.Instance != null)
            VastuAssistantManager.Instance.OnMessageAdded -= OnMessageAdded;

        if (VastuApiClient.Instance != null)
            VastuApiClient.Instance.OnAnalysisComplete -= OnApiAnalysisComplete;
    }

    public void RefreshRooms()
    {
        rooms.Clear();
        if (LibraryDataManager.Instance != null)
        {
            foreach (LibraryItem item in LibraryDataManager.Instance.GetByCategory("rooms"))
                rooms.Add(item);
        }

        if (selectedRoom == null || !rooms.Contains(selectedRoom))
            selectedRoom = rooms.Count > 0 ? rooms[0] : null;

        foreach (Transform child in roomChoices)
            Destroy(child.gameObject);

        if (rooms.Count == 0)
        {
            TextMeshProUGUI empty = Text(roomChoices, "Empty", "No saved room scans yet. Scan and save a room to personalise your Vastu guidance.", 14, FontStyles.Normal, Muted);
            empty.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                LibraryItem room = rooms[i];
                LibraryItem capturedRoom = room;
                Button choice = Button(roomChoices, "RoomChoice", "Room " + (i + 1), room == selectedRoom ? Sand : Mint, Ink, 54f);
                LayoutElement choiceSize = choice.GetComponent<LayoutElement>();
                choiceSize.preferredWidth = 116f;
                choiceSize.minWidth = 116f;
                choice.onClick.AddListener(() => SelectRoom(capturedRoom));
            }
        }

        UpdateRoomPresentation();
    }

    void BuildHeader(Transform parent)
    {
        GameObject header = Panel(parent, "VastuHeader", Teal);
        Height(header, 94f);
        VerticalLayoutGroup layout = Stack(header, 18, 18, 15f);
        Text(header.transform, "Subtitle", "Guidance grounded in your scanned room and selected direction.", 15, FontStyles.Normal, Cream).alignment = TextAlignmentOptions.MidlineLeft;
    }

    void BuildRoomSelector(Transform parent)
    {
        GameObject card = Card(parent, "RoomSelection", 184f);
        VerticalLayoutGroup layout = Stack(card, 18, 18, 10f);
        Text(card.transform, "SectionTitle", "1. Select a room", 19, FontStyles.Bold, Ink);
        roomSummary = Text(card.transform, "RoomSummary", "Loading saved rooms...", 13, FontStyles.Normal, Muted);

        GameObject roomScroll = new GameObject("RoomChoicesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectMask2D));
        roomScroll.transform.SetParent(card.transform, false);
        LayoutElement scrollSize = roomScroll.AddComponent<LayoutElement>();
        scrollSize.preferredHeight = 58f;

        GameObject choices = new GameObject("RoomChoices", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        choices.transform.SetParent(roomScroll.transform, false);
        RectTransform choicesRect = choices.GetComponent<RectTransform>();
        choicesRect.anchorMin = new Vector2(0f, 0f);
        choicesRect.anchorMax = new Vector2(0f, 1f);
        choicesRect.pivot = new Vector2(0f, 0.5f);
        choicesRect.sizeDelta = Vector2.zero;
        HorizontalLayoutGroup choiceLayout = choices.GetComponent<HorizontalLayoutGroup>();
        choiceLayout.spacing = 8f;
        choiceLayout.padding = new RectOffset(1, 1, 0, 0);
        choiceLayout.childControlWidth = false;
        choiceLayout.childControlHeight = true;
        choiceLayout.childForceExpandWidth = false;
        ContentSizeFitter choicesFitter = choices.GetComponent<ContentSizeFitter>();
        choicesFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        choicesFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        ScrollRect roomScrollRect = roomScroll.GetComponent<ScrollRect>();
        roomScrollRect.viewport = roomScroll.GetComponent<RectTransform>();
        roomScrollRect.content = choicesRect;
        roomScrollRect.horizontal = true;
        roomScrollRect.vertical = false;
        roomScrollRect.movementType = ScrollRect.MovementType.Clamped;
        roomChoices = choices.transform;
    }

    void BuildDirectionSelector(Transform parent)
    {
        GameObject card = Card(parent, "Orientation", 166f);
        VerticalLayoutGroup layout = Stack(card, 18, 18, 10f);
        Text(card.transform, "SectionTitle", "2. Room orientation", 19, FontStyles.Bold, Ink);
        directionSummary = Text(card.transform, "DirectionSummary", "Room faces: North", 14, FontStyles.Normal, Muted);

        GameObject directions = new GameObject("DirectionChoices", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        directions.transform.SetParent(card.transform, false);
        HorizontalLayoutGroup directionLayout = directions.GetComponent<HorizontalLayoutGroup>();
        directionLayout.spacing = 8f;
        directionLayout.childControlWidth = true;
        directionLayout.childControlHeight = true;
        directionLayout.childForceExpandWidth = true;
        directionLayout.childAlignment = TextAnchor.MiddleCenter;
        LayoutElement elem = directions.AddComponent<LayoutElement>();
        elem.preferredHeight = 38f;

        AddDirection(directions.transform, "North", RoomDirection.North);
        AddDirection(directions.transform, "East", RoomDirection.East);
        AddDirection(directions.transform, "South", RoomDirection.South);
        AddDirection(directions.transform, "West", RoomDirection.West);
    }

    void BuildScore(Transform parent)
    {
        GameObject card = Card(parent, "VastuScore", 142f);
        VerticalLayoutGroup layout = Stack(card, 18, 18, 7f);
        Text(card.transform, "SectionTitle", "3. Vastu score", 19, FontStyles.Bold, Ink);
        scoreValue  = Text(card.transform, "Score", "-- / 100", 30, FontStyles.Bold, Teal);
        scoreStatus = Text(card.transform, "ScoreStatus", "Tap \"Scan with AI\" to get a real AI-powered score.", 14, FontStyles.Normal, Muted);
    }

    void BuildActions(Transform parent)
    {
        GameObject section = new GameObject("PriorityActions", typeof(RectTransform), typeof(VerticalLayoutGroup));
        section.transform.SetParent(parent, false);
        VerticalLayoutGroup layout = section.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        LayoutElement sectionSize = section.AddComponent<LayoutElement>();
        sectionSize.preferredHeight = 356f;  // Slightly taller to fit the new button

        Text(section.transform, "SectionTitle", "Priority actions", 21, FontStyles.Bold, Cream);
        AddAction(section.transform, "Keep the northeast open", "Use this zone for light, movement, and less visual clutter.", Sand);
        AddAction(section.transform, "Place heavy furniture southwest", "A sofa or storage unit is best kept toward the south or west where possible.", Mint);
        AddAction(section.transform, "Confirm the entrance", "For a detailed recommendation, ask the assistant where your entry sits.", Mint);

        // ── Scan with AI button ────────────────────────────────────────────
        // Calls VastuApiClient → FastAPI backend → InjectApiResults into chat.
        // The room type comes from the currently selected room's name
        // (falls back to "livingroom" if nothing is selected yet).
        Button scanBtn = Button(section.transform, "ScanWithAI", "Scan with AI", Teal, Cream, 50f);
        scanBtn.onClick.AddListener(OnScanWithAIClicked);
    }

    void OnScanWithAIClicked()
    {
        if (VastuApiClient.Instance == null)
        {
            UIManager.Instance?.ShowToast(
                "VastuApiClient not in scene. AppManager should create it automatically — " +
                "check that AppManager is in your Bootstrap scene.");
            return;
        }

        if (VastuApiClient.Instance.IsBusy)
        {
            UIManager.Instance?.ShowToast("Scan already in progress…");
            return;
        }

        // Infer room type from selected library room name, fall back to livingroom.
        string roomType = "livingroom";
        if (selectedRoom != null && !string.IsNullOrEmpty(selectedRoom.title))
        {
            string t = selectedRoom.title.ToLowerInvariant().Replace(" ", "");
            if      (t.Contains("bed"))   roomType = "bedroom";
            else if (t.Contains("kit"))   roomType = "kitchen";
            else if (t.Contains("bath"))  roomType = "bathroom";
            else if (t.Contains("study")) roomType = "studyroom";
        }

        scoreValue.text  = "…";
        scoreStatus.text = "AI scan running…";

        UIManager.Instance?.ShowToast($"Scanning {roomType}…");
        VastuApiClient.Instance.AnalyzeCurrentRoom(roomType);
    }

    /// <summary>
    /// Callback from VastuApiClient.OnAnalysisComplete — update the score card
    /// with real AI data without the user needing to tap anything extra.
    /// </summary>
    void OnApiAnalysisComplete(VastuApiResponse result)
    {
        if (result == null) return;

        scoreValue.text = result.overall_score + " / 100";

        // Use the named colour constants; this also prevents CS0414 "unused field" warnings.
        if (result.overall_score >= 80)
        {
            scoreStatus.text  = "Excellent layout. " + result.summary;
            scoreValue.color  = Good;
        }
        else if (result.overall_score >= 50)
        {
            scoreStatus.text  = "Good — some improvements possible. " + result.summary;
            scoreValue.color  = Warn;
        }
        else
        {
            scoreStatus.text  = "Needs attention. " + result.summary;
            scoreValue.color  = Bad;
        }
    }


    void BuildAssistant(Transform parent)
    {
        // Taller card so AI results (multiple items) fit without excessive scrolling.
        GameObject card = Card(parent, "VastuAssistant", 480f);
        VerticalLayoutGroup layout = Stack(card, 18, 18, 10f);
        Text(card.transform, "SectionTitle", "Vastu assistant", 20, FontStyles.Bold, Ink);
        Text(card.transform, "AssistantSub", "AI scan results appear here. Ask about furniture placement or kitchen zones.", 13, FontStyles.Normal, Muted);

        GameObject transcript = new GameObject("Transcript", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(RectMask2D));
        transcript.transform.SetParent(card.transform, false);
        transcript.GetComponent<Image>().color = new Color(0.93f, 0.96f, 0.94f, 1f);
        LayoutElement transcriptSize = transcript.AddComponent<LayoutElement>();
        transcriptSize.preferredHeight = 300f;

        // Inner content container that grows with messages.
        GameObject innerContent = new GameObject("InnerContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        innerContent.transform.SetParent(transcript.transform, false);
        RectTransform innerRect = innerContent.GetComponent<RectTransform>();
        innerRect.anchorMin = new Vector2(0f, 1f);
        innerRect.anchorMax = new Vector2(1f, 1f);
        innerRect.pivot     = new Vector2(0.5f, 1f);
        innerRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup transcriptLayout = innerContent.GetComponent<VerticalLayoutGroup>();
        transcriptLayout.padding              = new RectOffset(12, 12, 10, 10);
        transcriptLayout.spacing              = 7f;
        transcriptLayout.childControlWidth    = true;
        transcriptLayout.childControlHeight   = true;
        transcriptLayout.childForceExpandHeight = false;

        ContentSizeFitter innerFitter = innerContent.GetComponent<ContentSizeFitter>();
        innerFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = transcript.GetComponent<ScrollRect>();
        scrollRect.content    = innerRect;
        scrollRect.viewport   = transcript.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.vertical   = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        chatContent = innerContent.transform;

        GameObject inputRow = new GameObject("QuestionRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        inputRow.transform.SetParent(card.transform, false);
        HorizontalLayoutGroup inputLayout = inputRow.GetComponent<HorizontalLayoutGroup>();
        inputLayout.spacing              = 8f;
        inputLayout.childControlWidth    = true;
        inputLayout.childControlHeight   = true;
        inputLayout.childForceExpandWidth = false;

        questionInput = CreateInput(inputRow.transform);
        Button send = Button(inputRow.transform, "Ask", "Ask", Sand, Ink, 42f);
        LayoutElement sendSize = send.GetComponent<LayoutElement>();
        sendSize.preferredWidth = 72f;
        send.onClick.AddListener(SendQuestion);
    }

    void AddDirection(Transform parent, string label, RoomDirection direction)
    {
        Button button = Button(parent, label, label, Mint, Ink, 38f);
        button.onClick.AddListener(() =>
        {
            VastuAssistantManager.Instance?.SetDirection(direction);
            UpdateRoomPresentation();
        });
    }

    void AddAction(Transform parent, string title, string body, Color accent)
    {
        GameObject action = Panel(parent, "ActionCard", Cream);
        LayoutElement size = action.AddComponent<LayoutElement>();
        size.preferredHeight = 72f;
        VerticalLayoutGroup layout = Stack(action, 14, 14, 3f);
        Text(action.transform, "ActionTitle", title, 15, FontStyles.Bold, Teal);
        Text(action.transform, "ActionBody", body, 12, FontStyles.Normal, Muted);
        action.GetComponent<Image>().color = accent == Sand ? new Color(1f, 0.96f, 0.87f, 1f) : Cream;
    }

    void SelectRoom(LibraryItem room)
    {
        selectedRoom = room;
        RefreshRooms();
    }

    void UpdateRoomPresentation()
    {
        RoomDirection direction = VastuAssistantManager.Instance != null ? VastuAssistantManager.Instance.SelectedDirection : RoomDirection.North;
        directionSummary.text = "Room faces: " + direction + ". Change this to match the direction shown by your compass.";

        if (selectedRoom == null || selectedRoom.roomScanData == null)
        {
            roomSummary.text = "No measured room is selected.";
            scoreValue.text = "-- / 100";
            scoreStatus.text = "Save at least one measured wall or floor before analysing the layout.";
            return;
        }

        RoomScanData scan = selectedRoom.roomScanData;
        int surfaces = scan.wallCount + scan.floorCount;
        int score = Mathf.Clamp(64 + surfaces * 7 + (direction == RoomDirection.North || direction == RoomDirection.East ? 8 : 3), 0, 92);
        int roomIndex = rooms.IndexOf(selectedRoom) + 1;
        roomSummary.text = "Room " + roomIndex + " selected - " + scan.wallCount + " wall(s), " + scan.floorCount + " floor(s), " + (scan.totalFloorAreaSquareMeters + scan.totalWallAreaSquareMeters).ToString("0.0") + " m2 measured.";
        scoreValue.text = score + " / 100";
        scoreStatus.text = score >= 80 ? "Balanced starting point. Review the priority actions before arranging furniture." : "Good scan data is available. Follow the actions below to improve the layout.";
    }

    void SendQuestion()
    {
        if (questionInput == null || string.IsNullOrWhiteSpace(questionInput.text))
            return;

        VastuAssistantManager.Instance?.SendUserMessage(questionInput.text);
        questionInput.text = string.Empty;
    }

    void RefreshAssistant()
    {
        if (VastuAssistantManager.Instance == null)
            return;

        foreach (Transform child in chatContent)
            Destroy(child.gameObject);
        renderedMessages.Clear();
        foreach (VastuMessage message in VastuAssistantManager.Instance.GetMessages())
            RenderMessage(message);
    }

    void OnMessageAdded(VastuMessage message)
    {
        if (built)
            RenderMessage(message);
    }

    void RenderMessage(VastuMessage message)
    {
        if (message == null || renderedMessages.Contains(message) || chatContent == null)
            return;

        renderedMessages.Add(message);

        // Estimate line count from newlines to give multi-line AI results enough height.
        int lines = 1 + CountNewlines(message.text);
        float bubbleHeight = Mathf.Max(36f, lines * 22f + 12f);

        TextMeshProUGUI bubble = Text(chatContent, "Message",
            (message.isUser ? "You: " : "CoZee: ") + message.text,
            13,
            message.isUser ? FontStyles.Bold : FontStyles.Normal,
            Ink);
        bubble.enableWordWrapping = true;
        bubble.overflowMode       = TextOverflowModes.Overflow;
        LayoutElement le = bubble.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = bubbleHeight;
        le.flexibleWidth   = 1f;
        bubble.alignment   = message.isUser
            ? TextAlignmentOptions.MidlineRight
            : TextAlignmentOptions.TopLeft;
    }

    static int CountNewlines(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        int n = 0;
        foreach (char c in s) if (c == '\n') n++;
        return n;
    }

    static GameObject Card(Transform parent, string name, float height)
    {
        GameObject card = Panel(parent, name, Cream);
        Height(card, height);
        return card;
    }

    static GameObject Panel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        Image image = panel.GetComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = color;
        return panel;
    }

    static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        const int size = 48;
        const float radius = 11f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x - center.x) - (size * 0.5f - radius), 0f);
                float dy = Mathf.Max(Mathf.Abs(y - center.y) - (size * 0.5f - radius), 0f);
                bool inside = dx * dx + dy * dy <= radius * radius;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, inside ? 1f : 0f));
            }
        }

        texture.Apply(false, true);
        roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return roundedSprite;
    }

    static VerticalLayoutGroup Stack(GameObject target, int horizontalPadding, int verticalPadding, float spacing)
    {
        VerticalLayoutGroup layout = target.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(horizontalPadding, horizontalPadding, verticalPadding, verticalPadding);
        layout.spacing = spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        return layout;
    }

    static void Height(GameObject target, float value)
    {
        target.AddComponent<LayoutElement>().preferredHeight = value;
    }

    static Button Button(Transform parent, string name, string label, Color color, Color textColor, float height)
    {
        GameObject button = Panel(parent, name, color);
        button.AddComponent<Button>();
        LayoutElement size = button.AddComponent<LayoutElement>();
        size.preferredHeight = height;
        TextMeshProUGUI text = Text(button.transform, "Label", label, 14, FontStyles.Bold, textColor);
        text.alignment = TextAlignmentOptions.Center;
        Stretch(text.rectTransform);
        return button.GetComponent<Button>();
    }

    static TMP_InputField CreateInput(Transform parent)
    {
        GameObject root = Panel(parent, "QuestionInput", new Color(0.93f, 0.96f, 0.94f, 1f));
        LayoutElement size = root.AddComponent<LayoutElement>();
        size.flexibleWidth = 1f;
        size.preferredHeight = 42f;
        TMP_InputField input = root.AddComponent<TMP_InputField>();

        TextMeshProUGUI text = Text(root.transform, "Text", "", 14, FontStyles.Normal, Ink);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.rectTransform.offsetMin = new Vector2(12f, 0f);
        text.rectTransform.offsetMax = new Vector2(-8f, 0f);
        input.textComponent = text;

        TextMeshProUGUI placeholder = Text(root.transform, "Placeholder", "Ask about your room...", 14, FontStyles.Italic, Muted);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        placeholder.rectTransform.offsetMin = new Vector2(12f, 0f);
        placeholder.rectTransform.offsetMax = new Vector2(-8f, 0f);
        input.placeholder = placeholder;
        return input;
    }

    static TextMeshProUGUI Text(Transform parent, string name, string value, float size, FontStyles style, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.font = CoZeeTypography.FontFor(name, size, style);
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
