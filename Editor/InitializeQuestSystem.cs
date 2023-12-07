using System.IO;
using jeanf.EventSystem;
using UnityEditor;
using UnityEngine;

namespace jeanf.questsystem
{
    public class InitializeQuestSystem : MonoBehaviour
    {
        [MenuItem("GameObject/Initialize Quest System")]
        private static void initializeQuestSystem()
        {
            var folder = "Quests";
            var questFolderPath = $"Assets/Resources/{folder}";
            
            var channelFolder = "Channels";
            var channelFolderPath = $"{questFolderPath}/{channelFolder}";
            
            // files needed
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

            CreateQuestStartEventChannelFile(channelFolderPath, startQuestEventChannel);
            CreateQuestProgressChannelFile(channelFolderPath, questProgressChannel);
        }
        
        public static void CreateQuestStartEventChannelFile(string path, string startEventChannel)
        {
            if (File.Exists($"{path}/{startEventChannel}"))
            {
                Debug.Log($"file: [{path}/{startEventChannel}] was found.");
                return;
            }

            Debug.Log($"file: [{path}/{startEventChannel}] not found. Creating it for you.");
            StringEventChannelSO asset = ScriptableObject.CreateInstance<StringEventChannelSO>();

            AssetDatabase.CreateAsset(asset, $"{path}/{startEventChannel}");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

        public static void CreateQuestProgressChannelFile(string path, string questProgressChannel)
        {
            if (File.Exists($"{path}/{questProgressChannel}"))
            {
                Debug.Log($"file: [{path}/{questProgressChannel}] was found.");
                return;
            }

            Debug.Log($"file: [{path}/{questProgressChannel}] not found. Creating it for you.");
            StringFloatEventChannelSO asset = ScriptableObject.CreateInstance<StringFloatEventChannelSO>();

            AssetDatabase.CreateAsset(asset, $"{path}/{questProgressChannel}");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}