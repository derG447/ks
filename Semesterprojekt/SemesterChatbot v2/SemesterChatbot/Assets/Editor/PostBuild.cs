using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class PostBuild : MonoBehaviour {

    [MenuItem("MyTools/Windows-Build with Postprocess")]
    public static void BuildGame()
    {
        // Get filename.
        string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        //FileUtil.DeleteFileOrDirectory(path + "/BuiltGame.exe");

        string[] levels = new string[] { "Assets/Scene/Main.unity" };

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + "/BuiltGame.exe", BuildTarget.StandaloneWindows, BuildOptions.None);

        // Copy a file from the project folder to the build folder, alongside the built game.
        FileUtil.ReplaceFile("Assets/Plugins/aiml", path + "/aiml");
        FileUtil.ReplaceFile("Assets/Plugins/config", path + "/config");
    }
}
