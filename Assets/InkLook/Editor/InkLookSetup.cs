using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace InkLook.EditorTools
{
    /// <summary>
    /// One-click setup for the ink look. Everything is under Tools > Ink Look:
    ///   Add Ink Pass to Renderer    adds the full-screen ink pass to a URP renderer
    ///   Remove Ink Pass             takes it out again
    ///   Add Ink World to Scene      the object that feeds the shaders, plus the brush-stroke spawner
    ///   Create Demo Scene           a small scene to see everything working
    ///   Edit Transition Settings    timings and sounds of the painted scene change
    /// and Assets > Create > Ink Look > Ink Lit Material.
    /// </summary>
    public static class InkLookSetup
    {
        const string Menu = "Tools/Ink Look/";
        const string UserFolder = "Assets/InkLookSettings";

        /// <summary>The package folder, wherever it was installed (Assets/ or Packages/).</summary>
        public static string Root
        {
            get
            {
                string asmdef = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName("InkLook.Runtime");
                if (string.IsNullOrEmpty(asmdef)) throw new System.Exception("InkLook.Runtime assembly not found.");
                return Path.GetDirectoryName(Path.GetDirectoryName(asmdef)).Replace('\\', '/');
            }
        }

        static T Load<T>(string relative) where T : Object
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(Root + "/" + relative);
            if (a == null) throw new System.Exception("Ink Look: missing " + Root + "/" + relative);
            return a;
        }

        // ------------------------------------------------------------------ renderer

        [MenuItem(Menu + "Add Ink Pass to Renderer", false, 0)]
        static void AddPassMenu()
        {
            var data = PickRenderer();
            if (data == null) return;
            if (FindPass(data) != null)
            {
                EditorUtility.DisplayDialog("Ink Look", data.name + " already has the ink pass.", "OK");
                Selection.activeObject = data;
                return;
            }
            if (!EditorUtility.DisplayDialog("Ink Look", "Add the ink pass to the renderer '" + data.name + "'?\n\n" + AssetDatabase.GetAssetPath(data)
                + "\n\nEvery camera that uses this renderer will get outlines, paper grain and the screen effects.", "Add", "Cancel")) return;
            AddPass(data, UserPostMaterial());
            Selection.activeObject = data;
            Debug.Log("Ink Look: ink pass added to " + AssetDatabase.GetAssetPath(data));
        }

        [MenuItem(Menu + "Remove Ink Pass from Renderer", false, 1)]
        static void RemovePassMenu()
        {
            var data = PickRenderer();
            if (data == null) return;
            var f = FindPass(data);
            if (f == null) { EditorUtility.DisplayDialog("Ink Look", data.name + " has no ink pass.", "OK"); return; }
            RemovePass(data, f);
            Debug.Log("Ink Look: ink pass removed from " + AssetDatabase.GetAssetPath(data));
        }

        /// <summary>The selected renderer asset, or the default renderer of the active URP asset.</summary>
        static UniversalRendererData PickRenderer()
        {
            var selected = Selection.activeObject as UniversalRendererData;
            if (selected != null) return selected;
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null)
            {
                EditorUtility.DisplayDialog("Ink Look", "The project is not using URP. The ink look needs the Universal Render Pipeline.", "OK");
                return null;
            }
            var so = new SerializedObject(urp);
            var list = so.FindProperty("m_RendererDataList");
            int idx = so.FindProperty("m_DefaultRendererIndex").intValue;
            var data = list.arraySize > idx ? list.GetArrayElementAtIndex(idx).objectReferenceValue as UniversalRendererData : null;
            if (data == null) EditorUtility.DisplayDialog("Ink Look", "Select a Universal Renderer asset in the Project window and try again.", "OK");
            return data;
        }

        public static FullScreenPassRendererFeature FindPass(UniversalRendererData data)
        {
            foreach (var f in data.rendererFeatures)
            {
                var fs = f as FullScreenPassRendererFeature;
                if (fs != null && fs.passMaterial != null && fs.passMaterial.shader != null && fs.passMaterial.shader.name == "Ink/InkPost") return fs;
            }
            return null;
        }

        /// <summary>Adds the full-screen ink pass to a renderer. Safe to call from build scripts.</summary>
        public static FullScreenPassRendererFeature AddPass(UniversalRendererData data, Material postMaterial)
        {
            var existing = FindPass(data);
            if (existing != null) { existing.passMaterial = postMaterial; EditorUtility.SetDirty(data); return existing; }

            var feat = ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();
            feat.name = "InkPost";
            feat.passMaterial = postMaterial;
            feat.injectionPoint = FullScreenPassRendererFeature.InjectionPoint.BeforeRenderingPostProcessing;
            feat.requirements = ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal;
            feat.fetchColorBuffer = true;
            feat.bindDepthStencilAttachment = false;
            Undo.RegisterCreatedObjectUndo(feat, "Add Ink Pass");
            AssetDatabase.AddObjectToAsset(feat, data);
            data.rendererFeatures.Add(feat);
            SyncFeatureMap(data);
            return feat;
        }

        static void RemovePass(UniversalRendererData data, ScriptableRendererFeature f)
        {
            data.rendererFeatures.Remove(f);
            Object.DestroyImmediate(f, true);
            SyncFeatureMap(data);
        }

        /// <summary>URP keeps a list of local file IDs next to the features; it must match or the renderer complains.</summary>
        static void SyncFeatureMap(UniversalRendererData data)
        {
            var so = new SerializedObject(data);
            so.Update();
            var map = so.FindProperty("m_RendererFeatureMap");
            map.arraySize = data.rendererFeatures.Count;
            for (int i = 0; i < data.rendererFeatures.Count; i++)
            {
                string guid;
                long id;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(data.rendererFeatures[i], out guid, out id);
                map.GetArrayElementAtIndex(i).longValue = id;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            data.SetDirty();
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// The post material you tune lives in your project (Assets/InkLookSettings), so updating the package
        /// never overwrites your settings. Created from the package default the first time.
        /// </summary>
        public static Material UserPostMaterial()
        {
            string path = UserFolder + "/M_InkPost.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            if (!AssetDatabase.IsValidFolder(UserFolder)) AssetDatabase.CreateFolder("Assets", "InkLookSettings");
            AssetDatabase.CopyAsset(Root + "/Materials/M_InkPost_Default.mat", path);
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        // ------------------------------------------------------------------ scene

        [MenuItem(Menu + "Add Ink World to Scene", false, 20)]
        static void AddWorldMenu()
        {
            var go = AddWorld();
            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        /// <summary>Adds (or returns) the InkWorld object with a BrushFx on it, wired to the package assets.</summary>
        public static GameObject AddWorld()
        {
            var world = Object.FindFirstObjectByType<InkWorld>();
            GameObject go;
            if (world == null)
            {
                go = new GameObject("InkWorld");
                Undo.RegisterCreatedObjectUndo(go, "Add Ink World");
                world = go.AddComponent<InkWorld>();
            }
            else go = world.gameObject;
            world.grain = Load<Texture2D>("Textures/ink_grain.png");

            var fx = go.GetComponent<BrushFx>();
            if (fx == null) fx = Undo.AddComponent<BrushFx>(go);
            fx.brushWet = Load<Material>("Materials/M_BrushWet.mat");
            fx.brushDry = Load<Material>("Materials/M_BrushDry.mat");
            fx.splat = Load<Material>("Materials/M_BrushSplat.mat");
            fx.grain = world.grain;

            Camera cam = Camera.main;
            if (cam != null)
            {
                Undo.RecordObject(cam, "Paper background");
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = world.paper;
            }
            EditorUtility.SetDirty(world);
            EditorUtility.SetDirty(fx);
            return go;
        }

        [MenuItem(Menu + "Edit Transition Settings", false, 40)]
        static void EditTransition()
        {
            Selection.activeObject = Load<InkTransitionSettings>("Resources/InkLook/InkTransitionSettings.asset");
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        [MenuItem("Assets/Create/Ink Look/Ink Lit Material", false, 300)]
        static void CreateLitMaterial()
        {
            string folder = "Assets";
            foreach (var o in Selection.GetFiltered<Object>(SelectionMode.Assets))
            {
                string p = AssetDatabase.GetAssetPath(o);
                if (AssetDatabase.IsValidFolder(p)) { folder = p; break; }
                if (!string.IsNullOrEmpty(p)) { folder = Path.GetDirectoryName(p).Replace('\\', '/'); break; }
            }
            var m = new Material(Shader.Find("Ink/InkLit"));
            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/M_InkLit.mat");
            AssetDatabase.CreateAsset(m, path);
            Selection.activeObject = m;
        }

        // ------------------------------------------------------------------ demo

        [MenuItem(Menu + "Create Demo Scene", false, 60)]
        static void CreateDemo()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, 168f, 0f);
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.fieldOfView = 30f;
            camGo.AddComponent<UniversalAdditionalCameraData>();
            camGo.transform.position = new Vector3(0f, 2.4f, -16f);
            camGo.transform.rotation = Quaternion.Euler(4f, 0f, 0f);

            var world = AddWorld();
            world.AddComponent<InkLookDemo>();

            Material ground = Lit("M_Demo_Ground", 0.82f, 0.44f, 0.09f, 0.94f);
            Material stone = Lit("M_Demo_Stone", 0.78f, 0.5f, 0.2f, 0f);
            Material accent = Lit("M_Demo_Accent", 0f, 0f, 0f, 0f);
            accent.SetColor("_ToneLight", new Color(0.86f, 0.31f, 0.2f));
            accent.SetColor("_ToneMid", new Color(0.76f, 0.23f, 0.13f));
            accent.SetColor("_ToneShade", new Color(0.52f, 0.14f, 0.09f));

            Shape(PrimitiveType.Cube, new Vector3(0f, -12f, 2f), new Vector3(40f, 24f, 9f), ground);
            Shape(PrimitiveType.Cube, new Vector3(-4f, 1.2f, 1.5f), new Vector3(2.2f, 2.4f, 2f), stone, 12f);
            Shape(PrimitiveType.Sphere, new Vector3(0f, 1f, 0f), Vector3.one * 2f, accent);
            Shape(PrimitiveType.Cylinder, new Vector3(4.2f, 2f, 2f), new Vector3(0.6f, 2f, 0.6f), stone);
            for (int i = 0; i < 6; i++)
                Shape(PrimitiveType.Cube, new Vector3(-14f + i * 6f, 3f, 14f + i * 3f), new Vector3(3f, 6f + i, 3f), stone, i * 17f);

            string scenePath = UserFolder + "/InkLookDemo.unity";
            if (!AssetDatabase.IsValidFolder(UserFolder)) AssetDatabase.CreateFolder("Assets", "InkLookSettings");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);

            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            bool hasPass = false;
            if (urp != null)
            {
                var so = new SerializedObject(urp);
                var list = so.FindProperty("m_RendererDataList");
                int idx = so.FindProperty("m_DefaultRendererIndex").intValue;
                var data = list.arraySize > idx ? list.GetArrayElementAtIndex(idx).objectReferenceValue as UniversalRendererData : null;
                hasPass = data != null && FindPass(data) != null;
            }
            if (!hasPass)
                Debug.LogWarning("Ink Look: demo scene saved to " + scenePath + ". The default renderer has no ink pass yet, so you will see the shading but no outlines or paper. Use Tools > Ink Look > Add Ink Pass to Renderer.");
            else Debug.Log("Ink Look: demo scene saved to " + scenePath + ". Press Play and use the buttons.");
        }

        static Material Lit(string name, float light, float mid, float shade, float mist)
        {
            string path = UserFolder + "/" + name + ".mat";
            if (!AssetDatabase.IsValidFolder(UserFolder)) AssetDatabase.CreateFolder("Assets", "InkLookSettings");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Ink/InkLit")); AssetDatabase.CreateAsset(m, path); }
            System.Func<float, Color> g = v => new Color(v, v * 0.972f, v * 0.925f);
            m.SetColor("_ToneLight", g(light)); m.SetColor("_ToneMid", g(mid)); m.SetColor("_ToneShade", g(shade));
            m.SetFloat("_Mist", mist); m.SetFloat("_MistTop", -0.3f); m.SetFloat("_MistRange", 3.6f);
            EditorUtility.SetDirty(m);
            return m;
        }

        static void Shape(PrimitiveType t, Vector3 pos, Vector3 scale, Material m, float yaw = 0f)
        {
            var go = GameObject.CreatePrimitive(t);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.GetComponent<Renderer>().sharedMaterial = m;
        }
    }
}
