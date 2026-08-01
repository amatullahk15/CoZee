using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class MobileAppSceneBuilder
{
    const string UiScenePath = "Assets/Scenes/UI/";
    const string MainShellScenePath = UiScenePath + "MainShell.unity";

    // Modern Commercial Theme Colors (Slate Dark Palette)
    static readonly Color BgDark = new Color(0.06f, 0.09f, 0.16f, 1f);          // #0F172A Slate 900
    static readonly Color SurfaceDark = new Color(0.12f, 0.16f, 0.23f, 0.96f);     // #1E293B Slate 800
    static readonly Color SurfaceElevated = new Color(0.20f, 0.25f, 0.33f, 0.96f); // #334155 Slate 700
    static readonly Color PrimaryAccent = new Color(0.23f, 0.51f, 0.96f, 1f);    // #3B82F6 Royal Blue
    static readonly Color DangerAccent = new Color(0.94f, 0.27f, 0.27f, 1f);     // #EF4444 Coral Red
    static readonly Color SuccessAccent = new Color(0.16f, 0.72f, 0.53f, 1f);    // #10B981 Emerald
    static readonly Color TextPrimary = new Color(0.97f, 0.98f, 0.99f, 1f);     // #F8FAFC White
    static readonly Color TextMuted = new Color(0.58f, 0.64f, 0.72f, 1f);       // #94A3B8 Slate Muted

    [InitializeOnLoadMethod]
    static void AutoSetupIfMissing()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(MainShellScenePath))
            {
                Debug.LogWarning("ARFurniture: Missing UI scenes detected. Running Setup Mobile App Scenes...");
                SetupAllScenes();
            }
        };
    }

    [MenuItem("ARFurniture/Setup Mobile App Scenes")]
    public static void SetupAllScenes()
    {
        EnsureFolder("Assets/Scenes/UI");
        EnsureFolder("Assets/Editor");

        CreateBootstrap();
        CreateSplash();
        CreateOnboarding();
        CreatePermissions();
        CreateMainShell();

        RegisterBuildScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Mobile app UI scenes rebuilt successfully with full commercial AR interior design layouts!");
    }

    static void CreateBootstrap()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var bootstrap = new GameObject("Bootstrap");
        bootstrap.AddComponent<AppManager>();
        bootstrap.AddComponent<SceneLoader>();
        bootstrap.AddComponent<PermissionManager>();
        bootstrap.AddComponent<LibraryDataManager>();
        bootstrap.AddComponent<DesignAIManager>();
        bootstrap.AddComponent<VastuAssistantManager>();
        bootstrap.AddComponent<AudioManager>();

        var transition = new GameObject("SceneTransition");
        transition.transform.SetParent(bootstrap.transform);
        var fadeGo = CreateUiRoot("FadeOverlay", transition.transform);
        var fadeImage = fadeGo.AddComponent<Image>();
        fadeImage.color = Color.black;
        Stretch(fadeGo.GetComponent<RectTransform>());
        var transitionComp = transition.AddComponent<SceneTransition>();
        SetPrivateField(transitionComp, "fadeImage", fadeImage);

        EditorSceneManager.SaveScene(scene, UiScenePath + "Bootstrap.unity");
    }

    static void CreateSplash()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateEventSystem();

        var canvasGo = CreateCanvas("SplashCanvas", BgDark);
        var safeArea = AddSafeArea(canvasGo.transform);

        var panel = CreatePanel(safeArea.transform, "SplashPanel", Color.clear);
        Stretch(panel.GetComponent<RectTransform>());
        var cg = panel.AddComponent<CanvasGroup>();

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(48, 48, 140, 140);
        layout.spacing = 24;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        // Logo Icon Card
        var logoCard = CreatePanel(panel.transform, "LogoBadge", PrimaryAccent);
        var logoElem = logoCard.AddComponent<LayoutElement>();
        logoElem.preferredWidth = 140f;
        logoElem.preferredHeight = 140f;
        var logoText = CreateText(logoCard.transform, "LogoText", "AR", 56, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        Stretch(logoText.GetComponent<RectTransform>());

        // Title
        var title = CreateText(panel.transform, "Title", "AR Interior Design", 44, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        var titleElem = title.AddComponent<LayoutElement>();
        titleElem.preferredHeight = 80f;

        // Subtitle
        var subtitle = CreateText(panel.transform, "Subtitle", "Visualize & Transform Your Space in 3D", 22, FontStyles.Normal, TextAlignmentOptions.Center, Color.white);
        var subElem = subtitle.AddComponent<LayoutElement>();
        subElem.preferredHeight = 50f;

        // Loading Indicator Spinner Placeholder
        var loaderCard = CreatePanel(panel.transform, "LoadingSpinner", SurfaceElevated);
        var loaderElem = loaderCard.AddComponent<LayoutElement>();
        loaderElem.preferredWidth = 200f;
        loaderElem.preferredHeight = 48f;
        var loaderText = CreateText(loaderCard.transform, "SpinnerLabel", "Loading...", 18, FontStyles.Italic, TextAlignmentOptions.Center, TextMuted);
        Stretch(loaderText.GetComponent<RectTransform>());

        var controller = panel.AddComponent<SplashScreenController>();
        SetPrivateField(controller, "canvasGroup", cg);
        SetPrivateField(controller, "titleText", title.GetComponent<TextMeshProUGUI>());

        EditorSceneManager.SaveScene(scene, UiScenePath + "SplashScreen.unity");
    }

    static void CreateOnboarding()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateEventSystem();

        var canvasGo = CreateCanvas("OnboardingCanvas", BgDark);
        var safeArea = AddSafeArea(canvasGo.transform);

        var managerGo = new GameObject("OnboardingManager");
        var manager = managerGo.AddComponent<OnboardingManager>();
        SetPrivateField(manager, "slides", new OnboardingSlideData[]
        {
            new OnboardingSlideData { title = "3D Room Measurement", body = "Measure floor areas, walls, and captured room dimensions in real-time with AR.", icon = "📐" },
            new OnboardingSlideData { title = "AR Furniture Placement", body = "Preview 3D sofas, wardrobes, and decor items in high fidelity inside your home.", icon = "🛋️" },
            new OnboardingSlideData { title = "AI & Vastu Assistant", body = "Generate instant room design themes and consult smart Vastu directional layout guidance.", icon = "✨" }
        });

        var rootPanel = CreatePanel(safeArea.transform, "OnboardingPanel", Color.clear);
        Stretch(rootPanel.GetComponent<RectTransform>());

        var mainLayout = rootPanel.AddComponent<VerticalLayoutGroup>();
        mainLayout.padding = new RectOffset(36, 36, 36, 48);
        mainLayout.spacing = 24;
        mainLayout.childAlignment = TextAnchor.UpperCenter;
        mainLayout.childControlWidth = true;
        mainLayout.childControlHeight = false;

        // Top Bar with Skip Button
        var topBar = CreatePanel(rootPanel.transform, "TopBar", Color.clear);
        var topBarElem = topBar.AddComponent<LayoutElement>();
        topBarElem.preferredHeight = 60f;
        var topBarLayout = topBar.AddComponent<HorizontalLayoutGroup>();
        topBarLayout.childAlignment = TextAnchor.MiddleRight;
        topBarLayout.childControlWidth = false;

        var skipBtn = CreateButton(topBar.transform, "SkipButton", "Skip", SurfaceElevated, Color.white, 52f, 120f);

        // Center Slide Container
        var slideViewGo = CreatePanel(rootPanel.transform, "SlideView", Color.clear);
        var slideViewElem = slideViewGo.AddComponent<LayoutElement>();
        slideViewElem.flexibleHeight = 1f;

        var slideViewLayout = slideViewGo.AddComponent<VerticalLayoutGroup>();
        slideViewLayout.spacing = 24;
        slideViewLayout.childAlignment = TextAnchor.MiddleCenter;
        slideViewLayout.childControlWidth = true;
        slideViewLayout.childControlHeight = false;

        // Hero Card Illustration Badge
        var heroCard = CreatePanel(slideViewGo.transform, "HeroCard", SurfaceDark);
        var heroElem = heroCard.AddComponent<LayoutElement>();
        heroElem.preferredHeight = 280f;
        var heroLayout = heroCard.AddComponent<VerticalLayoutGroup>();
        heroLayout.childAlignment = TextAnchor.MiddleCenter;

        var heroIcon = CreateText(heroCard.transform, "HeroIcon", "📐", 80, FontStyles.Bold, TextAlignmentOptions.Center, PrimaryAccent);

        var slideTitle = CreateText(slideViewGo.transform, "SlideTitle", "3D Room Measurement", 32, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        var titleElem = slideTitle.AddComponent<LayoutElement>();
        titleElem.preferredHeight = 60f;

        var slideBody = CreateText(slideViewGo.transform, "SlideBody", "Measure floor areas, walls, and captured room dimensions in real-time with AR.", 20, FontStyles.Normal, TextAlignmentOptions.Center, Color.white);
        var bodyElem = slideBody.AddComponent<LayoutElement>();
        bodyElem.preferredHeight = 100f;

        var slideView = slideViewGo.AddComponent<OnboardingSlideView>();
        SetPrivateField(slideView, "iconText", heroIcon.GetComponent<TextMeshProUGUI>());
        SetPrivateField(slideView, "titleText", slideTitle.GetComponent<TextMeshProUGUI>());
        SetPrivateField(slideView, "bodyText", slideBody.GetComponent<TextMeshProUGUI>());

        // Bottom Navigation Controls (Dots + Next Button)
        var bottomStack = CreatePanel(rootPanel.transform, "BottomStack", Color.clear);
        var bottomElem = bottomStack.AddComponent<LayoutElement>();
        bottomElem.preferredHeight = 160f;
        var bottomLayout = bottomStack.AddComponent<VerticalLayoutGroup>();
        bottomLayout.spacing = 24;
        bottomLayout.childAlignment = TextAnchor.LowerCenter;
        bottomLayout.childControlWidth = true;
        bottomLayout.childControlHeight = false;

        var dots = CreatePanel(bottomStack.transform, "Dots", Color.clear);
        var dotsElem = dots.AddComponent<LayoutElement>();
        dotsElem.preferredHeight = 32f;
        var dotsLayout = dots.AddComponent<HorizontalLayoutGroup>();
        dotsLayout.spacing = 16;
        dotsLayout.childAlignment = TextAnchor.MiddleCenter;
        dotsLayout.childControlWidth = false;
        dotsLayout.childControlHeight = false;

        var nextBtn = CreateButton(bottomStack.transform, "NextButton", "Next", PrimaryAccent, Color.white, 72f);

        var carousel = rootPanel.AddComponent<OnboardingCarousel>();
        SetPrivateField(carousel, "onboardingManager", manager);
        SetPrivateField(carousel, "slideView", slideView);
        SetPrivateField(carousel, "nextButton", nextBtn.GetComponent<Button>());
        SetPrivateField(carousel, "skipButton", skipBtn.GetComponent<Button>());
        SetPrivateField(carousel, "nextButtonLabel", nextBtn.GetComponentInChildren<TextMeshProUGUI>());
        SetPrivateField(carousel, "dotsContainer", dots.transform);

        // Dot Prefab template
        var dotPrefab = CreatePanel(dots.transform, "DotPrefab", Color.white);
        var dotRect = dotPrefab.GetComponent<RectTransform>();
        dotRect.sizeDelta = new Vector2(16f, 16f);
        dotPrefab.SetActive(false);
        SetPrivateField(carousel, "dotPrefab", dotPrefab);

        EditorSceneManager.SaveScene(scene, UiScenePath + "Onboarding.unity");
    }

    static void CreatePermissions()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateEventSystem();

        var canvasGo = CreateCanvas("PermissionsCanvas", BgDark);
        var safeArea = AddSafeArea(canvasGo.transform);

        var panel = CreatePanel(safeArea.transform, "PermissionsPanel", Color.clear);
        Stretch(panel.GetComponent<RectTransform>());

        var mainLayout = panel.AddComponent<VerticalLayoutGroup>();
        mainLayout.padding = new RectOffset(36, 36, 48, 48);
        mainLayout.spacing = 24;
        mainLayout.childAlignment = TextAnchor.UpperCenter;
        mainLayout.childControlWidth = true;
        mainLayout.childControlHeight = false;

        // Header
        var headerPanel = CreatePanel(panel.transform, "Header", Color.clear);
        var headerElem = headerPanel.AddComponent<LayoutElement>();
        headerElem.preferredHeight = 120f;
        var headerLayout = headerPanel.AddComponent<VerticalLayoutGroup>();
        headerLayout.spacing = 8;
        headerLayout.childAlignment = TextAnchor.MiddleLeft;

        CreateText(headerPanel.transform, "HeaderTitle", "App Permissions", 36, FontStyles.Bold, TextAlignmentOptions.Left, TextPrimary);
        CreateText(headerPanel.transform, "HeaderSub", "To enable real-time room measurement and AR furniture preview, please allow camera and sensor access.", 18, FontStyles.Normal, TextAlignmentOptions.Left, TextMuted);

        // Rows Container
        var listContainer = CreatePanel(panel.transform, "ListContainer", Color.clear);
        var listElem = listContainer.AddComponent<LayoutElement>();
        listElem.flexibleHeight = 1f;
        var listLayout = listContainer.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 20;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = false;

        var cameraRow = CreatePermissionRow(listContainer.transform, "CameraRow", "Camera Access", "Required for AR plane detection & camera view");
        var photosRow = CreatePermissionRow(listContainer.transform, "PhotosRow", "Photo & Storage", "Save generated interior designs & room scans");
        var motionRow = CreatePermissionRow(listContainer.transform, "MotionRow", "Motion Sensors", "Accurate 6DoF spatial tracking & measurement");

        var continueBtn = CreateButton(panel.transform, "ContinueButton", "Continue to App", PrimaryAccent, TextPrimary, 72f);

        var controller = panel.AddComponent<PermissionsScreenController>();
        SetPrivateField(controller, "cameraRow", cameraRow);
        SetPrivateField(controller, "photosRow", photosRow);
        SetPrivateField(controller, "motionRow", motionRow);
        SetPrivateField(controller, "continueButton", continueBtn.GetComponent<Button>());

        EditorSceneManager.SaveScene(scene, UiScenePath + "Permissions.unity");
    }

    static PermissionRowUI CreatePermissionRow(Transform parent, string goName, string titleStr, string descStr)
    {
        var card = CreatePanel(parent, goName, SurfaceDark);
        var cardElem = card.AddComponent<LayoutElement>();
        cardElem.preferredHeight = 120f;

        var layout = card.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 16, 16);
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        // Icon Box
        var iconBox = CreatePanel(card.transform, "IconBox", SurfaceElevated);
        var iconElem = iconBox.AddComponent<LayoutElement>();
        iconElem.preferredWidth = 64f;
        iconElem.preferredHeight = 64f;
        var iconText = CreateText(iconBox.transform, "IconText", "✓", 28, FontStyles.Bold, TextAlignmentOptions.Center, PrimaryAccent);
        Stretch(iconText.GetComponent<RectTransform>());

        // Text Column
        var textCol = CreatePanel(card.transform, "TextCol", Color.clear);
        var textElem = textCol.AddComponent<LayoutElement>();
        textElem.preferredWidth = 440f;
        textElem.flexibleWidth = 1f;

        var colLayout = textCol.AddComponent<VerticalLayoutGroup>();
        colLayout.childAlignment = TextAnchor.MiddleLeft;
        colLayout.spacing = 4;

        var title = CreateText(textCol.transform, "Title", titleStr, 24, FontStyles.Bold, TextAlignmentOptions.Left, TextPrimary);
        var desc = CreateText(textCol.transform, "Status", descStr, 16, FontStyles.Normal, TextAlignmentOptions.Left, TextMuted);

        // Allow Button
        var btn = CreateButton(card.transform, "RequestBtn", "Allow", SurfaceElevated, TextPrimary, 52f, 140f);

        var rowUi = card.AddComponent<PermissionRowUI>();
        SetPrivateField(rowUi, "titleText", title.GetComponent<TextMeshProUGUI>());
        SetPrivateField(rowUi, "statusText", desc.GetComponent<TextMeshProUGUI>());
        SetPrivateField(rowUi, "requestButton", btn.GetComponent<Button>());
        return rowUi;
    }

    static void CreateMainShell()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateEventSystem();

        var canvasGo = CreateCanvas("MainShellCanvas", BgDark);
        var safeArea = AddSafeArea(canvasGo.transform);

        // Content Panel (Top Region above BottomNav)
        var content = CreatePanel(safeArea.transform, "Content", Color.clear);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(0f, 90f); // 90px bottom navigation height
        contentRect.offsetMax = Vector2.zero;

        var home = CreateTabScreen<HomeDashboardController>(content.transform, "HomeTab");
        var scan = CreateTabScreen<ScanARScreenController>(content.transform, "ScanARTab");
        var design = CreateTabScreen<DesignAIController>(content.transform, "DesignAITab");
        var vastu = CreateTabScreen<VastuScreenController>(content.transform, "VastuTab");
        var library = CreateTabScreen<LibraryScreenController>(content.transform, "LibraryTab");

        WireHomeTab(home);
        WireScanARTab(scan);
        WireDesignAITab(design);
        WireVastuTab(vastu);
        WireLibraryTab(library);

        var navGo = new GameObject("NavigationManager");
        var nav = navGo.AddComponent<NavigationManager>();
        SetPrivateField(nav, "tabScreens", new ScreenBase[]
        {
            home.GetComponent<ScreenBase>(),
            scan.GetComponent<ScreenBase>(),
            design.GetComponent<ScreenBase>(),
            vastu.GetComponent<ScreenBase>(),
            library.GetComponent<ScreenBase>()
        });

        // Bottom Navigation Bar
        var bottomNav = CreatePanel(safeArea.transform, "BottomNav", SurfaceDark);
        var bottomRect = bottomNav.GetComponent<RectTransform>();
        bottomRect.anchorMin = new Vector2(0, 0);
        bottomRect.anchorMax = new Vector2(1, 0);
        bottomRect.pivot = new Vector2(0.5f, 0);
        bottomRect.sizeDelta = new Vector2(0, 90);
        bottomRect.anchoredPosition = Vector2.zero;

        var bottomLayout = bottomNav.AddComponent<HorizontalLayoutGroup>();
        bottomLayout.padding = new RectOffset(16, 16, 12, 12);
        bottomLayout.spacing = 8;
        bottomLayout.childAlignment = TextAnchor.MiddleCenter;
        bottomLayout.childControlWidth = true;
        bottomLayout.childControlHeight = true;
        bottomLayout.childForceExpandHeight = false;
        bottomLayout.childForceExpandWidth = true;

        var navBar = bottomNav.AddComponent<BottomNavBar>();
        var tabs = new BottomNavBar.TabButton[5];
        string[] labels = { "Home", "Scan AR", "Design AI", "Vastu", "Library" };
        string[] icons = { "🏠", "📐", "✨", "🧭", "📚" };

        for (int i = 0; i < 5; i++)
        {
            var tabBtn = CreateNavTabButton(bottomNav.transform, labels[i] + "Btn", labels[i], icons[i]);
            tabs[i] = new BottomNavBar.TabButton
            {
                button = tabBtn.GetComponent<Button>(),
                highlight = tabBtn.GetComponent<Image>(),
                tab = (AppTab)i
            };
        }
        SetPrivateField(navBar, "tabs", tabs);

        // Toast System
        var uiMgrGo = new GameObject("UIManager");
        var uiMgr = uiMgrGo.AddComponent<UIManager>();

        var toastPanel = CreatePanel(safeArea.transform, "Toast", SurfaceElevated);
        var toastRect = toastPanel.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0.1f, 0.88f);
        toastRect.anchorMax = new Vector2(0.9f, 0.96f);
        toastRect.offsetMin = Vector2.zero;
        toastRect.offsetMax = Vector2.zero;

        var toastCg = toastPanel.AddComponent<CanvasGroup>();
        var toastText = CreateText(toastPanel.transform, "Message", "Notification Message", 20, FontStyles.Normal, TextAlignmentOptions.Center, TextPrimary);
        Stretch(toastText.GetComponent<RectTransform>());

        var toast = toastPanel.AddComponent<ToastNotification>();
        SetPrivateField(toast, "canvasGroup", toastCg);
        SetPrivateField(toast, "messageText", toastText.GetComponent<TextMeshProUGUI>());
        SetPrivateField(uiMgr, "toast", toast);

        EditorSceneManager.SaveScene(scene, UiScenePath + "MainShell.unity");
    }

    static GameObject CreateNavTabButton(Transform parent, string name, string label, string icon)
    {
        var btn = CreatePanel(parent, name, SurfaceDark);
        btn.AddComponent<Button>();

        var layout = btn.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(4, 4, 8, 8);
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        var iconText = CreateText(btn.transform, "Icon", icon, 32, FontStyles.Bold, TextAlignmentOptions.Center, TextPrimary);
        var iconElem = iconText.AddComponent<LayoutElement>();
        iconElem.preferredHeight = 40f;

        var labelText = CreateText(btn.transform, "Label", label, 16, FontStyles.Bold, TextAlignmentOptions.Center, TextMuted);
        var labelElem = labelText.AddComponent<LayoutElement>();
        labelElem.preferredHeight = 24f;

        return btn;
    }

    static void WireHomeTab(GameObject tab)
    {
        // Scroll Container for vertical scrolling
        var scrollGo = CreateUiRoot("ScrollView", tab.transform);
        Stretch(scrollGo.GetComponent<RectTransform>());
        var scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        var viewport = CreateUiRoot("Viewport", scrollGo.transform);
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.AddComponent<RectMask2D>();

        var content = CreateUiRoot("Content", viewport.transform);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 0f);

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = contentRect;

        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 80);
        layout.spacing = 24;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // 1. Top Header Bar (Avatar + Greeting + Bell)
        var topBar = CreatePanel(content.transform, "TopBar", Color.clear);
        var topBarElem = topBar.AddComponent<LayoutElement>();
        topBarElem.preferredHeight = 56f;
        topBarElem.flexibleWidth = 1f;

        var topLayout = topBar.AddComponent<HorizontalLayoutGroup>();
        topLayout.childAlignment = TextAnchor.MiddleLeft;
        topLayout.spacing = 16;
        topLayout.childControlWidth = true;
        topLayout.childControlHeight = true;
        topLayout.childForceExpandWidth = false;
        topLayout.childForceExpandHeight = false;

        // Avatar Icon
        var avatar = CreatePanel(topBar.transform, "Avatar", SurfaceDark);
        var avElem = avatar.AddComponent<LayoutElement>();
        avElem.preferredWidth = 48f;
        avElem.preferredHeight = 48f;
        avElem.flexibleWidth = 0f;
        var avText = CreateText(avatar.transform, "AvText", "👤", 24, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        Stretch(avText.GetComponent<RectTransform>());

        // User Text Stack
        var userStack = CreatePanel(topBar.transform, "UserStack", Color.clear);
        var userElem = userStack.AddComponent<LayoutElement>();
        userElem.flexibleWidth = 1f;

        var userLayout = userStack.AddComponent<VerticalLayoutGroup>();
        userLayout.spacing = 2;
        userLayout.childAlignment = TextAnchor.MiddleLeft;
        userLayout.childControlWidth = true;
        userLayout.childControlHeight = true;
        userLayout.childForceExpandWidth = true;
        userLayout.childForceExpandHeight = false;

        CreateText(userStack.transform, "WelcomeTag", "WELCOME BACK", 12, FontStyles.Bold, TextAlignmentOptions.Left, TextMuted);
        CreateText(userStack.transform, "GreetingText", "Good morning, Alex", 22, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);

        // Bell Button
        var bellBtn = CreatePanel(topBar.transform, "BellBtn", SurfaceDark);
        var bellElem = bellBtn.AddComponent<LayoutElement>();
        bellElem.preferredWidth = 44f;
        bellElem.preferredHeight = 44f;
        bellElem.flexibleWidth = 0f;
        var bellText = CreateText(bellBtn.transform, "BellText", "🔔", 20, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        Stretch(bellText.GetComponent<RectTransform>());

        // 2. Hero Banner Card ("Start New Scan")
        var heroCard = CreatePanel(content.transform, "HeroBannerCard", SurfaceDark);
        var heroElem = heroCard.AddComponent<LayoutElement>();
        heroElem.preferredHeight = 170f;
        heroElem.flexibleWidth = 1f;

        var heroLayout = heroCard.AddComponent<HorizontalLayoutGroup>();
        heroLayout.padding = new RectOffset(24, 24, 24, 24);
        heroLayout.spacing = 16;
        heroLayout.childAlignment = TextAnchor.MiddleCenter;
        heroLayout.childControlWidth = true;
        heroLayout.childControlHeight = true;
        heroLayout.childForceExpandWidth = false;
        heroLayout.childForceExpandHeight = false;

        var heroTextCol = CreatePanel(heroCard.transform, "HeroTextCol", Color.clear);
        var heroColElem = heroTextCol.AddComponent<LayoutElement>();
        heroColElem.flexibleWidth = 1f;

        var heroColLayout = heroTextCol.AddComponent<VerticalLayoutGroup>();
        heroColLayout.spacing = 8;
        heroColLayout.childAlignment = TextAnchor.MiddleLeft;
        heroColLayout.childControlWidth = true;
        heroColLayout.childControlHeight = true;
        heroColLayout.childForceExpandWidth = true;
        heroColLayout.childForceExpandHeight = false;

        CreateText(heroTextCol.transform, "HeroTitle", "Start New\nScan", 26, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        CreateText(heroTextCol.transform, "HeroSub", "Analyze your space for Vastu harmony", 14, FontStyles.Normal, TextAlignmentOptions.Left, TextMuted);

        var scanPillBtn = CreateButton(heroCard.transform, "ScanPillBtn", "🔲 Scan Room", new Color(0.58f, 0.77f, 0.99f, 1f), new Color(0.12f, 0.23f, 0.54f, 1f), 48f, 140f);
        var pillElem = scanPillBtn.GetComponent<LayoutElement>();
        pillElem.flexibleWidth = 0f;

        // 3. Quick Actions Section (2x2 Grid)
        var actHeader = CreateText(content.transform, "SectionHeader", "Quick Actions", 22, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        var actHeadElem = actHeader.AddComponent<LayoutElement>();
        actHeadElem.preferredHeight = 32f;
        actHeadElem.flexibleWidth = 1f;

        var actionGrid = CreatePanel(content.transform, "ActionGrid", Color.clear);
        var gridElem = actionGrid.AddComponent<LayoutElement>();
        gridElem.preferredHeight = 250f;
        gridElem.flexibleWidth = 1f;

        var gridVertLayout = actionGrid.AddComponent<VerticalLayoutGroup>();
        gridVertLayout.spacing = 12;
        gridVertLayout.childControlWidth = true;
        gridVertLayout.childControlHeight = true;
        gridVertLayout.childForceExpandWidth = true;
        gridVertLayout.childForceExpandHeight = false;

        // Row 1
        var row1 = CreatePanel(actionGrid.transform, "Row1", Color.clear);
        var r1Elem = row1.AddComponent<LayoutElement>();
        r1Elem.preferredHeight = 114f;
        r1Elem.flexibleWidth = 1f;
        var r1Layout = row1.AddComponent<HorizontalLayoutGroup>();
        r1Layout.spacing = 12;
        r1Layout.childControlWidth = true;
        r1Layout.childControlHeight = true;
        r1Layout.childForceExpandWidth = false;
        r1Layout.childForceExpandHeight = false;

        var scan = CreateQuickAction(row1.transform, "ScanAction", "Scan Room", "MEASURE & MAP", "📐", AppTab.ScanAR);
        var design = CreateQuickAction(row1.transform, "DesignAction", "AI Design", "PROFOUND REVAMP", "✨", AppTab.DesignAI);

        // Row 2
        var row2 = CreatePanel(actionGrid.transform, "Row2", Color.clear);
        var r2Elem = row2.AddComponent<LayoutElement>();
        r2Elem.preferredHeight = 114f;
        r2Elem.flexibleWidth = 1f;
        var r2Layout = row2.AddComponent<HorizontalLayoutGroup>();
        r2Layout.spacing = 12;
        r2Layout.childControlWidth = true;
        r2Layout.childControlHeight = true;
        r2Layout.childForceExpandWidth = false;
        r2Layout.childForceExpandHeight = false;

        var vastu = CreateQuickAction(row2.transform, "VastuAction", "Vastu Check", "ENERGY FLOW", "🧭", AppTab.Vastu);
        var saved = CreateQuickAction(row2.transform, "SavedAction", "Library", "PAST PROJECTS", "📚", AppTab.Library);

        // Wire Scan Pill button too
        var scanPillComp = scanPillBtn.GetComponent<Button>();
        if (scanPillComp != null)
        {
            scanPillComp.onClick.AddListener(() => NavigationManager.Instance?.SelectTab(AppTab.ScanAR));
        }

        // 4. Recent Projects Section
        var projHeaderRow = CreatePanel(content.transform, "ProjHeaderRow", Color.clear);
        var projHeadElem = projHeaderRow.AddComponent<LayoutElement>();
        projHeadElem.preferredHeight = 32f;
        projHeadElem.flexibleWidth = 1f;

        var projHeadLayout = projHeaderRow.AddComponent<HorizontalLayoutGroup>();
        projHeadLayout.childAlignment = TextAnchor.MiddleCenter;
        projHeadLayout.childControlWidth = true;
        projHeadLayout.childControlHeight = true;
        projHeadLayout.childForceExpandWidth = false;
        projHeadLayout.childForceExpandHeight = false;

        var recentTitle = CreateText(projHeaderRow.transform, "RecentTitle", "Recent Projects", 22, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        var recentTitleElem = recentTitle.AddComponent<LayoutElement>();
        recentTitleElem.flexibleWidth = 1f;

        var viewAllBtn = CreateText(projHeaderRow.transform, "ViewAllBtn", "View all", 15, FontStyles.Bold, TextAlignmentOptions.Right, PrimaryAccent);
        var viewAllElem = viewAllBtn.AddComponent<LayoutElement>();
        viewAllElem.preferredWidth = 100f;
        viewAllElem.flexibleWidth = 0f;

        var featCard = CreatePanel(content.transform, "FeaturedCard", SurfaceDark);
        var featElem = featCard.AddComponent<LayoutElement>();
        featElem.preferredHeight = 150f;
        featElem.flexibleWidth = 1f;

        var featLayout = featCard.AddComponent<VerticalLayoutGroup>();
        featLayout.padding = new RectOffset(24, 24, 20, 20);
        featLayout.spacing = 10;
        featLayout.childAlignment = TextAnchor.UpperLeft;
        featLayout.childControlWidth = true;
        featLayout.childControlHeight = true;
        featLayout.childForceExpandWidth = true;
        featLayout.childForceExpandHeight = false;

        CreateText(featCard.transform, "FeatStatus", "● 92 Score  •  Harmony Achieved", 14, FontStyles.Bold, TextAlignmentOptions.Left, SuccessAccent);
        CreateText(featCard.transform, "FeatTitle", "Main Living Room", 22, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        CreateText(featCard.transform, "FeatDesc", "Dimensions: 4.2m x 5.0m • 3D Furniture Items Placed (Sofa, Wardrobe)", 15, FontStyles.Normal, TextAlignmentOptions.Left, TextMuted);

        // 5. Vastu Wisdom Section Card
        var vastuCard = CreatePanel(content.transform, "VastuWisdomCard", SurfaceDark);
        var vastuElem = vastuCard.AddComponent<LayoutElement>();
        vastuElem.preferredHeight = 160f;
        vastuElem.flexibleWidth = 1f;

        var vastuCardLayout = vastuCard.AddComponent<VerticalLayoutGroup>();
        vastuCardLayout.padding = new RectOffset(24, 24, 20, 20);
        vastuCardLayout.spacing = 10;
        vastuCardLayout.childAlignment = TextAnchor.UpperLeft;
        vastuCardLayout.childControlWidth = true;
        vastuCardLayout.childControlHeight = true;
        vastuCardLayout.childForceExpandWidth = true;
        vastuCardLayout.childForceExpandHeight = false;

        CreateText(vastuCard.transform, "VastuTag", "💡 VASTU WISDOM", 12, FontStyles.Bold, TextAlignmentOptions.Left, TextMuted);
        CreateText(vastuCard.transform, "VastuBody", "Placing a mirror on the North wall of your living room can double the flow of positive energy.", 15, FontStyles.Normal, TextAlignmentOptions.Left, Color.white);
        CreateText(vastuCard.transform, "VastuLink", "Learn more →", 14, FontStyles.Bold, TextAlignmentOptions.Left, PrimaryAccent);

        var ctrl = tab.GetComponent<HomeDashboardController>();
        SetPrivateField(ctrl, "scanAction", scan);
        SetPrivateField(ctrl, "designAction", design);
        SetPrivateField(ctrl, "vastuAction", vastu);
        SetPrivateField(ctrl, "savedAction", saved);
        SetPrivateField(ctrl, "recentListRoot", featCard.transform);
    }

    static QuickActionButton CreateQuickAction(Transform parent, string name, string label, string sublabel, string icon, AppTab tab)
    {
        var card = CreatePanel(parent, name, SurfaceDark);
        card.AddComponent<Button>();

        var layoutElem = card.AddComponent<LayoutElement>();
        layoutElem.flexibleWidth = 1f;

        var layout = card.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 14, 14);
        layout.spacing = 6;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // Top-left Icon Badge
        var iconBadge = CreatePanel(card.transform, "IconBadge", SurfaceElevated);
        var iconBadgeElem = iconBadge.AddComponent<LayoutElement>();
        iconBadgeElem.preferredWidth = 36f;
        iconBadgeElem.preferredHeight = 36f;
        iconBadgeElem.flexibleWidth = 0f;

        var iconText = CreateText(iconBadge.transform, "IconText", icon, 20, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        Stretch(iconText.GetComponent<RectTransform>());

        // Main Label
        var labelText = CreateText(card.transform, "Label", label, 16, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        var labelElem = labelText.AddComponent<LayoutElement>();
        labelElem.preferredHeight = 22f;

        // Sub Label
        var subText = CreateText(card.transform, "SubLabel", sublabel, 11, FontStyles.Bold, TextAlignmentOptions.Left, TextMuted);
        var subElem = subText.AddComponent<LayoutElement>();
        subElem.preferredHeight = 16f;

        var action = card.AddComponent<QuickActionButton>();
        SetPrivateField(action, "button", card.GetComponent<Button>());
        SetPrivateField(action, "labelText", labelText.GetComponent<TextMeshProUGUI>());
        SetPrivateField(action, "targetTab", tab);
        return action;
    }

    static void WireScanARTab(GameObject scanTab)
    {
        var bridgeGo = new GameObject("ARSessionBridge");
        bridgeGo.transform.SetParent(scanTab.transform, false);
        var bridge = bridgeGo.AddComponent<ARSessionBridge>();

        // 1. Top Header Bar (Back Button + Status Card + Settings)
        var topBar = CreatePanel(scanTab.transform, "ARTopBar", Color.clear);
        var topBarRect = topBar.GetComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0.03f, 0.88f);
        topBarRect.anchorMax = new Vector2(0.97f, 0.98f);
        topBarRect.offsetMin = Vector2.zero;
        topBarRect.offsetMax = Vector2.zero;

        var topLayout = topBar.AddComponent<HorizontalLayoutGroup>();
        topLayout.spacing = 12;
        topLayout.childAlignment = TextAnchor.MiddleCenter;
        topLayout.childControlWidth = true;
        topLayout.childControlHeight = true;

        var backBtn = CreateButton(topBar.transform, "ARBackButton", "← Back", SurfaceDark, Color.white, 56f, 100f);

        var statusCard = CreatePanel(topBar.transform, "MeasurementStatusCard", SurfaceDark);
        var statusCardElem = statusCard.AddComponent<LayoutElement>();
        statusCardElem.flexibleWidth = 1f;

        var statusLayout = statusCard.AddComponent<VerticalLayoutGroup>();
        statusLayout.padding = new RectOffset(16, 16, 8, 8);
        statusLayout.spacing = 2;
        statusLayout.childAlignment = TextAnchor.MiddleCenter;

        var statusText = CreateText(statusCard.transform, "MeasurementStatus", "Tap 1st corner to measure room", 18, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        var dimText = CreateText(statusCard.transform, "RoomDimensions", "Width: -- m | Length: -- m | Area: -- m² • Compass: 15° N", 13, FontStyles.Normal, TextAlignmentOptions.Center, TextMuted);

        var settingsBtn = CreateButton(topBar.transform, "ARSettingsBtn", "⚙️", SurfaceDark, Color.white, 56f, 56f);

        var statusUi = statusCard.AddComponent<MeasurementStatusUI>();
        SetPrivateField(statusUi, "bridge", bridge);
        SetPrivateField(statusUi, "statusText", statusText.GetComponent<TextMeshProUGUI>());
        SetPrivateField(statusUi, "dimText", dimText.GetComponent<TextMeshProUGUI>());

        // 2. Center Crosshair Indicator
        var crosshair = CreatePanel(scanTab.transform, "Crosshair", Color.clear);
        var crosshairRect = crosshair.GetComponent<RectTransform>();
        crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRect.sizeDelta = new Vector2(80f, 80f);
        crosshairRect.anchoredPosition = Vector2.zero;

        var chText = CreateText(crosshair.transform, "CHText", "⌖", 48, FontStyles.Bold, TextAlignmentOptions.Center, PrimaryAccent);
        Stretch(chText.GetComponent<RectTransform>());

        // 3. Floating AR Right Controls (Rotate, Delete, Reset) - Fades in on selection!
        var controls = CreatePanel(scanTab.transform, "ARControls", SurfaceDark);
        var controlsRect = controls.GetComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(0.82f, 0.28f);
        controlsRect.anchorMax = new Vector2(0.97f, 0.68f);
        controlsRect.offsetMin = Vector2.zero;
        controlsRect.offsetMax = Vector2.zero;

        var controlsLayout = controls.AddComponent<VerticalLayoutGroup>();
        controlsLayout.padding = new RectOffset(8, 8, 12, 12);
        controlsLayout.spacing = 10;
        controlsLayout.childAlignment = TextAnchor.MiddleCenter;
        controlsLayout.childControlWidth = true;
        controlsLayout.childControlHeight = true;

        var controlsUi = controls.AddComponent<ARControlsOverlay>();
        var rotL = CreateButton(controls.transform, "RotLeft", "⟲ Left", SurfaceElevated, Color.white, 52f);
        var rotR = CreateButton(controls.transform, "RotRight", "⟳ Right", SurfaceElevated, Color.white, 52f);
        var del = CreateButton(controls.transform, "Delete", "🗑 Delete", DangerAccent, Color.white, 52f);
        var reset = CreateButton(controls.transform, "Reset", "↺ Reset", SurfaceElevated, Color.white, 52f);

        SetPrivateField(controlsUi, "rotateLeftButton", rotL.GetComponent<Button>());
        SetPrivateField(controlsUi, "rotateRightButton", rotR.GetComponent<Button>());
        SetPrivateField(controlsUi, "deleteButton", del.GetComponent<Button>());
        SetPrivateField(controlsUi, "resetButton", reset.GetComponent<Button>());

        // 4. Floating Bottom Furniture Tray
        var tray = CreatePanel(scanTab.transform, "FurnitureTray", SurfaceDark);
        var trayRect = tray.GetComponent<RectTransform>();
        trayRect.anchorMin = new Vector2(0.03f, 0.04f);
        trayRect.anchorMax = new Vector2(0.97f, 0.16f);
        trayRect.offsetMin = Vector2.zero;
        trayRect.offsetMax = Vector2.zero;

        var trayLayout = tray.AddComponent<HorizontalLayoutGroup>();
        trayLayout.padding = new RectOffset(16, 16, 12, 12);
        trayLayout.spacing = 12;
        trayLayout.childAlignment = TextAnchor.MiddleCenter;
        trayLayout.childControlWidth = true;
        trayLayout.childControlHeight = true;

        var sofaBtn = CreateButton(tray.transform, "SofaBtn", "🛋 Sofa", SurfaceElevated, Color.white, 56f);
        var wardrobeBtn = CreateButton(tray.transform, "WardrobeBtn", "🚪 Wardrobe", SurfaceElevated, Color.white, 56f);
        var measureBtn = CreateButton(tray.transform, "MeasureBtn", "📐 Measure", SurfaceElevated, Color.white, 56f);
        var saveBtn = CreateButton(tray.transform, "SaveRoomBtn", "💾 Save Room", PrimaryAccent, Color.white, 56f);

        var trayUi = tray.AddComponent<ARFurnitureTrayUI>();
        SetPrivateField(trayUi, "sofaButton", sofaBtn.GetComponent<Button>());
        SetPrivateField(trayUi, "wardrobeButton", wardrobeBtn.GetComponent<Button>());

        var scanCtrl = scanTab.GetComponent<ScanARScreenController>();
        SetPrivateField(scanCtrl, "bridge", bridge);
        SetPrivateField(scanCtrl, "backButton", backBtn.GetComponent<Button>());
        SetPrivateField(scanCtrl, "saveRoomButton", saveBtn.GetComponent<Button>());
    }

    static void WireDesignAITab(GameObject tab)
    {
        var layout = tab.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(36, 36, 36, 36);
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        // Header Card
        var header = CreatePanel(tab.transform, "Header", SurfaceDark);
        var headElem = header.AddComponent<LayoutElement>();
        headElem.preferredHeight = 110f;

        var headLayout = header.AddComponent<VerticalLayoutGroup>();
        headLayout.padding = new RectOffset(24, 24, 16, 16);
        headLayout.spacing = 6;
        headLayout.childAlignment = TextAnchor.MiddleLeft;

        CreateText(header.transform, "Title", "AI Room Designer", 32, FontStyles.Bold, TextAlignmentOptions.Left, TextPrimary);
        CreateText(header.transform, "Sub", "Describe your room vision and generate realistic interior concepts", 18, FontStyles.Normal, TextAlignmentOptions.Left, TextMuted);

        // Input Card
        var inputCard = CreatePanel(tab.transform, "InputCard", SurfaceDark);
        var inputCardElem = inputCard.AddComponent<LayoutElement>();
        inputCardElem.preferredHeight = 220f;

        var inputCardLayout = inputCard.AddComponent<VerticalLayoutGroup>();
        inputCardLayout.padding = new RectOffset(20, 20, 20, 20);
        inputCardLayout.spacing = 16;
        inputCardLayout.childControlWidth = true;
        inputCardLayout.childControlHeight = false;

        var prompt = CreateInputField(inputCard.transform, "PromptInput", "Describe your room prompt (e.g. Scandinavian living room with warm lighting)...");
        var promptElem = prompt.AddComponent<LayoutElement>();
        promptElem.preferredHeight = 90f;

        var genBtn = CreateButton(inputCard.transform, "GenerateBtn", "✨ Generate Concept", PrimaryAccent, TextPrimary, 64f);

        // Concept Gallery View Root
        var galleryRoot = CreatePanel(tab.transform, "GalleryRoot", Color.clear);
        var galElem = galleryRoot.AddComponent<LayoutElement>();
        galElem.flexibleHeight = 1f;

        var galLayout = galleryRoot.AddComponent<VerticalLayoutGroup>();
        galLayout.spacing = 16;
        galLayout.childControlWidth = true;
        galLayout.childControlHeight = false;

        var gallery = galleryRoot.AddComponent<ConceptGalleryView>();
        SetPrivateField(gallery, "contentRoot", galleryRoot.transform);

        var ctrl = tab.GetComponent<DesignAIController>();
        SetPrivateField(ctrl, "promptInput", prompt.GetComponent<TMP_InputField>());
        SetPrivateField(ctrl, "generateButton", genBtn.GetComponent<Button>());
        SetPrivateField(ctrl, "gallery", gallery);
    }

    static void WireVastuTab(GameObject tab)
    {
        var layout = tab.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(36, 36, 36, 36);
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        // Header Card
        var header = CreatePanel(tab.transform, "Header", SurfaceDark);
        var headElem = header.AddComponent<LayoutElement>();
        headElem.preferredHeight = 100f;

        var headLayout = header.AddComponent<VerticalLayoutGroup>();
        headLayout.padding = new RectOffset(24, 24, 16, 16);
        headLayout.spacing = 4;
        headLayout.childAlignment = TextAnchor.MiddleLeft;

        CreateText(header.transform, "Title", "Vastu Assistant", 30, FontStyles.Bold, TextAlignmentOptions.Left, TextPrimary);
        CreateText(header.transform, "Sub", "Directional balance and spatial layout guidelines", 16, FontStyles.Normal, TextAlignmentOptions.Left, TextMuted);

        // Direction Selector Panel
        var dirPanel = CreatePanel(tab.transform, "DirectionPanel", SurfaceDark);
        var dirElem = dirPanel.AddComponent<LayoutElement>();
        dirElem.preferredHeight = 84f;

        var dirLayout = dirPanel.AddComponent<HorizontalLayoutGroup>();
        dirLayout.padding = new RectOffset(16, 16, 12, 12);
        dirLayout.spacing = 12;
        dirLayout.childAlignment = TextAnchor.MiddleCenter;
        dirLayout.childControlWidth = true;
        dirLayout.childControlHeight = true;

        dirPanel.AddComponent<RoomDirectionSelector>();
        string[] dirs = { "North 🧭", "East 🌅", "South ☀️", "West 🌇" };
        foreach (var d in dirs)
        {
            CreateButton(dirPanel.transform, d + "Btn", d, SurfaceElevated, TextPrimary, 52f);
        }

        // Chat Root Container
        var chatRoot = CreatePanel(tab.transform, "ChatRoot", Color.clear);
        var chatElem = chatRoot.AddComponent<LayoutElement>();
        chatElem.flexibleHeight = 1f;

        var chatLayout = chatRoot.AddComponent<VerticalLayoutGroup>();
        chatLayout.spacing = 12;
        chatLayout.childControlWidth = true;
        chatLayout.childControlHeight = false;

        var chatView = chatRoot.AddComponent<VastuChatView>();

        // Bottom Input Row
        var inputRow = CreatePanel(tab.transform, "InputRow", SurfaceDark);
        var rowElem = inputRow.AddComponent<LayoutElement>();
        rowElem.preferredHeight = 72f;

        var rowLayout = inputRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.padding = new RectOffset(12, 12, 10, 10);
        rowLayout.spacing = 12;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = true;

        var input = CreateInputField(inputRow.transform, "ChatInput", "Ask Vastu assistant about furniture placement...");
        var inputElem = input.AddComponent<LayoutElement>();
        inputElem.preferredWidth = 700f;
        inputElem.flexibleWidth = 1f;

        var sendBtn = CreateButton(inputRow.transform, "SendBtn", "Send", PrimaryAccent, TextPrimary, 52f, 140f);

        SetPrivateField(chatView, "contentRoot", chatRoot.transform);
        SetPrivateField(chatView, "inputField", input.GetComponent<TMP_InputField>());
        SetPrivateField(chatView, "sendButton", sendBtn.GetComponent<Button>());
    }

    static void WireLibraryTab(GameObject tab)
    {
        var layout = tab.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(36, 36, 36, 36);
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        // Header Card
        var header = CreatePanel(tab.transform, "Header", SurfaceDark);
        var headElem = header.AddComponent<LayoutElement>();
        headElem.preferredHeight = 100f;

        var headLayout = header.AddComponent<VerticalLayoutGroup>();
        headLayout.padding = new RectOffset(24, 24, 16, 16);
        headLayout.spacing = 4;
        headLayout.childAlignment = TextAnchor.MiddleLeft;

        CreateText(header.transform, "Title", "Saved Library", 30, FontStyles.Bold, TextAlignmentOptions.Left, TextPrimary);
        CreateText(header.transform, "Sub", "Access saved 3D rooms & AI generated designs", 16, FontStyles.Normal, TextAlignmentOptions.Left, TextMuted);

        // Filter Bar
        var tabBarGo = CreatePanel(tab.transform, "LibraryTabs", SurfaceDark);
        var barElem = tabBarGo.AddComponent<LayoutElement>();
        barElem.preferredHeight = 72f;

        var barLayout = tabBarGo.AddComponent<HorizontalLayoutGroup>();
        barLayout.padding = new RectOffset(12, 12, 8, 8);
        barLayout.spacing = 12;
        barLayout.childAlignment = TextAnchor.MiddleCenter;
        barLayout.childControlWidth = true;
        barLayout.childControlHeight = true;

        var tabBar = tabBarGo.AddComponent<LibraryTabBar>();
        string[] tabs = { "All", "Saved Rooms", "AI Concepts", "Favorites" };
        foreach (var t in tabs)
        {
            CreateButton(tabBarGo.transform, t + "TabBtn", t, SurfaceElevated, TextPrimary, 52f);
        }

        // List Root Container
        var listRoot = CreatePanel(tab.transform, "ListRoot", Color.clear);
        var listElem = listRoot.AddComponent<LayoutElement>();
        listElem.flexibleHeight = 1f;

        var listLayout = listRoot.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 16;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = false;

        var ctrl = tab.GetComponent<LibraryScreenController>();
        SetPrivateField(ctrl, "tabBar", tabBar);
        SetPrivateField(ctrl, "listRoot", listRoot.transform);
    }

    static GameObject CreateInputField(Transform parent, string name, string placeholder)
    {
        var go = CreatePanel(parent, name, SurfaceElevated);
        var input = go.AddComponent<TMP_InputField>();

        var textGo = CreateText(go.transform, "Text", "", 20, FontStyles.Normal, TextAlignmentOptions.Left, TextPrimary);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16, 12);
        textRect.offsetMax = new Vector2(-16, -12);

        var placeholderGo = CreateText(go.transform, "Placeholder", placeholder, 20, FontStyles.Italic, TextAlignmentOptions.Left, TextMuted);
        var placeholderRect = placeholderGo.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(16, 12);
        placeholderRect.offsetMax = new Vector2(-16, -12);

        input.textComponent = textGo.GetComponent<TextMeshProUGUI>();
        input.placeholder = placeholderGo.GetComponent<TextMeshProUGUI>();
        return go;
    }

    static GameObject CreateTabScreen<T>(Transform parent, string name) where T : ScreenBase
    {
        var go = CreatePanel(parent, name, Color.clear);
        Stretch(go.GetComponent<RectTransform>());
        go.AddComponent<CanvasGroup>();
        var screen = go.AddComponent<T>();
        screen.Hide();
        return go;
    }

    static void RegisterBuildScenes()
    {
        string[] scenes =
        {
            UiScenePath + "Bootstrap.unity",
            UiScenePath + "SplashScreen.unity",
            UiScenePath + "Onboarding.unity",
            UiScenePath + "Permissions.unity",
            UiScenePath + "MainShell.unity",
            "Assets/Scenes/SampleScene.unity"
        };

        var list = new EditorBuildSettingsScene[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            list[i] = new EditorBuildSettingsScene(scenes[i], true);
        }

        EditorBuildSettings.scenes = list;
    }

    static void CreateEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
            return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    static GameObject CreateCanvas(string name, Color bgColor)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(390, 844);
        scaler.matchWidthOrHeight = 0f; // Width match for portrait mobile screens
        go.AddComponent<GraphicRaycaster>();

        // Create Full-Screen Solid Background Panel directly under Canvas to prevent any camera leakage!
        var bg = CreatePanel(go.transform, "CanvasBackground", bgColor);
        Stretch(bg.GetComponent<RectTransform>());

        return go;
    }

    static GameObject CreateUiRoot(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = Vector2.zero;
        return go;
    }

    static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = CreateUiRoot(name, parent);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static TMP_FontAsset cachedFont;

    static TMP_FontAsset GetDefaultFont()
    {
        if (cachedFont != null)
            return cachedFont;

        cachedFont = TMP_Settings.defaultFontAsset;
        if (cachedFont != null)
            return cachedFont;

        cachedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (cachedFont != null)
            return cachedFont;

        string[] guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            cachedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            return cachedFont;
        }

        return null;
    }

    static GameObject CreateText(Transform parent, string name, string text, float size, FontStyles fontStyle = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.Left, Color? color = null, float preferredHeight = 0f)
    {
        var go = CreateUiRoot(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        var font = GetDefaultFont();
        if (font != null)
            tmp.font = font;

        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = fontStyle;
        tmp.alignment = alignment;
        tmp.color = color ?? Color.white;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;

        var elem = go.AddComponent<LayoutElement>();
        elem.preferredHeight = preferredHeight > 0f ? preferredHeight : size * 1.5f;
        elem.flexibleWidth = 1f;

        return go;
    }

    static GameObject CreateButton(Transform parent, string name, string label, Color bgColor, Color textColor, float height = 64f, float width = 0f)
    {
        var go = CreatePanel(parent, name, bgColor);
        go.AddComponent<Button>();

        var rect = go.GetComponent<RectTransform>();
        if (width > 0f)
        {
            rect.sizeDelta = new Vector2(width, height);
        }

        var layoutElem = go.AddComponent<LayoutElement>();
        layoutElem.preferredHeight = height;
        if (width > 0f)
        {
            layoutElem.preferredWidth = width;
            layoutElem.flexibleWidth = 0f;
        }
        else
        {
            layoutElem.flexibleWidth = 1f;
        }

        var textGo = CreateText(go.transform, "Label", label, 20, FontStyles.Bold, TextAlignmentOptions.Center, textColor);
        Stretch(textGo.GetComponent<RectTransform>());
        return go;
    }

    static GameObject AddSafeArea(Transform canvas)
    {
        var safe = CreateUiRoot("SafeArea", canvas);
        Stretch(safe.GetComponent<RectTransform>());
        safe.AddComponent<SafeAreaFitter>();
        return safe;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }

    static void SetPrivateField(Object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }
}
