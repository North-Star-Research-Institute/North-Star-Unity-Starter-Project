/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2018.                                 *
 * Leap Motion proprietary and confidential.                                  *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Leap.Unity.Packaging {
  using Attributes;

  [CreateAssetMenu(fileName = "Build", menuName = "Build Definition", order = 201)]
  public class BuildDefinition : DefinitionBase {
    private const string BUILD_EXPORT_FOLDER_KEY = "LeapBuildDefExportFolder";
    private const string DEFAULT_BUILD_NAME = "Build.asset";

    [SerializeField]
    protected bool _trySuffixWithGitHash = false;

#if UNITY_EDITOR
    [Tooltip("The options to enable for this build.")]
    [EnumFlags]
    [SerializeField]
    protected BuildOptions _options = BuildOptions.None;

    [Tooltip("If disabled, the editor's current Player Settings will be used. " +
      "Not all Player Settings are supported. Any unspecified settings are " +
      "inherited from the editor's current Player Settings.")]
    [SerializeField]
    protected bool _useSpecificPlayerSettings = true;

    [System.Serializable]
    public struct BuildPlayerSettings {
      public FullScreenMode fullScreenMode;
      public int defaultScreenWidth;
      public int defaultScreenHeight;
      public bool resizableWindow;
      public static BuildPlayerSettings Default() {
        return new BuildPlayerSettings() {
          fullScreenMode = FullScreenMode.Windowed,
          defaultScreenWidth = 800,
          defaultScreenHeight = 500,
          resizableWindow = true
        };
      }
    }
    [Tooltip("Only used when Use Specific Player Settings is true. " +
      "Not all Player Settings are supported. Any unspecified settings " +
      "are inherited from the editor's current Player Settings.")]
    [SerializeField]
    protected BuildPlayerSettings _playerSettings =
      BuildPlayerSettings.Default();

    [Tooltip("The scenes that should be included in this build, " + "" +
             "in the order they should be included.")]
    [SerializeField]
    protected SceneAsset[] _scenes;

    [Tooltip("The build targets to use for this build definition.")]
    [SerializeField]
    protected BuildTarget[] _targets = { BuildTarget.StandaloneWindows64 };

    public BuildDefinition() {
      _definitionName = "Build";
    }

    public static void Build(string guid) {
      var path = AssetDatabase.GUIDToAssetPath(guid);
      var def = AssetDatabase.LoadAssetAtPath<BuildDefinition>(path);
      def.Build();
    }

    public void Build() {
      string exportFolder;
      if (!TryGetPackageExportFolder(out exportFolder, promptIfNotDefined: true)) {
        UnityEngine.Debug.LogError("Could not build " + DefinitionName + " because no export folder was chosen.");
        return;
      }

      if (_trySuffixWithGitHash) {
        string hash;
        if (tryGetGitCommitHash(out hash)) {
          exportFolder = Path.Combine(exportFolder, DefinitionName + "_" + hash);
        } else {
          UnityEngine.Debug.LogWarning("Failed to get git hash.");
        }
      }

      string fullPath = Path.Combine(
        Path.Combine(exportFolder, _definitionName),
        DefinitionName
      );

      var buildOptions = new BuildPlayerOptions() {
        scenes = _scenes.Where(s => s != null).
                                    Select(s => AssetDatabase.GetAssetPath(s)).
                                    ToArray(),
        options = _options,
      };

      foreach (var target in _targets) {
        buildOptions.target = target;
        buildOptions.locationPathName = fullPath + "." + getFileSuffix(target);
        
        if (_useSpecificPlayerSettings) {
          var origFullscreenMode = PlayerSettings.fullScreenMode;
          var origDefaultWidth = PlayerSettings.defaultScreenWidth;
          var origDefaultHeight = PlayerSettings.defaultScreenHeight;
          var origResizableWindow = PlayerSettings.resizableWindow;
          try {
            PlayerSettings.fullScreenMode = _playerSettings.fullScreenMode;
            PlayerSettings.defaultScreenWidth = _playerSettings.defaultScreenWidth;
            PlayerSettings.defaultScreenHeight =
            _playerSettings.defaultScreenHeight;
            PlayerSettings.resizableWindow = _playerSettings.resizableWindow;

            BuildPipeline.BuildPlayer(buildOptions);
          }
          finally {
            PlayerSettings.fullScreenMode = origFullscreenMode;
            PlayerSettings.defaultScreenWidth = origDefaultWidth;
            PlayerSettings.defaultScreenHeight = origDefaultHeight;
            PlayerSettings.resizableWindow = origResizableWindow;
          }
        }
        else {
          BuildPipeline.BuildPlayer(buildOptions);
        }

        if (_options.HasFlag(BuildOptions.EnableHeadlessMode)) {
          // The -batchmode flag is the only important part of headless mode
          // for Windows. The EnableHeadlessMode build option only actually has
          // an effect on Linux standalone builds.
          // Here, it's being used to mark the _intention_ of a headless build
          // for Windows builds.
          var text = "\"" + _definitionName + ".exe" + "\" -batchmode";
          var headlessModeBatPath = Path.Combine(Path.Combine(exportFolder,
            _definitionName), "Run Headless Mode.bat");
          File.WriteAllText(headlessModeBatPath, text);
        }
      }

      Process.Start(exportFolder);
    }

    private static string getFileSuffix(BuildTarget target) {
      switch (target) {
        case BuildTarget.StandaloneWindows:
        case BuildTarget.StandaloneWindows64:
          return "exe";
        case BuildTarget.Android:
          return "apk";
        default:
          return "";
      }
    }

    private static bool tryGetGitCommitHash(out string hash) {
      try {
        Process process = new Process();

        ProcessStartInfo startInfo = new ProcessStartInfo() {
          WindowStyle = ProcessWindowStyle.Hidden,
          FileName = "cmd.exe",
          Arguments = "/C git log -1",
          WorkingDirectory = Directory.GetParent(Application.dataPath).FullName,
          RedirectStandardOutput = true,
          CreateNoWindow = true,
          UseShellExecute = false
        };

        process.StartInfo = startInfo;
        process.Start();

        string result = process.StandardOutput.ReadToEnd();

        var match = Regex.Match(result, @"^commit (\w{40})");
        if (match.Success) {
          hash = match.Groups[1].Value;
          return true;
        } else {
          hash = "";
          return false;
        }
      } catch (Exception e) {
        UnityEngine.Debug.LogException(e);
        hash = "";
        return false;
      }
    }

    [MenuItem("Build/All Apps", priority = 1)]
    public static void BuildAll() {
      foreach (var item in EditorResources.FindAllAssetsOfType<BuildDefinition>()) {
        item.Build();
      }
    }

    [MenuItem("Build/All Apps", priority = 1, validate = true)]
    public static bool ValidateBuildAll() {
      return EditorResources.FindAllAssetsOfType<BuildDefinition>().Length > 0;
    }
#endif
  }
}
