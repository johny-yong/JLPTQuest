using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace JLPTQuest.EditorTools
{
    /*
        Lays out the greybox battle screen in Battle.unity and wires every reference.
        Safe to run again, it wipes "Battle UI" and builds it fresh, so change the numbers here instead of by hand.
        Real art comes in later, the view scripts and the wiring stay the same.

        Screen split as a fraction of the safe area height, top to bottom:
        top bar 0.945-1, enemy 0.55-0.945, hints 0.49-0.55, question 0.32-0.49, answers 0.02-0.31
    */
    public static class BattleGreyboxBuilder
    {
        private const string scenePath = "Assets/_Project/Scenes/Battle.unity";
        private const string configPath = "Assets/_Project/Data/Settings/BattleConfig.asset";
        private const string semiBoldPath = "Assets/_Project/Fonts/KleeOne/KleeOne-SemiBold SDF.asset";
        private const string rootName = "Battle UI";
        private const string previewEnemy = "kuchi-he";

        private static readonly Color cardColor = new Color(0.988f, 0.976f, 0.949f); //A touch lighter than the paper
        private static readonly Color shadeColor = new Color(0.102f, 0.090f, 0.078f, 0.45f);

        private static Sprite roundSprite;
        private static TMP_FontAsset semiBold;

        [MenuItem("Tools/JLPT Quest/Dev/Rebuild Battle Greybox", priority = 100)]
        private static void RebuildFromMenu()
        {
            Rebuild(true);
        }

        //interactive false never shows a dialog, so tools and scripts can call it
        public static void Rebuild(bool interactive)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Battle] Stop play mode before rebuilding the greybox.");
                return;
            }

            Scene scene = OpenBattleScene(interactive);
            if (!scene.IsValid()) return;

            SafeAreaFitter safeArea = Object.FindAnyObjectByType<SafeAreaFitter>();
            if (safeArea == null)
            {
                Debug.LogError("[Battle] Battle.unity needs Canvas > SafeArea with a SafeAreaFitter on it.");
                return;
            }

            roundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            semiBold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(semiBoldPath);

            ContentDatabase database = AssetDatabase.LoadAssetAtPath<ContentDatabase>(ContentSyncSettings.LoadOrCreate().DatabasePath);
            if (database == null) Debug.LogWarning("[Battle] No ContentDatabase yet, run Tools > JLPT Quest > Import Local CSVs and rebuild.");
            EnemyData preview = database != null ? database.GetEnemy(previewEnemy) : null;

            RemoveChild(safeArea.transform, "FontCheck"); //The Phase 0 furigana test, the battle screen shows furigana now
            RemoveChild(safeArea.transform, rootName);

            RectTransform root = Rect(rootName, safeArea.transform, 0f, 0f, 1f, 1f);
            var controller = root.gameObject.AddComponent<BattleController>();

            TMP_Text topLabel = BuildTopBar(root);
            EnemyView enemyView = BuildEnemy(root, preview);
            Rect("Hint Row (Phase 3)", root, 0.04f, 0.49f, 0.96f, 0.55f);
            QuestionCardView questionCard = BuildQuestionCard(root);
            AnswerTilesView answerTiles = BuildAnswers(root);
            TeachCardView teachCard = BuildTeachCard(root); //Overlays last so they draw on top
            VictoryView victory = BuildVictory(root, preview);

            Wire(controller, "database", database);
            Wire(controller, "config", LoadOrCreateConfig());
            Wire(controller, "topLabel", topLabel);
            Wire(controller, "enemyView", enemyView);
            Wire(controller, "questionCard", questionCard);
            Wire(controller, "answerTiles", answerTiles);
            Wire(controller, "teachCard", teachCard);
            Wire(controller, "victory", victory);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = root.gameObject;
            Debug.Log("[Battle] Greybox rebuilt in " + scenePath);
        }

        private static Scene OpenBattleScene(bool interactive)
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.path == scenePath) return active;

            if (interactive)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return default;
            }
            else
            {
                EditorSceneManager.SaveOpenScenes(); //No popup, just keep whatever was open
            }
            return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        // ---------- Sections ----------

        private static TMP_Text BuildTopBar(RectTransform root)
        {
            RectTransform bar = Rect("Top Bar", root, 0f, 0.945f, 1f, 1f);
            return Label("Top Label", bar, 0.05f, 0f, 0.95f, 1f, "Lesson 1  ·  Question 1", 36f, InkColors.InkSoft, TextAlignmentOptions.Left);
        }

        private static EnemyView BuildEnemy(RectTransform root, EnemyData preview)
        {
            RectTransform stage = Rect("Enemy Stage", root, 0f, 0.55f, 1f, 0.945f);
            var view = stage.gameObject.AddComponent<EnemyView>();

            //The body is just a very round panel, the face glyphs do the talking
            RectTransform face = Rect("Face", stage, 0.22f, 0.2f, 0.78f, 0.92f);
            Panel(face, InkColors.Panel, 0.08f);
            TMP_Text eyes = Label("Eyes", face, 0f, 0.56f, 1f, 0.86f, preview != null ? preview.faceEyes : "令令", 140f, InkColors.Ink, TextAlignmentOptions.Center);
            eyes.characterSpacing = 30f;
            TMP_Text nose = Label("Nose", face, 0f, 0.38f, 1f, 0.58f, preview != null ? preview.faceNose : "も", 100f, InkColors.Ink, TextAlignmentOptions.Center);
            TMP_Text mouth = Label("Mouth", face, 0f, 0.1f, 1f, 0.38f, preview != null ? preview.faceMouth : "へ", 140f, InkColors.Ink, TextAlignmentOptions.Center);

            string name = preview != null ? preview.displayName + "  " + preview.nameJp : "Kuchi-he  くちへ";
            TMP_Text nameLabel = Label("Name", stage, 0.1f, 0.02f, 0.9f, 0.1f, name, 40f, InkColors.Ink, TextAlignmentOptions.Center);

            RectTransform hp = Rect("HP Bar", stage, 0.2f, 0.115f, 0.8f, 0.15f);
            Panel(hp, InkColors.Tile, 2f);
            RectTransform hpFill = Rect("Fill", hp, 0f, 0f, 1f, 1f);
            Panel(hpFill, InkColors.Vermilion, 2f);
            BarFill hpBar = hp.gameObject.AddComponent<BarFill>();
            Wire(hpBar, "fill", hpFill);
            hpBar.Value = 1f;

            //Turns until the enemy hits back, like the CD number in Tower of Saviors
            RectTransform badge = Rect("Cooldown Badge", stage, 0.76f, 0.8f, 0.96f, 0.94f);
            Panel(badge, cardColor, 0.5f);
            TMP_Text cooldown = Label("Label", badge, 0f, 0f, 1f, 1f, "CD 3", 44f, InkColors.Ink, TextAlignmentOptions.Center, true);

            TMP_Text damage = Label("Damage", stage, 0.1f, 0.62f, 0.9f, 0.82f, "165", 120f, InkColors.Vermilion, TextAlignmentOptions.Center, true);
            damage.alpha = 0f;

            Wire(view, "eyes", eyes);
            Wire(view, "nose", nose);
            Wire(view, "mouth", mouth);
            Wire(view, "nameLabel", nameLabel);
            Wire(view, "cooldownLabel", cooldown);
            Wire(view, "damageLabel", damage);
            Wire(view, "hpBar", hpBar);
            return view;
        }

        private static QuestionCardView BuildQuestionCard(RectTransform root)
        {
            RectTransform card = Rect("Question Card", root, 0.04f, 0.32f, 0.96f, 0.49f);
            Panel(card, cardColor, 0.5f);
            var view = card.gameObject.AddComponent<QuestionCardView>();

            RectTransform timer = Rect("Timer Bar", card, 0.04f, 0.9f, 0.96f, 0.94f);
            Panel(timer, InkColors.Tile, 2f);
            RectTransform timerFillRect = Rect("Fill", timer, 0f, 0f, 1f, 1f);
            Image timerFill = Panel(timerFillRect, InkColors.InkSoft, 2f);
            BarFill timerBar = timer.gameObject.AddComponent<BarFill>();
            Wire(timerBar, "fill", timerFillRect);
            timerBar.Value = 1f;

            RubyText prompt = Ruby("Prompt", card, 0.04f, 0.2f, 0.96f, 0.86f, PromptDisplay.WithBlanks("わたし＿ {学生|がくせい}です。"), 52f, InkColors.Ink);
            TMP_Text instruction = Label("Instruction", card, 0.04f, 0.03f, 0.96f, 0.19f, "also", 30f, InkColors.InkSoft, TextAlignmentOptions.Center);
            instruction.fontStyle = FontStyles.Italic;

            Wire(view, "prompt", prompt);
            Wire(view, "instruction", instruction);
            Wire(view, "timerBar", timerBar);
            Wire(view, "timerFill", timerFill);
            return view;
        }

        private static AnswerTilesView BuildAnswers(RectTransform root)
        {
            RectTransform zone = Rect("Answer Zone", root, 0.04f, 0.02f, 0.96f, 0.31f);
            var view = zone.gameObject.AddComponent<AnswerTilesView>();

            string[] previewOptions = { "は", "を", "に", "で" };
            var tiles = new List<Object>();
            for (int i = 0; i < previewOptions.Length; i++)
            {
                //Reading order: top left, top right, bottom left, bottom right
                bool right = i % 2 == 1;
                bool top = i < 2;
                tiles.Add(BuildTile(zone, i, right ? 0.52f : 0f, top ? 0.52f : 0f, right ? 1f : 0.48f, top ? 1f : 0.48f, previewOptions[i]));
            }

            WireList(view, "tiles", tiles);
            return view;
        }

        private static AnswerTile BuildTile(RectTransform zone, int index, float xMin, float yMin, float xMax, float yMax, string option)
        {
            RectTransform rect = Rect("Tile " + (index + 1), zone, xMin, yMin, xMax, yMax);
            Image background = Panel(rect, InkColors.Tile, 0.5f);
            background.raycastTarget = true;

            Button button = MakeButton(rect, background);
            RubyText label = Ruby("Label", rect, 0.06f, 0.08f, 0.94f, 0.88f, option, 72f, InkColors.Ink);
            label.GetComponent<TMP_Text>().textWrappingMode = TextWrappingModes.NoWrap; //AnswerTile shrinks long answers instead
            TMP_Text mark = Label("Mark", rect, 0.7f, 0.5f, 0.98f, 0.98f, "○", 96f, InkColors.Vermilion, TextAlignmentOptions.Center);
            mark.enabled = false;

            var tile = rect.gameObject.AddComponent<AnswerTile>();
            Wire(tile, "button", button);
            Wire(tile, "background", background);
            Wire(tile, "label", label);
            Wire(tile, "mark", mark);
            return tile;
        }

        private static TeachCardView BuildTeachCard(RectTransform root)
        {
            RectTransform overlay = Rect("Teach Card", root, 0f, 0f, 1f, 1f);
            Image shade = Shade(overlay, shadeColor);
            CanvasGroup group = HiddenGroup(overlay);
            Button tapArea = MakeButton(overlay, shade);
            tapArea.transition = Selectable.Transition.None; //Tapping anywhere closes it, card included

            RectTransform card = Rect("Card", overlay, 0.06f, 0.33f, 0.94f, 0.66f);
            Panel(card, cardColor, 0.5f).raycastTarget = true;

            TMP_Text header = Label("Header", card, 0.06f, 0.8f, 0.94f, 0.95f, "Not quite", 46f, InkColors.Vermilion, TextAlignmentOptions.Center, true);
            RubyText sentence = Ruby("Sentence", card, 0.05f, 0.46f, 0.95f, 0.8f, "わたし<color=#C23B22>は</color> {学生|がくせい}です。", 52f, InkColors.Ink);
            TMP_Text rule = Label("Rule", card, 0.08f, 0.17f, 0.92f, 0.46f, "は marks the topic, what the sentence is about.", 34f, InkColors.Ink, TextAlignmentOptions.Center);
            TMP_Text tapHint = Label("Tap Hint", card, 0.06f, 0.03f, 0.94f, 0.15f, "Tap to continue", 28f, InkColors.InkFaint, TextAlignmentOptions.Center);

            var view = overlay.gameObject.AddComponent<TeachCardView>();
            Wire(view, "group", group);
            Wire(view, "tapArea", tapArea);
            Wire(view, "header", header);
            Wire(view, "sentence", sentence);
            Wire(view, "rule", rule);
            Wire(view, "tapHint", tapHint);
            return view;
        }

        private static VictoryView BuildVictory(RectTransform root, EnemyData preview)
        {
            RectTransform overlay = Rect("Victory", root, 0f, 0f, 1f, 1f);
            Color backdrop = InkColors.Paper;
            backdrop.a = 0.94f;
            Shade(overlay, backdrop);
            CanvasGroup group = HiddenGroup(overlay);

            string name = preview != null ? preview.displayName : "Kuchi-he";
            TMP_Text title = Label("Title", overlay, 0.06f, 0.58f, 0.94f, 0.68f, name + " is beaten!", 64f, InkColors.Ink, TextAlignmentOptions.Center, true);
            RubyText seal = Ruby("Word Seal", overlay, 0.06f, 0.48f, 0.94f, 0.58f, "Word Seal: {先生|せんせい}", 52f, InkColors.Vermilion);

            RectTransform againRect = Rect("Fight Again", overlay, 0.25f, 0.34f, 0.75f, 0.41f);
            Image againImage = Panel(againRect, InkColors.Ink, 0.5f);
            againImage.raycastTarget = true;
            Button again = MakeButton(againRect, againImage);
            Label("Label", againRect, 0f, 0f, 1f, 1f, "Fight again", 44f, InkColors.Paper, TextAlignmentOptions.Center, true);

            var view = overlay.gameObject.AddComponent<VictoryView>();
            Wire(view, "group", group);
            Wire(view, "title", title);
            Wire(view, "wordSeal", seal);
            Wire(view, "againButton", again);
            return view;
        }

        // ---------- Building blocks ----------

        private static RectTransform Rect(string name, Transform parent, float xMin, float yMin, float xMax, float yMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;

            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        //corners below 1 make the rounded corners bigger, above 1 smaller
        private static Image Panel(RectTransform rect, Color color, float corners)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = roundSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = corners;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        //A flat full screen colour that also eats taps while its overlay is showing
        private static Image Shade(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return image;
        }

        private static CanvasGroup HiddenGroup(RectTransform rect)
        {
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            return group;
        }

        private static Button MakeButton(RectTransform rect, Graphic target)
        {
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = target;

            //Disabled stays white so the right and wrong colours on finished tiles don't get greyed out
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f);
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f);
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.fadeDuration = 0.05f;
            button.colors = colors;

            //Touch first, no keyboard focus jumping between tiles
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
            return button;
        }

        private static TextMeshProUGUI Label(string name, Transform parent, float xMin, float yMin, float xMax, float yMax,
            string text, float size, Color color, TextAlignmentOptions alignment, bool bold = false)
        {
            RectTransform rect = Rect(name, parent, xMin, yMin, xMax, yMax);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (bold && semiBold != null) label.font = semiBold;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            return label;
        }

        //RubyText needs a fixed font size and some line spacing so the readings have room
        private static RubyText Ruby(string name, Transform parent, float xMin, float yMin, float xMax, float yMax, string markup, float size, Color color)
        {
            TextMeshProUGUI label = Label(name, parent, xMin, yMin, xMax, yMax, string.Empty, size, color, TextAlignmentOptions.Center);
            label.lineSpacing = 40f;
            var ruby = label.gameObject.AddComponent<RubyText>();
            ruby.Markup = markup;
            return ruby;
        }

        private static void RemoveChild(Transform parent, string name)
        {
            Transform child;
            while ((child = parent.Find(name)) != null)
                Object.DestroyImmediate(child.gameObject);
        }

        private static BattleConfig LoadOrCreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<BattleConfig>(configPath);
            if (config != null) return config;

            config = ScriptableObject.CreateInstance<BattleConfig>();
            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[Battle] Made " + configPath + ", tweak the battle numbers there.");
            return config;
        }

        //Private [SerializeField]s can only be set through SerializedObject. A typo in a field name throws so it can't fail quietly
        private static void Wire(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            FindField(serialized, target, field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireList(Object target, string field, IList<Object> values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty list = FindField(serialized, target, field);
            list.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty FindField(SerializedObject serialized, Object target, string field)
        {
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null) throw new InvalidOperationException(target.GetType().Name + " has no serialized field called '" + field + "'.");
            return property;
        }
    }
}
