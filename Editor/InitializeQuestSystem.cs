using System.IO;
using jeanf.EventSystem;
using UnityEditor;
using UnityEngine;

namespace jeanf.questsystem
{
    public class InitializeQuestSystem : MonoBehaviour
    {
        #if UNITY_EDITOR  
        [MenuItem("GameObject/Initialize Quest System")]
        private static void initializeQuestSystem()
        {
            var folder = "Quests";
            var questFolderPath = $"Assets/Resources/{folder}";
            
            var channelFolder = "Channels";
            var channelFolderPath = $"{questFolderPath}/{channelFolder}";
            
            // files needed
            var questStatusUpdate = "QuestStatusUpdate.asset";
            var questRequirementCheck = "QuestRequirementCheck.asset";
            var questProgressChannel = $"QuestsProgressChannel.asset";
            var startQuestEventChannel = $"StartQuestEventChannel.asset";

            if (!AssetDatabase.IsValidFolder(questFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", folder);
            }
            if (!AssetDatabase.IsValidFolder(channelFolderPath))
            {
                AssetDatabase.CreateFolder(questFolderPath, channelFolder);
            }

            CreateStringEventFile(channelFolderPath, questStatusUpdate);
            CreateStringEventFile(channelFolderPath, questRequirementCheck);
            CreateStringEventFile(channelFolderPath, startQuestEventChannel);
            CreateStringFloatEventFile(channelFolderPath, questProgressChannel);
        }
        
        public static void CreateStringEventFile(string path, string fileName)
        {
            if (File.Exists($"{path}/{fileName}"))
            {
                Debug.Log($"file: [{path}/{fileName}] was found.");
                return;
            }

            Debug.Log($"file: [{path}/{fileName}] not found. Creating it for you.");
            StringEventChannelSO asset = ScriptableObject.CreateInstance<StringEventChannelSO>();

            AssetDatabase.CreateAsset(asset, $"{path}/{fileName}");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

        public static void CreateStringFloatEventFile(string path, string fileName)
        {
            if (File.Exists($"{path}/{fileName}"))
            {
                Debug.Log($"file: [{path}/{fileName}] was found.");
                return;
            }

            Debug.Log($"file: [{path}/{fileName}] not found. Creating it for you.");
            StringFloatEventChannelSO asset = ScriptableObject.CreateInstance<StringFloatEventChannelSO>();

            AssetDatabase.CreateAsset(asset, $"{path}/{fileName}");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        
    }
    #endif
}