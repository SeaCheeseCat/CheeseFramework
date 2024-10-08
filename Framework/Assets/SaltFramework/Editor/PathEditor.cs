using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public  class PathEditor : EditorWindow
{
    private const string TortoiseGitPathKey = "TortoiseGitPath";
    private const string SourceTreeGitPathKey = "SourceTreeGitPath";
    public static string tortoiseGitPath;
    public static string sourceTreeGitPath;
    
    [MenuItem("▷ SaltFramework/资源路径", priority = 10)]
    public static void Open()
    {
        PathEditor window = GetWindow<PathEditor>("资源路径", true);
        
    }

    [MenuItem("▷ SaltFramework/打开调试UI", priority = 10)]
    public static void DebugOpen()
    {
        UIManager.Instance.OpenUI<DebugUI>();
    }
    [MenuItem("▷ SaltFramework/重启项目", priority = 10)]
    public static void OpenTortoiseGit()
    {
        /*LoadPaths();
        OpenGitTool(tortoiseGitPath, "TortoiseGit");*/
        EditorApplication.OpenProject(Application.dataPath.Replace("Assets", string.Empty));
    }

    public static void SavePaths()
    {
        DebugEX.Log("保存", tortoiseGitPath);
        EditorPrefs.SetString(TortoiseGitPathKey, tortoiseGitPath);
        EditorPrefs.SetString(SourceTreeGitPathKey, sourceTreeGitPath);
    }

    public static string PersistentDataPath
    {
        get
        {
            string path =
#if UNITY_ANDROID
         Application.persistentDataPath;
#elif UNITY_IPHONE && !UNITY_EDITOR
         Application.persistentDataPath;
#elif UNITY_STANDLONE_WIN || UNITY_EDITOR
         Application.persistentDataPath;
#else
        string.Empty;
#endif
            return path;
        }
    }




    public static string ConfigExcelPath
    {
        get
        {
            string path = "../../All-Doe-s-Life-Res/All-Doe-s-Life-Tool/ExcelTool/ExcelConfig";
            return path;
        }
    }

    public static string ConfigBatPath
    {
        get
        {
            string path = "../Tool/ExcelTool";
            return path;
        }
    }

    private void OnGUI()
    {
        DebugEX.Log("ONGUI");
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width), GUILayout.Height(position.height));
        {
            GUILayout.Label("（各种各样的路径）（目移）");
            GUILayout.Label("\n哦... ...\n你想查阅些什么呢... ...", EditorStyles.boldLabel);
            if (GUILayout.Button("存档路径", GUILayout.Height(45f), GUILayout.Width(100f)))
            {
                DirectoryInfo direction = new DirectoryInfo(PersistentDataPath);
                System.Diagnostics.Process.Start(direction.FullName);
            }
            if (GUILayout.Button("清空存档", GUILayout.Height(45f), GUILayout.Width(100f)))
            {
                DeletPathFile(PersistentDataPath);
            }
            if (GUILayout.Button("配置表路径", GUILayout.Height(45f), GUILayout.Width(100f)))
            {
                DirectoryInfo direction = new DirectoryInfo(ConfigExcelPath);
                System.Diagnostics.Process.Start(direction.FullName);
            }
            if (GUILayout.Button("生成配置", GUILayout.Height(45f), GUILayout.Width(100f)))
            {
                
            }

        }
        EditorGUILayout.EndVertical();
    }


    public static void DeletPathFile(string path)
    {
        DirectoryInfo direction = new DirectoryInfo(path);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith(".meta"))
            {
                continue;
            }
            string FilePath = path + "/" + files[i].Name;
            File.Delete(FilePath);
        }
    }





    public static System.Diagnostics.Process CreateShellExProcess(string cmd, string args, string workingDir = "")

    {

        var pStartInfo = new System.Diagnostics.ProcessStartInfo(cmd);

        pStartInfo.Arguments = args;

        pStartInfo.CreateNoWindow = false;

        pStartInfo.UseShellExecute = true;

        pStartInfo.RedirectStandardError = false;

        pStartInfo.RedirectStandardInput = false;

        pStartInfo.RedirectStandardOutput = false;

        if (!string.IsNullOrEmpty(workingDir))

            pStartInfo.WorkingDirectory = workingDir;

        return System.Diagnostics.Process.Start(pStartInfo);

    }




    public static void RunBat(string batfile, string args, string workingDir = "")

    {

        var p = CreateShellExProcess(batfile, args, workingDir);

        p.Close();

    }



    public static string FormatPath(string path)
    {

        path = path.Replace("/", "\\");

        if (Application.platform == RuntimePlatform.OSXEditor)

            path = path.Replace("\\", "/");
        return path;
    }

}