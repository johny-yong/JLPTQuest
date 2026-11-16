using UnityEditor;
using UnityEngine;

namespace JLPTQuest.EditorTools
{
    //The two ways content gets in: fresh from the sheet, or from the raw CSVs already in the project
    public static class ContentSync
    {
        public static ImportReport LastReport { get; private set; }

        public static void SyncFromSheet(bool interactive)
        {
            ContentSyncSettings settings = ContentSyncSettings.LoadOrCreate();
            SheetDownloader.DownloadAll(settings, interactive, (ok, problem) =>
            {
                if (!ok)
                {
                    Debug.LogError("[Content] Sync failed: " + problem);
                    if (interactive) EditorUtility.DisplayDialog("Sync failed", problem + "\n\nNothing in the game changed.", "OK");
                    return;
                }

                AssetDatabase.Refresh();
                ImportLocal(interactive);
            });
        }

        public static ImportReport ImportLocal(bool interactive)
        {
            ContentSyncSettings settings = ContentSyncSettings.LoadOrCreate();
            ImportReport report = ContentImporter.ImportFromRaw(settings);
            report.LogToConsole();
            LastReport = report;

            if (interactive)
            {
                string title = report.ErrorCount == 0 ? "Content imported" : "Content imported with problems";
                string details = report.issues.Count > 0 ? "\n\nThe Console lists each one with its tab and row number." : string.Empty;
                EditorUtility.DisplayDialog(title, report.Summary() + details, "OK");
            }
            return report;
        }
    }
}
