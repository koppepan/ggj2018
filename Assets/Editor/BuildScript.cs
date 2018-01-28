using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Editor
{
	public class BuildScript
	{
		static readonly BuildOptions devOptions = BuildOptions.Development | BuildOptions.AllowDebugging;
		static readonly BuildOptions prodOptions = BuildOptions.None;

		[MenuItem("Build/Check Current IP Address")]
		public static void CheckIpAddress()
		{
			Debug.Log (IPAddress.Parse (UnityEngine.Network.player.ipAddress).ToString ());
		}
	
		[MenuItem("Build/Development/Client")]
		public static void BuildDevelopmentClient()
		{
			var target = EditorUserBuildSettings.activeBuildTarget;
			var path = target == BuildTarget.StandaloneOSX ? "Build/Development/DevClient.app" : "Build/Development/DevClient.exe";

			Build (path, target, devOptions, true, true);
		}

		[MenuItem("Build/Development/Server")]
		public static void BuildDevelopmentServer()
		{
			var target = EditorUserBuildSettings.activeBuildTarget;
			var path = target == BuildTarget.StandaloneOSX ? "Build/Development/DevServer.app" : "Build/Development/DevServer.exe";

			Build (path, target, devOptions, false, true);
		}

		[MenuItem("Build/Production/Client")]
		public static void BuildProductionClient()
		{
			var target = EditorUserBuildSettings.activeBuildTarget;
			var path = target == BuildTarget.StandaloneOSX ? "Build/Production/Client.app" : "Build/Production/Client.exe";

			Build (path, target, prodOptions, true, false);
		}

		[MenuItem("Build/Production/Server")]
		public static void BuildProductionServer()
		{
			var target = EditorUserBuildSettings.activeBuildTarget;
			var path = target == BuildTarget.StandaloneOSX ? "Build/Production/Server.app" : "Build/Production/Server.exe";

			Build (path, target, prodOptions, false, false);
		}

		static void Build(string buildPath, BuildTarget target, BuildOptions options, bool client, bool debug)
		{
			var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup (BuildTargetGroup.Standalone).Split (';').ToList ();

			if (debug && !symbols.Contains ("GAME_DEBUG")) {
				symbols.Add ("GAME_DEBUG");
			} else if (!debug && symbols.Contains ("GAME_DEBUG")) {
				symbols.Remove ("GAME_DEBUG");
			}

			if (client) {
				if (!symbols.Contains ("GAME_CLIENT")) {
					symbols.Add ("GAME_CLIENT");
				}
				if (symbols.Contains ("GAME_SERVER")) {
					symbols.Remove ("GAME_SERVER");
				}
			} else {
				if (symbols.Contains ("GAME_CLIENT")) {
					symbols.Remove ("GAME_CLIENT");
				}
				if (!symbols.Contains ("GAME_SERVER")) {
					symbols.Add ("GAME_SERVER");
				}
			}

			PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.Standalone, string.Join(";", symbols.Distinct().ToArray()));

			UnityEditor.BuildPipeline.BuildPlayer (
				UnityEditor.EditorBuildSettings.scenes,
				buildPath,
				target,
				options);

			PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.Standalone, "GAME_DEBUG");
		}
	}
}
