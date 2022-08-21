using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Resources;
using Launcher.Properties;
using System.Collections.Generic;

namespace Launcher
{
	class Program
	{
		static void Main(string[] args)
		{
			string SteamFolder = args[0];
			Login = args[1].ToLower();
			string Password = args[2];
			string xPos = args[3];
			string yPos = args[4];
			ResX = args[5];
			ResY = args[6];

			JObject accInfo = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Loginusers.json"));
			string SteamID = accInfo[Login]["SteamID"].ToString();
			long SteamID3 = long.Parse(SteamID) - 76561197960265728;

			Config = args[7];
			string CSGOFolder = args[9];

            if (Config.Contains("LEADER"))
				EngineFocusSleep = "engine_no_focus_sleep 50";

			switch (args[8])
			{
				case "0":
                    LogFile = @"con_logfile log/0.log";
                    Port = "3001";
                    break;
				case "1":
                    LogFile = @"con_logfile log/1.log";
                    Port = "3002";
                    break;
			}

			LoadSet();

			if (short.Parse(args[8]) == 0 || short.Parse(args[8]) == 2)
				File.WriteAllText(CSGOFolder + @"\csgo_" + SteamID3 + @"\cfg\gamestate_integration_boost.cfg", GamestateIntegration);
			if (File.Exists(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\config.cfg"))
				File.SetAttributes(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\config.cfg", FileAttributes.Normal);
			if (File.Exists(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\video.txt"))
				File.SetAttributes(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\video.txt", FileAttributes.Normal);
			if (File.Exists(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\videodefaults.txt"))
				File.SetAttributes(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\videodefaults.txt", FileAttributes.Normal);

            List<string> directories = new List<string>()
            {
                $@"{CSGOFolder}\csgo_{SteamID3}\cfg\",
                $@"{CSGOFolder}\csgo_{SteamID3}\panorama\videos\",
                $@"{SteamFolder}\userdata\{SteamID3}\730\local\cfg",
            };
            foreach (string directory in directories)
                Directory.CreateDirectory(directory);

            Dictionary<string, string> files = new Dictionary<string, string>()
            {
                { $@"{CSGOFolder}\csgo_{SteamID3}\cfg\autoexec.cfg", Autoexec },
                { $@"{CSGOFolder}\csgo_{SteamID3}\cfg\server_blacklist.txt", Resources.server_blacklist },
                { $@"{CSGOFolder}\csgo_{SteamID3}\GameInfo.txt",  GameInfo },
                { $@"{SteamFolder}\userdata\{SteamID3}\730\local\cfg\config.cfg",  Resources.config },
                { $@"{SteamFolder}\userdata\{SteamID3}\730\local\cfg\video.txt",  Resources.video },
                { $@"{SteamFolder}\userdata\{SteamID3}\730\local\cfg\videodefaults.txt",  Resources.videodefaults },
            };
            foreach (var kvp in files)
                File.WriteAllText(kvp.Key, kvp.Value);

            Process.Start(SteamFolder + @"\Steam.exe",
				$"-login {Login} {Password} -console -applaunch 730 " +
				$"-language {SteamID3} -x {xPos} -y {yPos} -low -nohltv " +
				$"-nojoy -novid -nosound -nopreload -nopreloadmodels -nopreloadsounds");
        }

		public static string EngineFocusSleep, ResX, ResY, LogFile, Login,
			Config, Port, Autoexec, GameInfo, GamestateIntegration;

		public static void LoadSet()
		{
			Autoexec = $@"+left
+right
sv_max_allowed_developer 1
developer 1
con_filter_enable 2
con_filter_text match_id
mat_setvideomode {ResX} {ResY} 1
{EngineFocusSleep}
{LogFile}

clear
echo  IziBoost";

			GameInfo = $@"""GameInfo""
{{
	game	""LOGIN: {Login} | {Config}""
	title	""COUNTER-STRIKE'""
	title2	""GO""
	type multiplayer_only
	nomodels 1
	nohimodel 1
	nocrosshair 0
	bots 1
	hidden_maps
	{{
		""test_speakers""		1
		""test_hardware""		1
	}}
	nodegraph 0
	SupportsXbox360 1
	SupportsDX8	0
	GameData	""csgo.fgd""


	FileSystem
	{{
		SteamAppId				730		// This will mount all the GCFs we need (240=CS:S, 220=HL2).
		ToolsAppId				211		// Tools will load this (ie: source SDK caches) to get things like materials\debug, materials\editor, etc.

		//
		// The code that loads this file automatically does a few things here:
		//
		// 1. For each ""Game"" search path, it adds a ""GameBin"" path, in <dir>\bin
		// 2. For each ""Game"" search path, it adds another ""Game"" path in front of it with _<langage> at the end.
		//    For example: c:\hl2\cstrike on a french machine would get a c:\hl2\cstrike_french path added to it.
		// 3. For the first ""Game"" search path, it adds a search path called ""MOD"".
		// 4. For the first ""Game"" search path, it adds a search path called ""DEFAULT_WRITE_PATH"".
		//

		//
		// Search paths are relative to the base directory, which is where hl2.exe is found.
		//
		// |gameinfo_path| points at the directory where gameinfo.txt is.
		// We always want to mount that directory relative to gameinfo.txt, so
		// people can mount stuff in c:\mymod, and the main game resources are in
		// someplace like c:\program files\valve\steam\steamapps\<username>\half-life 2.
		//
		SearchPaths
		{{
			Game				|gameinfo_path|.
			Game				csgo
		}}
	}}
}}";

			GamestateIntegration = $@"""CSGSI Example""
{{
	""uri"" ""http://localhost:{Port}""
	""timeout"" ""5.0""
	""auth""
	{{
		""token""				""boost_panel""
	}}
	""data""
	{{
		""provider""              	""0""
		""map""                   	""1""
		""round""                 	""1""
		""player_id""					""0""
		""player_weapons""			""0""
		""player_match_stats""		""0""
		""player_state""				""0""
		""allplayers_id""				""0""
		""allplayers_state""			""0""
		""allplayers_match_stats""	""0""
	}}
}}";
		}
	}
}