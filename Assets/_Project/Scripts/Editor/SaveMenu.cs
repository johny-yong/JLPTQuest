using UnityEditor;
using UnityEngine;

namespace JLPTQuest.EditorTools
{
    //Dev helper so you can play as a brand new player again
    public static class SaveMenu
    {
        [MenuItem("Tools/JLPT Quest/Delete Save", priority = 40)]
        private static void DeleteSave()
        {
            if (!EditorUtility.DisplayDialog("Delete save", "Wipe all stars, floors and seals? This can't be undone.", "Delete", "Cancel")) return;
            PlayerPrefs.DeleteKey(SaveSystem.Key);
            PlayerPrefs.Save();
            Debug.Log("[Save] Deleted. The next play starts as a new player.");
        }
    }
}
