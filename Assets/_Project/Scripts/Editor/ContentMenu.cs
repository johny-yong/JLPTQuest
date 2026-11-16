using UnityEditor;
using UnityEngine;

namespace JLPTQuest.EditorTools
{
    //Everything content related lives under Tools > JLPT Quest
    public static class ContentMenu
    {
        private const string root = "Tools/JLPT Quest/";

        [MenuItem(root + "Sync From Google Sheet", priority = 0)]
        private static void SyncFromSheet()
        {
            ContentSync.SyncFromSheet(true);
        }

        [MenuItem(root + "Import Local CSVs", priority = 1)]
        private static void ImportLocal()
        {
            ContentSync.ImportLocal(true);
        }

        [MenuItem(root + "Open Google Sheet", priority = 20)]
        private static void OpenSheet()
        {
            Application.OpenURL(ContentSyncSettings.LoadOrCreate().SheetUrl);
        }

        [MenuItem(root + "Select Content Database", priority = 21)]
        private static void SelectDatabase()
        {
            var database = AssetDatabase.LoadAssetAtPath<ContentDatabase>(ContentSyncSettings.LoadOrCreate().DatabasePath);
            if (database == null)
            {
                Debug.LogWarning("[Content] No database yet, run Sync From Google Sheet first.");
                return;
            }
            Selection.activeObject = database;
            EditorGUIUtility.PingObject(database);
        }

        [MenuItem(root + "Select Sync Settings", priority = 22)]
        private static void SelectSettings()
        {
            Selection.activeObject = ContentSyncSettings.LoadOrCreate();
        }
    }
}
