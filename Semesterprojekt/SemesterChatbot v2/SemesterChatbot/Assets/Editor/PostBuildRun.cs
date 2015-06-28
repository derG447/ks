using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class PostBuildRun : MonoBehaviour {

    //[MenuItem("MyTools/Windows-Build with Postprocess and Run")]
    public static void BuildGameAndRun()
    {
        // Get filename.
        string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        string[] levels = new string[] { "Assets/Scene/Main.unity" };

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + "/BuiltGame.exe", BuildTarget.StandaloneWindows, BuildOptions.None);

        // Copy a file from the project folder to the build folder, alongside the built game.
        FileUtil.ReplaceFile("Assets/Plugins/aiml", path + "/aiml");
        FileUtil.ReplaceFile("Assets/Plugins/config", path + "/config");

        // Run the game (Process class from System.Diagnostics).

        Process proc = new Process();
        proc.StartInfo.FileName = path + "/BuiltGame.exe";
        proc.Start();
    }
}
