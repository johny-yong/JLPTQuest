using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JLPTQuest.EditorTools
{
    public enum ContentKind { Questions, Enemies, Stages, Grammar, Words }

    [Serializable]
    public class SheetTab
    {
        public string name;
        public long gid; //The number after gid= in the tab's URL
        public ContentKind kind;
    }

    //Where the content comes from and where it lands. Editors adding rows never need to open this
    public class ContentSyncSettings : ScriptableObject
    {
        public const string AssetPath = "Assets/_Project/Data/Settings/ContentSyncSettings.asset";

        public string spreadsheetId = "1W2bQW6EK8joA7Q48KC6Y6M93pXtFetcmGw5CiW-WbyA";
        public List<SheetTab> tabs = new List<SheetTab>
        {
            new SheetTab { name = "Questions", gid = 100, kind = ContentKind.Questions },
            new SheetTab { name = "Enemies", gid = 200, kind = ContentKind.Enemies },
            new SheetTab { name = "Stages", gid = 300, kind = ContentKind.Stages },
            new SheetTab { name = "Grammar", gid = 400, kind = ContentKind.Grammar },
            new SheetTab { name = "Words", gid = 500, kind = ContentKind.Words }
        };
        public string rawFolder = "Assets/_Project/Data/Raw";
        public string generatedFolder = "Assets/_Project/Data/Generated";

        public string SheetUrl => "https://docs.google.com/spreadsheets/d/" + spreadsheetId + "/edit";
        public string DatabasePath => generatedFolder + "/ContentDatabase.asset";

        public string ExportUrl(SheetTab tab)
        {
            return "https://docs.google.com/spreadsheets/d/" + spreadsheetId + "/export?format=csv&gid=" + tab.gid;
        }

        public string RawPath(SheetTab tab)
        {
            return rawFolder + "/" + tab.name + ".csv";
        }

        //A settings asset made before a tab existed won't list it, so new tabs get added here once
        private void AddMissingTabs()
        {
            var defaults = new List<SheetTab>
            {
                new SheetTab { name = "Grammar", gid = 400, kind = ContentKind.Grammar },
                new SheetTab { name = "Words", gid = 500, kind = ContentKind.Words }
            };
            bool added = false;
            foreach (SheetTab tab in defaults)
            {
                if (TabFor(tab.kind) != null) continue;
                tabs.Add(tab);
                added = true;
            }
            if (!added) return;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public SheetTab TabFor(ContentKind kind)
        {
            return tabs.Find(tab => tab.kind == kind);
        }

        public static ContentSyncSettings LoadOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ContentSyncSettings>(AssetPath);
            if (settings != null)
            {
                settings.AddMissingTabs();
                return settings;
            }

            settings = CreateInstance<ContentSyncSettings>();
            EditorFolders.Ensure(System.IO.Path.GetDirectoryName(AssetPath));
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }

    public static class EditorFolders
    {
        //Makes every folder along the path through the AssetDatabase so the .meta files come along too
        public static void Ensure(string folder)
        {
            folder = folder.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(folder)) return;

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
