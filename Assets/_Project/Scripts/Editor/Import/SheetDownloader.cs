using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine.Networking;

namespace JLPTQuest.EditorTools
{
    /*
        Grabs every tab from Google Sheets as CSV and saves it as the raw snapshot.
        The raw files only get written once every tab came back fine, so a half finished sync never sticks.
        Runs off EditorApplication.update so the editor doesn't freeze while it waits.
    */
    public static class SheetDownloader
    {
        public static void DownloadAll(ContentSyncSettings settings, bool showProgress, Action<bool, string> onDone)
        {
            new DownloadJob(settings, showProgress, onDone).Start();
        }

        private class DownloadJob
        {
            private const int timeoutSeconds = 30;

            private readonly ContentSyncSettings settings;
            private readonly bool showProgress;
            private readonly Action<bool, string> onDone;
            private readonly List<string> results = new List<string>();
            private UnityWebRequest request;

            public DownloadJob(ContentSyncSettings settings, bool showProgress, Action<bool, string> onDone)
            {
                this.settings = settings;
                this.showProgress = showProgress;
                this.onDone = onDone;
            }

            public void Start()
            {
                if (settings.tabs.Count == 0)
                {
                    onDone?.Invoke(false, "ContentSyncSettings has no tabs to download.");
                    return;
                }

                StartNext();
                EditorApplication.update += Tick;
            }

            private SheetTab CurrentTab => settings.tabs[results.Count];

            private void StartNext()
            {
                request = UnityWebRequest.Get(settings.ExportUrl(CurrentTab));
                request.timeout = timeoutSeconds;
                request.SendWebRequest();
            }

            private void Tick()
            {
                if (showProgress)
                {
                    float progress = (results.Count + request.downloadProgress) / settings.tabs.Count;
                    EditorUtility.DisplayProgressBar("Syncing content", "Downloading " + CurrentTab.name + "...", progress);
                }
                if (!request.isDone) return;

                string problem = CheckResponse(request, CurrentTab);
                string text = problem == null ? request.downloadHandler.text : null;
                request.Dispose();

                if (problem != null)
                {
                    Finish(false, problem);
                    return;
                }

                results.Add(text);
                if (results.Count < settings.tabs.Count)
                {
                    StartNext();
                    return;
                }

                WriteRawFiles();
                Finish(true, null);
            }

            private void WriteRawFiles()
            {
                for (int i = 0; i < settings.tabs.Count; i++)
                {
                    string path = settings.RawPath(settings.tabs[i]);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    string text = results[i].Replace("\r\n", "\n"); //LF only keeps git diffs clean
                    File.WriteAllText(path, text, new UTF8Encoding(false));
                }
            }

            private void Finish(bool ok, string problem)
            {
                EditorApplication.update -= Tick;
                if (showProgress) EditorUtility.ClearProgressBar();
                onDone?.Invoke(ok, problem);
            }

            private static string CheckResponse(UnityWebRequest request, SheetTab tab)
            {
                const string shareHint = " Check the sheet is shared as 'Anyone with the link: Viewer' and that the tab's gid in ContentSyncSettings is right.";

                if (request.result != UnityWebRequest.Result.Success)
                    return tab.name + ": " + request.error + " (HTTP " + request.responseCode + ")." + shareHint;

                //Google sends a sign in page instead of CSV when the sheet isn't shared
                string text = request.downloadHandler.text ?? string.Empty;
                if (text.TrimStart().StartsWith("<"))
                    return tab.name + " came back as a web page instead of CSV." + shareHint;
                if (text.Trim().Length == 0)
                    return tab.name + " came back empty." + shareHint;

                return null;
            }
        }
    }
}
