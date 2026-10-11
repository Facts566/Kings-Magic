using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ===========================================================================
//  ВРЕМЕННЫЕ ТЕСТОВЫЕ ЧИТЫ. Нужны только чтобы быстро прыгать по уровням
//  и проверять новых врагов. Убрать из игры: удалить
//  Assets/Scripts/DebugCheats.cs (+ .meta) и блоки Cheat* в
//  GameManager.cs / EnemyAttack.cs / PlayerAttack.cs
//
//  Открытие: 5 быстрых тапов подряд (2.5 сек) по любой надписи на экране.
//  Панель рисуется кодом, в сцене ничего менять не нужно.
// ===========================================================================
public class DebugCheats : MonoBehaviour
{
    // главный выключатель: false = панель не создаётся вообще
    private static readonly bool CheatsOn = true;

    private const int TapsToOpen = 5;   // столько тапов по надписи открывают панель
    private const float TapWindow = 2.5f; // окно между тапами, сек

    private static Font s_font;
    private static Sprite s_white;

    private GameObject canvasGo;
    private Text statusText;
    private Text confirmText;
    private InputField levelInput;
    private Coroutine confirmRoutine;

    private int taps;
    private float lastTapTime;
    private bool resetArmed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawn()
    {
        if (!CheatsOn) return;
        if (FindObjectOfType<DebugCheats>() != null) return;
        new GameObject("DebugCheats").AddComponent<DebugCheats>();
    }

    void Start()
    {
        StartCoroutine(SetupNextFrame());
    }

    private IEnumerator SetupNextFrame()
    {
        // ждём, пока Awake/Start остальных скриптов отработают и ссылки встанут
        yield return null;
        yield return null;
        BuildPanel();
        HookSecretTaps();
    }

    // ------------------------------------------------------------------
    // Скрытый триггер: серия тапов по надписи уровня/монет
    // ------------------------------------------------------------------
    private void HookSecretTaps()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            HookTap(gm.levelText != null ? gm.levelText.gameObject : null);
            HookTap(gm.coinsText != null ? gm.coinsText.gameObject : null);
            HookTap(gm.honorText != null ? gm.honorText.gameObject : null);
        }

        // запасной путь: подцепиться к любой надписи на экране,
        // чтобы панель открылась независимо от того, что перекрывает HUD
        Text[] texts = FindObjectsOfType<Text>();
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
                HookTap(texts[i].gameObject);
        }
    }

    private void HookTap(GameObject go)
    {
        if (go == null) return;

        EventTrigger trigger = go.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = go.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => OnSecretTap());
        trigger.triggers.Add(entry);
    }

    private void OnSecretTap()
    {
        if (canvasGo != null && canvasGo.activeSelf)
            return;

        if (Time.unscaledTime - lastTapTime > TapWindow)
            taps = 0;

        lastTapTime = Time.unscaledTime;
        taps++;

        if (taps >= TapsToOpen)
        {
            taps = 0;
            OpenPanel();
        }
    }

    private void OpenPanel()
    {
        if (canvasGo == null)
            return;

        canvasGo.SetActive(true);
        GameManager gm = GameManager.Instance;
        if (levelInput != null && gm != null)
            levelInput.text = gm.level.ToString();
        RefreshStatus();
    }

    private void ClosePanel()
    {
        if (canvasGo != null)
            canvasGo.SetActive(false);
    }

    // ------------------------------------------------------------------
    // Построение панели
    // ------------------------------------------------------------------
    private void BuildPanel()
    {
        canvasGo = new GameObject("CheatCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720f, 1604f);
        scaler.matchWidthOrHeight = 0f;

        GameObject panel = NewUI("Panel", canvasGo.transform);
        SetRect(panel, new Vector2(700f, 1560f), Vector2.zero);
        Image panelImg = panel.AddComponent<Image>();
        panelImg.sprite = WhiteSprite();
        panelImg.color = new Color(0.04f, 0.05f, 0.08f, 0.96f);

        MakeText("Title", panel.transform, "ЧИТЫ (временные)", 46, new Color(1f, 0.35f, 0.35f),
            new Vector2(660f, 70f), new Vector2(0f, 700f));

        statusText = MakeText("Status", panel.transform, "", 30, new Color(0.6f, 0.95f, 1f),
            new Vector2(660f, 90f), new Vector2(0f, 615f));

        MakeText("LevelLabel", panel.transform, "Новый уровень:", 28, Color.white,
            new Vector2(660f, 50f), new Vector2(0f, 528f));

        levelInput = MakeInput(panel.transform, new Vector2(660f, 96f), new Vector2(0f, 452f));

        // -10 | -1 | +1 | +10
        float bw = 158f, bh = 96f, gap = 6f;
        float x0 = -(bw * 1.5f) - gap * 1.5f;
        MakeButton(panel.transform, "-10", new Vector2(bw, bh), new Vector2(x0, 322f), new Color(0.45f, 0.2f, 0.2f), () => NudgeLevel(-10));
        MakeButton(panel.transform, "-1", new Vector2(bw, bh), new Vector2(x0 + (bw + gap), 322f), new Color(0.45f, 0.2f, 0.2f), () => NudgeLevel(-1));
        MakeButton(panel.transform, "+1", new Vector2(bw, bh), new Vector2(x0 + (bw + gap) * 2f, 322f), new Color(0.2f, 0.45f, 0.25f), () => NudgeLevel(1));
        MakeButton(panel.transform, "+10", new Vector2(bw, bh), new Vector2(x0 + (bw + gap) * 3f, 322f), new Color(0.2f, 0.45f, 0.25f), () => NudgeLevel(10));

        MakeButton(panel.transform, "Применить уровень", new Vector2(660f, 96f), new Vector2(0f, 190f), new Color(0.2f, 0.4f, 0.75f), ApplyTypedLevel);

        MakeButton(panel.transform, "Монеты +10000", new Vector2(660f, 96f), new Vector2(0f, 66f), new Color(0.6f, 0.5f, 0.15f), () => AddCoins(10000));
        MakeButton(panel.transform, "Опыт +50000", new Vector2(660f, 96f), new Vector2(0f, -58f), new Color(0.15f, 0.5f, 0.35f), () => AddXp(50000));
        MakeButton(panel.transform, "Честь +1000", new Vector2(660f, 96f), new Vector2(0f, -182f), new Color(0.45f, 0.3f, 0.55f), () => AddHonor(1000));

        MakeButton(panel.transform, "Восстановить HP/ST", new Vector2(660f, 96f), new Vector2(0f, -306f), new Color(0.15f, 0.4f, 0.45f), FullRestore);
        MakeButton(panel.transform, "Убить врага (дроп)", new Vector2(660f, 96f), new Vector2(0f, -430f), new Color(0.6f, 0.2f, 0.35f), KillEnemy);

        confirmText = MakeText("Confirm", panel.transform, "", 26, new Color(1f, 0.8f, 0.3f),
            new Vector2(660f, 40f), new Vector2(0f, -520f));

        MakeButton(panel.transform, "Сбросить прогресс (2x)", new Vector2(660f, 96f), new Vector2(0f, -584f), new Color(0.55f, 0.15f, 0.15f), ResetProgress);
        MakeButton(panel.transform, "Закрыть", new Vector2(660f, 96f), new Vector2(0f, -700f), new Color(0.25f, 0.25f, 0.3f), ClosePanel);

        // панель по умолчанию скрыта — открывается только секретной серией тапов
        canvasGo.SetActive(false);
    }

    // ------------------------------------------------------------------
    // Действия
    // ------------------------------------------------------------------
    private GameManager Gm()
    {
        return GameManager.Instance;
    }

    private void NudgeLevel(int delta)
    {
        GameManager gm = Gm();
        if (gm == null) return;
        gm.CheatSetLevel(gm.level + delta);
        if (levelInput != null) levelInput.text = gm.level.ToString();
        RefreshStatus();
    }

    private void ApplyTypedLevel()
    {
        GameManager gm = Gm();
        if (gm == null) return;

        int typed;
        if (!int.TryParse(levelInput.text, out typed))
        {
            if (levelInput != null) levelInput.text = gm.level.ToString();
            return;
        }

        gm.CheatSetLevel(typed);
        if (levelInput != null) levelInput.text = gm.level.ToString();
        RefreshStatus();
    }

    private void AddCoins(int amount)
    {
        GameManager gm = Gm();
        if (gm == null) return;
        gm.CheatAddCoins(amount);
        RefreshStatus();
    }

    private void AddXp(int amount)
    {
        GameManager gm = Gm();
        if (gm == null) return;
        gm.CheatAddXp(amount);
        if (levelInput != null) levelInput.text = gm.level.ToString();
        RefreshStatus();
    }

    private void AddHonor(int amount)
    {
        GameManager gm = Gm();
        if (gm == null) return;
        gm.CheatAddHonor(amount);
        RefreshStatus();
    }

    private void FullRestore()
    {
        EnemyAttack ea = FindObjectOfType<EnemyAttack>();
        if (ea != null)
            ea.CheatFullRestore();
        RefreshStatus();
    }

    private void KillEnemy()
    {
        PlayerAttack pa = FindObjectOfType<PlayerAttack>();
        if (pa != null)
            pa.CheatKillEnemy();
        RefreshStatus();
    }

    private void ResetProgress()
    {
        // первое нажатие — только предупреждение, второе в течение 3 сек — сам сброс
        if (!resetArmed)
        {
            resetArmed = true;
            if (confirmRoutine != null)
                StopCoroutine(confirmRoutine);
            confirmRoutine = StartCoroutine(ResetConfirm());
            return;
        }

        resetArmed = false;
        if (confirmRoutine != null)
            StopCoroutine(confirmRoutine);
        confirmRoutine = null;
        if (confirmText != null) confirmText.text = "";

        GameManager gm = Gm();
        if (gm != null)
            gm.CheatResetAll();
        if (levelInput != null && gm != null)
            levelInput.text = gm.level.ToString();
        RefreshStatus();
    }

    private IEnumerator ResetConfirm()
    {
        if (confirmText != null)
            confirmText.text = "Нажми ещё раз для сброса!";
        yield return new WaitForSecondsRealtime(3f);
        if (confirmText != null)
            confirmText.text = "";
        resetArmed = false;
        confirmRoutine = null;
    }

    private void RefreshStatus()
    {
        GameManager gm = Gm();
        if (statusText == null || gm == null)
            return;

        statusText.text = "Ур. " + gm.level + "   $ " + gm.coins + "   XP " + gm.xp + "   честь " + gm.honor;
    }

    // ------------------------------------------------------------------
    // Хелперы постройки UI
    // ------------------------------------------------------------------
    private GameObject NewUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private void SetRect(GameObject go, Vector2 size, Vector2 pos)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
    }

    private Text MakeText(string name, Transform parent, string text, int size, Color color, Vector2 rectSize, Vector2 pos)
    {
        GameObject go = NewUI(name, parent);
        SetRect(go, rectSize, pos);

        Text t = go.AddComponent<Text>();
        t.font = UiFont();
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.text = text;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private Button MakeButton(Transform parent, string label, Vector2 size, Vector2 pos, Color color, UnityEngine.Events.UnityAction action)
    {
        GameObject go = NewUI("Btn_" + label, parent);
        SetRect(go, size, pos);

        Image img = go.AddComponent<Image>();
        img.sprite = WhiteSprite();
        img.color = color;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = img;

        ColorBlock cb = button.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        cb.selectedColor = Color.white;
        cb.fadeDuration = 0.05f;
        button.colors = cb;

        MakeText("Label", go.transform, label, 30, Color.white, size, Vector2.zero);

        if (action != null)
            button.onClick.AddListener(action);

        return button;
    }

    private InputField MakeInput(Transform parent, Vector2 size, Vector2 pos)
    {
        GameObject go = NewUI("LevelInput", parent);
        SetRect(go, size, pos);

        Image img = go.AddComponent<Image>();
        img.sprite = WhiteSprite();
        img.color = new Color(0.15f, 0.17f, 0.22f, 1f);

        InputField field = go.AddComponent<InputField>();
        field.targetGraphic = img;
        field.contentType = InputField.ContentType.IntegerNumber;
        field.characterLimit = 5;
        field.lineType = InputField.LineType.SingleLine;

        Text text = MakeText("Text", go.transform, "", 32, Color.white, size, Vector2.zero);
        text.supportRichText = false;

        Text placeholder = MakeText("Placeholder", go.transform, "например 45", 28, new Color(1f, 1f, 1f, 0.45f), size, Vector2.zero);

        field.textComponent = text;
        field.placeholder = placeholder;
        field.caretColor = Color.white;
        field.customCaretColor = true;

        return field;
    }

    private static Sprite WhiteSprite()
    {
        if (s_white == null)
        {
            Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            s_white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
            s_white.name = "DebugCheatsWhite";
        }
        return s_white;
    }

    private static Font UiFont()
    {
        if (s_font != null)
            return s_font;

        // берём шрифт из любой существующей надписи сцены — так надёжнее встроенного
        Text[] texts = Resources.FindObjectsOfTypeAll<Text>();
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].font != null)
            {
                s_font = texts[i].font;
                return s_font;
            }
        }

        s_font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (s_font == null)
            s_font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return s_font;
    }
}