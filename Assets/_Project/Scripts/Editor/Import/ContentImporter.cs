using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace JLPTQuest.EditorTools
{
    //Everything read out of the CSVs that passed the checks, before it becomes assets
    public class ContentBundle
    {
        public readonly List<QuestionData> questions = new List<QuestionData>();
        public readonly List<EnemyData> enemies = new List<EnemyData>();
        public readonly List<StageData> stages = new List<StageData>();
    }

    /*
        Raw CSVs in, generated assets out.
        Rows with errors get skipped and reported, every other row still goes in.
        A missing tab or a missing column stops the whole import so a broken sync can never wipe content.
    */
    public static class ContentImporter
    {
        public static ImportReport ImportFromRaw(ContentSyncSettings settings)
        {
            var report = new ImportReport();
            string questions = ReadRaw(settings, ContentKind.Questions, report);
            string enemies = ReadRaw(settings, ContentKind.Enemies, report);
            string stages = ReadRaw(settings, ContentKind.Stages, report);
            if (report.aborted) return report;

            ContentBundle bundle = Build(questions, enemies, stages, report);
            if (!report.aborted) WriteAssets(bundle, settings, report);
            return report;
        }

        //The pure part, no assets touched, so tests can feed it plain strings
        public static ContentBundle Build(string questionsCsv, string enemiesCsv, string stagesCsv, ImportReport report)
        {
            var bundle = new ContentBundle();
            CsvTable questionTable = LoadTable(QuestionRowParser.Tab, questionsCsv, QuestionRowParser.requiredColumns, report);
            CsvTable enemyTable = LoadTable(EnemyRowParser.Tab, enemiesCsv, EnemyRowParser.requiredColumns, report);
            CsvTable stageTable = LoadTable(StageRowParser.Tab, stagesCsv, StageRowParser.requiredColumns, report);
            if (report.aborted) return bundle;

            ReadRows(questionTable, QuestionRowParser.Tab, report, row => QuestionRowParser.Parse(row, report), bundle.questions);
            ReadRows(enemyTable, EnemyRowParser.Tab, report, row => EnemyRowParser.Parse(row, report), bundle.enemies);
            ReadRows(stageTable, StageRowParser.Tab, report, row => StageRowParser.Parse(row, report), bundle.stages);
            CheckStages(bundle, report);

            report.questionsImported = bundle.questions.Count;
            report.enemiesImported = bundle.enemies.Count;
            report.stagesImported = bundle.stages.Count;
            return bundle;
        }

        public static void WriteAssets(ContentBundle bundle, ContentSyncSettings settings, ImportReport report)
        {
            string questionFolder = settings.generatedFolder + "/Questions";
            EditorFolders.Ensure(questionFolder);

            var sets = new List<QuestionSet>();
            var keptPaths = new HashSet<string>();
            foreach (IGrouping<int, QuestionData> lesson in bundle.questions.GroupBy(q => q.lesson).OrderBy(g => g.Key))
            {
                string path = questionFolder + "/Lesson" + lesson.Key.ToString("00") + ".asset";
                QuestionSet set = LoadOrCreate<QuestionSet>(path);
                set.lesson = lesson.Key;
                set.questions = lesson.OrderBy(q => q.id, StringComparer.Ordinal).ToList();
                EditorUtility.SetDirty(set);
                sets.Add(set);
                keptPaths.Add(path);
            }

            //A lesson with no ready questions left loses its asset, it only ever holds importer output
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(QuestionSet), new[] { questionFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!keptPaths.Contains(path)) AssetDatabase.DeleteAsset(path);
            }

            ContentDatabase database = LoadOrCreate<ContentDatabase>(settings.DatabasePath);
            database.lessons = sets;
            database.enemies = bundle.enemies;
            database.stages = bundle.stages;
            database.lastImport = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            database.RebuildLookups();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            report.lessonsImported = sets.Count;
        }

        private static void ReadRows<T>(CsvTable table, string tab, ImportReport report, Func<CsvRow, T> parse, List<T> output) where T : class
        {
            var seenIds = new Dictionary<string, int>();
            foreach (CsvRow row in table.rows)
            {
                if (row.IsEmpty) continue;

                string id = row.Get("id");
                if (id.Length == 0)
                {
                    report.Error(tab, row.sheetRow, "id", "empty, every row needs one");
                    continue;
                }
                if (!RowHelpers.IsValidId(id))
                {
                    report.Error(tab, row.sheetRow, "id", "'" + id + "' can only use letters, numbers, - and _");
                    continue;
                }

                //Drafts and retired rows still own their id, so nobody reuses it by accident
                if (seenIds.TryGetValue(id, out int firstRow))
                {
                    report.Error(tab, row.sheetRow, "id", "'" + id + "' is already used on row " + firstRow);
                    continue;
                }
                seenIds[id] = row.sheetRow;

                RowStatus status = RowHelpers.ReadStatus(row, tab, report);
                if (status == RowStatus.Draft)
                {
                    report.draftsSkipped++;
                    continue;
                }
                if (status == RowStatus.Retired)
                {
                    report.retiredSkipped++;
                    continue;
                }

                T item = parse(row);
                if (item != null) output.Add(item);
            }
        }

        //Stages point at enemies and lessons, so they get checked once everything else is in
        private static void CheckStages(ContentBundle bundle, ImportReport report)
        {
            var enemyIds = new HashSet<string>(bundle.enemies.Select(e => e.id));
            var lessons = new HashSet<int>(bundle.questions.Select(q => q.lesson));
            var kept = new List<StageData>();

            foreach (StageData stage in bundle.stages)
            {
                bool ok = true;
                foreach (string enemy in stage.waves)
                {
                    if (enemyIds.Contains(enemy)) continue;
                    report.Error(StageRowParser.Tab, stage.sheetRow, "waves", "'" + enemy + "' isn't a ready enemy id on the Enemies tab");
                    ok = false;
                }

                foreach (int lesson in stage.lessons)
                    if (!lessons.Contains(lesson))
                        report.Warn(StageRowParser.Tab, stage.sheetRow, "lessons", "lesson " + lesson + " has no ready questions yet");

                if (ok) kept.Add(stage);
            }

            bundle.stages.Clear();
            bundle.stages.AddRange(kept);
        }

        private static CsvTable LoadTable(string tab, string csv, string[] required, ImportReport report)
        {
            if (csv == null)
            {
                report.Abort(tab, "no CSV was given");
                return null;
            }

            var table = new CsvTable(tab, csv);
            if (table.headers.Count == 0)
            {
                report.Abort(tab, "the CSV is empty");
                return table;
            }

            foreach (string column in table.MissingColumns(required))
                report.Abort(tab, "the header row is missing the '" + column + "' column");
            return table;
        }

        private static string ReadRaw(ContentSyncSettings settings, ContentKind kind, ImportReport report)
        {
            SheetTab tab = settings.TabFor(kind);
            if (tab == null)
            {
                report.Abort(kind.ToString(), "no tab is set up for it in ContentSyncSettings");
                return null;
            }

            string path = settings.RawPath(tab);
            if (!File.Exists(path))
            {
                report.Abort(tab.name, "no CSV at " + path + ", run Sync From Google Sheet first");
                return null;
            }
            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
