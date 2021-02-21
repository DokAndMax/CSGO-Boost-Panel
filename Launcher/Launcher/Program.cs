﻿using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Windows;

namespace Launcher
{
	class Program
	{
		static void Main(string[] args)
		{
			bool fast = Boolean.Parse(args[0]);
			String SteamFolder = args[1];
			Login = args[2].ToLower();
			String Password = args[3];
			String xPos = args[4];
			String yPos = args[5];
			ResX = args[6];
			ResY = args[7];

			JObject accInfo = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("Loginusers.json"));

			String SteamID = accInfo[Login]["SteamID"].ToString();
			long SteamID3 = Int64.Parse(SteamID) - 76561197960265728;

			if (fast)
			{
				Process.Start(SteamFolder + @"\Steam.exe", "-login " + Login + " " + Password + " -console -applaunch 730 -nojoy -novid -language " + SteamID3 + " -low -nohltv -nosound -w 640 -h 480 -x " + xPos + " -y " + yPos);
				return;
			}

			Config = args[8];
			String CSGOFolder = args[10];

			if (Config.Contains("LEADER"))
				EngineFocusSleep = "engine_no_focus_sleep 50";

			if (Directory.Exists(CSGOFolder + @"\csgo_" + SteamID3))
			{
				Directory.Delete(CSGOFolder + @"\csgo_" + SteamID3, true);
			}
			Directory.CreateDirectory(CSGOFolder + @"\csgo_" + SteamID3 + @"\cfg\");
			Directory.CreateDirectory(CSGOFolder + @"\csgo_" + SteamID3 + @"\panorama\videos\");
			if (args[9] == "0")
			{
				logg = @"con_logfile log/0.log";
				port = "3001";
			}

			if (args[9] == "2")
			{
				logg = @"con_logfile log/1.log";
				port = "3002";
			}

			if (args[9] == "1" || args[9] == "3" || args[9] == null)
			{
				logg = @"//con_logfile log/0.log";
			}

			LoadSet();

			if (Int16.Parse(args[9]) == 0 || Int16.Parse(args[9]) == 2)
				File.WriteAllText(CSGOFolder + @"\csgo_" + SteamID3 + @"\cfg\gamestate_integration_boost.cfg", GamestateIntegration);
			File.WriteAllText(CSGOFolder + @"\csgo_" + SteamID3 + @"\cfg\autoexec.cfg", Autoexec);
			File.WriteAllText(CSGOFolder + @"\csgo_" + SteamID3 + @"\cfg\server_blacklist.txt", ServerBlackList);
			File.WriteAllText(CSGOFolder + @"\csgo_" + SteamID3 + @"\gameinfo.txt", GameInfo);
			if (Directory.Exists(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\"))
			{
				if(File.Exists(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\config.cfg"))
					File.SetAttributes(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\config.cfg", FileAttributes.Normal);
				if (File.Exists(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\video.txt"))
					File.SetAttributes(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\video.txt", FileAttributes.Normal);
				if (File.Exists(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\videodefaults.txt"))
					File.SetAttributes(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\videodefaults.txt", FileAttributes.Normal);
				Directory.Delete(SteamFolder + @"\userdata\" + SteamID3 + @"\730", true);
			}
			Directory.CreateDirectory(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg");
			File.WriteAllText(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\config.cfg", userconfig);
			File.WriteAllText(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\video.txt", uservideo);
			File.WriteAllText(SteamFolder + @"\userdata\" + SteamID3 + @"\730\local\cfg\videodefaults.txt", uservideodefaults);

			Process.Start(SteamFolder + @"\Steam.exe", "-login " + Login + " " + Password + " -console -applaunch 730 -novid -nojoy -language " + SteamID3 + " -low +mat_queue_mode 0 -nohltv -nosound -w 640 -h 480 -x " + xPos + " -y " + yPos);
		}

		public static string ResX, ResY, Config, Login, logg, uservideodefaults, Autoexec, GameInfo, GamestateIntegration, ServerBlackList, userconfig, uservideo, port, EngineFocusSleep;

		public static void LoadSet()
		{
			Autoexec = @"+left
+right
mm_dedicated_search_maxping 350
con_enable 1
con_filter_enable 2
developer 1
con_filter_text match_id
bind F10 disconnect
" + EngineFocusSleep + @"
m_rawinput 0
mat_setvideomode " + ResX + " " + ResY + @" 1

clear
echo  IziBoost

" + logg + @"

bind mwheeldown disconnect";
			GameInfo = @"""GameInfo""
{
	game	""LOGIN: " + Login + @" | " + Config + @"""
	title	""COUNTER-STRIKE'""
	title2	""GO""
	type multiplayer_only
	nomodels 1
	nohimodel 1
	nocrosshair 0
	bots 1
	hidden_maps
	{
		""test_speakers""		1
		""test_hardware""		1
	}
	nodegraph 0
	SupportsXbox360 1
	SupportsDX8	0
	GameData	""csgo.fgd""


	FileSystem
	{
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
		{
			Game				|gameinfo_path|.
			Game				csgo
		}
	}
}";
			ServerBlackList = @"""serverblacklist""
{
}";

			GamestateIntegration = @"""CSGSI Example""
{
	""uri"" ""http://localhost:" + port + @"""
	""timeout"" ""5.0""
	""auth""
	{
		""token""				""boost_panel""
	}
	""data""
	{
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
	}
}";


			userconfig = @"unbindall
bind ""0"" ""slot10""
bind ""1"" ""slot1""
bind ""2"" ""slot2""
bind ""3"" ""slot3""
bind ""4"" ""slot4""
bind ""5"" ""slot5""
bind ""6"" ""slot6""
bind ""7"" ""slot7""
bind ""8"" ""slot8""
bind ""9"" ""slot9""
bind ""a"" ""+moveleft""
bind ""b"" ""buymenu""
bind ""d"" ""+moveright""
bind ""e"" ""+use""
bind ""f"" ""+lookatweapon""
bind ""g"" ""drop""
bind ""i"" ""show_loadout_toggle""
bind ""k"" ""+voicerecord""
bind ""m"" ""teammenu""
bind ""q"" ""lastinv""
bind ""r"" ""+reload""
bind ""s"" ""+back""
bind ""t"" ""+spray_menu""
bind ""u"" ""messagemode2""
bind ""w"" ""+forward""
bind ""x"" ""slot12""
bind ""y"" ""messagemode""
bind ""z"" ""+radialradio2""
bind ""`"" ""toggleconsole""
bind "","" ""buyammo1""
bind ""."" ""buyammo2""
bind ""SPACE"" ""+jump""
bind ""TAB"" ""+showscores""
bind ""ESCAPE"" ""cancelselect""
bind ""DEL"" ""mute""
bind ""PAUSE"" ""pause""
bind ""SHIFT"" ""+speed""
bind ""CTRL"" ""+duck""
bind ""F3"" ""autobuy""
bind ""F4"" ""rebuy""
bind ""F5"" ""jpeg""
bind ""F6"" ""save quick""
bind ""F7"" ""load quick""
bind ""F10"" ""disconnect""
bind ""MOUSE1"" ""+attack""
bind ""MOUSE2"" ""+attack2""
bind ""MOUSE3"" ""player_ping""
bind ""MWHEELUP"" ""invprev""
bind ""MWHEELDOWN"" ""invnext""
@panorama_debug_overlay_opacity ""0.8""
adsp_debug ""0""
ai_report_task_timings_on_limit ""0""
ai_think_limit_label ""0""
budget_averages_window ""30""
budget_background_alpha ""128""
budget_bargraph_background_alpha ""128""
budget_bargraph_range_ms ""16.6666666667""
budget_history_numsamplesvisible ""100""
budget_history_range_ms ""66.666666667""
budget_panel_bottom_of_history_fraction "".25""
budget_panel_height ""384""
budget_panel_width ""512""
budget_panel_x ""0""
budget_panel_y ""50""
budget_peaks_window ""30""
budget_show_averages ""0""
budget_show_history ""1""
budget_show_peaks ""1""
bugreporter_uploadasync ""0""
bugreporter_username """"
c_maxdistance ""200""
c_maxpitch ""90""
c_maxyaw ""135""
c_mindistance ""30""
c_minpitch ""0""
c_minyaw ""-135""
c_orthoheight ""100""
c_orthowidth ""100""
c_thirdpersonshoulder ""false""
c_thirdpersonshoulderaimdist ""120.0""
c_thirdpersonshoulderdist ""40.0""
c_thirdpersonshoulderheight ""5.0""
c_thirdpersonshoulderoffset ""20.0""
cachedvalue_count_partybrowser ""1607773456""
cachedvalue_count_teammates ""1607773441""
cam_collision ""1""
cam_idealdelta ""4.0""
cam_idealdist ""150""
cam_idealdistright ""0""
cam_idealdistup ""0""
cam_ideallag ""4.0""
cam_idealpitch ""0""
cam_idealyaw ""0""
cam_snapto ""0""
cc_lang """"
cc_linger_time ""1.0""
cc_predisplay_time ""0.25""
cc_subtitles ""0""
chet_debug_idle ""0""
cl_allowdownload ""1""
cl_allowupload ""1""
cl_autohelp ""1""
cl_autowepswitch ""0""
cl_bob_lower_amt ""21""
cl_bobamt_lat ""0.33""
cl_bobamt_vert ""0.14""
cl_bobcycle ""0.98""
cl_buywheel_nomousecentering ""0""
cl_buywheel_nonumberpurchasing ""0""
cl_chatfilter_version ""0""
cl_chatfilters ""63""
cl_clanid ""0""
cl_cmdrate ""128""
cl_color ""0""
cl_compass_enabled ""1""
cl_crosshair_drawoutline ""1""
cl_crosshair_dynamic_maxdist_splitratio ""0.35""
cl_crosshair_dynamic_splitalpha_innermod ""1""
cl_crosshair_dynamic_splitalpha_outermod ""0.5""
cl_crosshair_dynamic_splitdist ""7""
cl_crosshair_friendly_warning ""1""
cl_crosshair_outlinethickness ""1""
cl_crosshair_sniper_show_normal_inaccuracy ""0""
cl_crosshair_sniper_width ""1""
cl_crosshair_t ""0""
cl_crosshairalpha ""200""
cl_crosshaircolor ""1""
cl_crosshaircolor_b ""50""
cl_crosshaircolor_g ""250""
cl_crosshaircolor_r ""50""
cl_crosshairdot ""0""
cl_crosshairgap ""-1.500000""
cl_crosshairgap_useweaponvalue ""0""
cl_crosshairsize ""1.300000""
cl_crosshairstyle ""4""
cl_crosshairthickness ""0.200000""
cl_crosshairusealpha ""1""
cl_debugrumble ""0""
cl_detail_avoid_force ""0""
cl_detail_avoid_radius ""0""
cl_detail_avoid_recover_speed ""0""
cl_detail_max_sway ""0""
cl_disable_round_end_report ""0""
cl_disablefreezecam ""0""
cl_disablehtmlmotd ""1""
cl_dm_buyrandomweapons ""1""
cl_downloadfilter ""all""
cl_dz_playagain_auto_spectate ""0""
cl_embedded_stream_audio_volume ""60""
cl_embedded_stream_audio_volume_xmaster ""1""
cl_fixedcrosshairgap ""3""
cl_forcepreload ""0""
cl_freezecampanel_position_dynamic ""1""
cl_grass_mip_bias ""-0.5""
cl_hide_avatar_images """"
cl_hud_background_alpha ""0.5""
cl_hud_bomb_under_radar ""1""
cl_hud_color ""0""
cl_hud_healthammo_style ""0""
cl_hud_playercount_pos ""0""
cl_hud_playercount_showcount ""0""
cl_hud_radar_scale ""1""
cl_idealpitchscale ""0.8""
cl_inventory_saved_filter2 ""all""
cl_inventory_saved_sort2 ""inv_sort_age""
cl_invites_only_friends ""0""
cl_invites_only_mainmenu ""0""
cl_itemimages_dynamically_generated ""2""
cl_join_advertise ""1""
cl_minimal_rtt_shadows ""1""
cl_mouselook ""1""
cl_mute_all_but_friends_and_party ""0""
cl_mute_enemy_team ""0""
cl_obs_interp_enable ""1""
cl_observed_bot_crosshair ""0""
cl_observercrosshair ""1""
cl_ping_fade_deadzone ""60""
cl_ping_fade_distance ""300""
cl_player_ping_mute ""0""
cl_playerspray_auto_apply ""1""
cl_promoted_settings_acknowledged ""1:1607773595501""
cl_quickinventory_lastinv ""1""
cl_quickinventory_line_update_speed ""65.0f""
cl_radar_always_centered ""1""
cl_radar_icon_scale_min ""0.6""
cl_radar_rotate ""1""
cl_radar_scale ""0.7""
cl_radar_square_with_scoreboard ""1""
cl_radial_radio_tab_0_text_1 ""#Chatwheel_requestspend""
cl_radial_radio_tab_0_text_2 ""#Chatwheel_requestweapon""
cl_radial_radio_tab_0_text_3 ""#Chatwheel_bplan""
cl_radial_radio_tab_0_text_4 ""#Chatwheel_followingyou""
cl_radial_radio_tab_0_text_5 ""#Chatwheel_midplan""
cl_radial_radio_tab_0_text_6 ""#Chatwheel_followme""
cl_radial_radio_tab_0_text_7 ""#Chatwheel_aplan""
cl_radial_radio_tab_0_text_8 ""#Chatwheel_requestecoround""
cl_radial_radio_tab_1_text_1 ""#Chatwheel_enemyspotted""
cl_radial_radio_tab_1_text_2 ""#Chatwheel_needbackup""
cl_radial_radio_tab_1_text_3 ""#Chatwheel_bplan""
cl_radial_radio_tab_1_text_4 ""#Chatwheel_bombcarrierspotted""
cl_radial_radio_tab_1_text_5 ""#Chatwheel_multipleenemieshere""
cl_radial_radio_tab_1_text_6 ""#Chatwheel_sniperspotted""
cl_radial_radio_tab_1_text_7 ""#Chatwheel_aplan""
cl_radial_radio_tab_1_text_8 ""#Chatwheel_inposition""
cl_radial_radio_tab_2_text_1 ""#Chatwheel_affirmative""
cl_radial_radio_tab_2_text_2 ""#Chatwheel_negative""
cl_radial_radio_tab_2_text_3 ""#Chatwheel_compliment""
cl_radial_radio_tab_2_text_4 ""#Chatwheel_thanks""
cl_radial_radio_tab_2_text_5 ""#Chatwheel_cheer""
cl_radial_radio_tab_2_text_6 ""#Chatwheel_peptalk""
cl_radial_radio_tab_2_text_7 ""#Chatwheel_sorry""
cl_radial_radio_tab_2_text_8 ""#Chatwheel_sectorclear""
cl_radial_radio_version_reset ""12""
cl_radialmenu_deadzone_size ""0.04""
cl_righthand ""1""
cl_rumblescale ""1.0""
cl_sanitize_player_names ""0""
cl_scoreboard_mouse_enable_binding ""+attack2""
cl_scoreboard_survivors_always_on ""0""
cl_show_clan_in_death_notice ""1""
cl_show_observer_crosshair ""1""
cl_showhelp ""1""
cl_showloadout ""1""
cl_showpluginmessages2 ""0""
cl_sniper_delay_unscope ""0""
cl_spec_follow_grenade_key ""0""
cl_spec_mode ""0""
cl_tablet_mapmode ""1""
cl_teamid_overhead_mode ""2""
cl_teammate_colors_show ""1""
cl_thirdperson ""0""
cl_timeout ""30""
cl_updaterate ""128""
cl_use_opens_buy_menu ""1""
cl_versus_intro ""1""
cl_viewmodel_shift_left_amt ""1.5""
cl_viewmodel_shift_right_amt ""0.75""
closecaption ""0""
closeonbuy ""0""
commentary_firstrun ""0""
con_allownotify ""1""
con_enable ""1""
crosshair ""1""
demo_index ""0""
demo_index_max_other ""500""
dsp_enhance_stereo ""0""
engine_no_focus_sleep ""150""
force_audio_english ""0""
func_break_max_pieces ""15""
g15_update_msec ""250""
gameinstructor_enable ""0""
hud_scaling ""0.85""
hud_showtargetid ""1""
hud_takesshots ""0""
joy_accelmax ""1.0""
joy_accelscale ""3.5""
joy_accelscalepoly ""0.4""
joy_advanced ""0""
joy_advaxisr ""0""
joy_advaxisu ""0""
joy_advaxisv ""0""
joy_advaxisx ""0""
joy_advaxisy ""0""
joy_advaxisz ""0""
joy_autoaimdampen ""0""
joy_autoAimDampenMethod ""0""
joy_autoaimdampenrange ""0""
joy_axisbutton_threshold ""0.3""
joy_cfg_preset ""1""
joy_circle_correct ""1""
joy_curvepoint_1 ""0.001""
joy_curvepoint_2 ""0.4""
joy_curvepoint_3 ""0.75""
joy_curvepoint_4 ""1""
joy_curvepoint_end ""2""
joy_diagonalpov ""0""
joy_display_input ""0""
joy_forwardsensitivity ""-1""
joy_forwardthreshold ""0.15""
joy_gamma ""0.2""
joy_inverty ""0""
joy_lowend ""1""
joy_lowend_linear ""0.55""
joy_lowmap ""1""
joy_movement_stick ""0""
joy_name ""joystick""
joy_no_accel_jump ""0""
joy_pitchsensitivity ""-1""
joy_pitchthreshold ""0.15""
joy_response_look ""0""
joy_response_look_pitch ""1""
joy_response_move ""1""
joy_sensitive_step0 ""0.1""
joy_sensitive_step1 ""0.4""
joy_sensitive_step2 ""0.90""
joy_sidesensitivity ""1""
joy_sidethreshold ""0.15""
joy_wingmanwarrior_centerhack ""0""
joy_wingmanwarrior_turnhack ""0""
joy_yawsensitivity ""-1""
joy_yawthreshold ""0.15""
joystick ""1""
joystick_force_disabled ""1""
joystick_force_disabled_set_from_options ""1""
key_bind_version ""5""
lobby_default_privacy_bits2 ""1""
lockMoveControllerRet ""0""
lookspring ""0""
lookstrafe ""0""
m_customaccel ""0""
m_customaccel_exponent ""1.05""
m_customaccel_max ""0""
m_customaccel_scale ""0.04""
m_forward ""1""
m_mouseaccel1 ""0""
m_mouseaccel2 ""0""
m_mousespeed ""1""
m_pitch ""0.022""
m_rawinput ""0""
m_side ""0.8""
m_yaw ""0.022""
mapoverview_icon_scale ""1.0""
mat_enable_uber_shaders ""0""
mat_monitorgamma ""2.2""
mat_monitorgamma_tv_enabled ""0""
mat_powersavingsmode ""0""
mat_queue_report ""0""
mat_spewalloc ""0""
mat_texture_list_content_path """"
mc_accel_band_size ""0.5""
mc_dead_zone_radius ""0.06""
mc_max_pitchrate ""100.0""
mc_max_yawrate ""230.0""
mm_csgo_community_search_players_min ""3""
mm_dedicated_search_maxping ""350.000000""
mm_server_search_lan_ports ""27015,27016,27017,27018,27019,27020""
muzzleflash_light ""1""
name ""invoker""
net_allow_multicast ""1""
net_graph ""0""
net_graphheight ""64""
net_graphholdsvframerate ""0""
net_graphipc ""0""
net_graphmsecs ""400""
net_graphpos ""1""
net_graphproportionalfont ""1""
net_graphshowinterp ""1""
net_graphshowlatency ""1""
net_graphshowsvframerate ""0""
net_graphsolid ""1""
net_graphtext ""1""
net_maxroutable ""1200""
net_scale ""5""
net_steamcnx_allowrelay ""1""
npc_height_adjust ""1""
option_duck_method ""0""
option_speed_method ""0""
password """"
play_distance ""1""
player_botdifflast_s ""2""
player_competitive_maplist_2v2_9_1_EB331822 ""mg_de_inferno""
player_competitive_maplist_8_9_1_73271C88 ""mg_cs_office""
player_nevershow_communityservermessage ""1""
player_survival_list_9_1_B ""mg_dz_blacksite,mg_dz_sirocco,mg_dz_frostbite""
player_teamplayedlast ""3""
player_wargames_list2_9_1_E04 ""mg_skirmish_flyingscoutsman,mg_skirmish_armsrace,mg_skirmish_demolitio""
player_wargames_retakes_list_9_1_E04 ""mg_skirmish_retakes""
r_drawmodelstatsoverlaymax ""1.5""
r_drawmodelstatsoverlaymin ""0.1""
r_drawtracers_firstperson ""1""
r_eyegloss ""1""
r_eyemove ""1""
r_eyeshift_x ""0""
r_eyeshift_y ""0""
r_eyeshift_z ""0""
r_eyesize ""0""
r_player_visibility_mode ""0""
rate ""786432""
safezonex ""1.0""
safezoney ""1.0""
sc_enable ""1.0""
sc_joystick_map ""1""
sc_pitch_sensitivity ""1.0""
sc_yaw_sensitivity ""1.0""
scene_showfaceto ""0""
scene_showlook ""0""
scene_showmoveto ""0""
scene_showunlock ""0""
sensitivity ""2.5""
sk_autoaim_mode ""1""
skill ""1""
snd_deathcamera_volume ""0.3""
snd_duckerattacktime ""0.5""
snd_duckerreleasetime ""2.5""
snd_duckerthreshold ""0.15""
snd_ducking_off ""1""
snd_ducktovolume ""0.55""
snd_dzmusic_volume ""0.2""
snd_hrtf_distance_behind ""100""
snd_hrtf_voice_delay ""0.1""
snd_hwcompat ""0""
snd_mapobjective_volume ""0""
snd_menumusic_volume ""0.3""
snd_mix_async ""1""
snd_mix_async_onetime_reset ""1""
snd_mixahead ""0.025""
snd_music_selection ""1""
snd_music_volume_onetime_reset_2 ""1""
snd_musicvolume_multiplier_inoverlay ""0.1""
snd_mute_losefocus ""1""
snd_mute_mvp_music_live_players ""0""
snd_mvp_volume ""1.0""
snd_pitchquality ""1""
snd_roundend_volume ""0""
snd_roundstart_volume ""0""
snd_surround_speakers ""-1""
snd_tensecondwarning_volume ""0""
sound_device_override """"
spec_replay_autostart ""1""
spec_show_xray ""0""
spec_usenumberkeys_nobinds ""1""
ss_splitmode ""0""
store_version ""1""
suitvolume ""0.25""
sv_forcepreload ""0""
sv_log_onefile ""0""
sv_logbans ""0""
sv_logecho ""1""
sv_logfile ""1""
sv_logflush ""0""
sv_logsdir ""logs""
sv_noclipaccelerate ""5""
sv_noclipspeed ""5""
sv_pvsskipanimation ""1""
sv_skyname ""sky_urb01""
sv_specaccelerate ""5""
sv_specnoclip ""1""
sv_specspeed ""3""
sv_unlockedchapters ""1""
sv_voiceenable ""1""
test_convar ""0""
texture_budget_background_alpha ""128""
texture_budget_panel_bottom_of_history_fraction "".25""
texture_budget_panel_height ""284""
texture_budget_panel_width ""512""
texture_budget_panel_x ""0""
texture_budget_panel_y ""450""
tr_best_course_time ""0""
tr_completed_training ""0""
triple_monitor_mode ""0""
trusted_launch ""1""
trusted_launch_once ""0""
tv_nochat ""0""
ui_deepstats_radio_heat_figurine ""0""
ui_deepstats_radio_heat_tab ""0""
ui_deepstats_radio_heat_team ""0""
ui_deepstats_toplevel_mode ""0""
ui_inventorysettings_recently_acknowledged """"
ui_mainmenu_bkgnd_movie_C5E107D7 ""ancient""
ui_nearbylobbies_filter3 ""competitive""
ui_news_last_read_link ""https://blog.counter-strike.net/index.php/2021/01/32375/""
ui_playsettings_maps_listen_casual ""mg_cs_office""
ui_playsettings_maps_listen_competitive ""mg_cs_office""
ui_playsettings_maps_listen_deathmatch ""random_classic""
ui_playsettings_maps_listen_scrimcomp2v2 ""mg_de_inferno""
ui_playsettings_maps_listen_skirmish ""mg_skirmish_flyingscoutsman""
ui_playsettings_maps_listen_skirmish_retakes ""mg_skirmish_retakes""
ui_playsettings_maps_official_casual ""mg_casualsigma""
ui_playsettings_maps_official_deathmatch ""mg_casualsigma""
ui_playsettings_maps_workshop """"
ui_playsettings_mode_listen ""competitive""
ui_playsettings_mode_official_v20 ""competitive""
ui_playsettings_survival_solo ""0""
ui_playsettings_warmup_map_name ""de_mirage""
ui_popup_weaponupdate_version ""2""
ui_setting_advertiseforhire_auto ""1""
ui_setting_advertiseforhire_auto_last ""/competitive""
ui_steam_overlay_notification_position ""topright""
ui_vanitysetting_loadoutslot_ct """"
ui_vanitysetting_loadoutslot_t ""smg1""
ui_vanitysetting_team ""t""
vgui_message_dialog_modal ""1""
viewmodel_fov ""60""
viewmodel_offset_x ""1""
viewmodel_offset_y ""1""
viewmodel_offset_z ""-1""
viewmodel_presetpos ""1""
viewmodel_recoil ""1.0""
voice_caster_enable ""0""
voice_caster_scale ""1""
voice_enable ""1""
voice_forcemicrecord ""1""
voice_mixer_boost ""0""
voice_mixer_mute ""0""
voice_mixer_volume ""1.0""
voice_modenable ""1""
voice_positional ""0""
voice_scale ""1.0""
voice_system_enable ""1""
voice_threshold ""4000""
volume ""0.000000""
vprof_graphheight ""256""
vprof_graphwidth ""512""
vprof_unaccounted_limit ""0.3""
vprof_verbose ""1""
vprof_warningmsec ""10""
weapon_accuracy_logging ""0""
xbox_autothrottle ""1""
xbox_throttlebias ""100""
xbox_throttlespoof ""200""
zoom_sensitivity_ratio_joystick ""1.0""
zoom_sensitivity_ratio_mouse ""1.0""
exec autoexec.cfg";
			uservideo = @"""VideoConfig""
{
	""setting.cpu_level""		""0""
	""setting.gpu_level""		""0""
	""setting.mat_antialias""		""0""
	""setting.mat_aaquality""		""0""
	""setting.mat_forceaniso""		""0""
	""setting.mat_vsync""		""0""
	""setting.mat_triplebuffered""		""0""
	""setting.mat_grain_scale_override""		""-1.0""
	""setting.gpu_mem_level""		""0""
	""setting.mem_level""		""0""
	""setting.mat_queue_mode""		""0""
	""setting.csm_quality_level""		""0""
	""setting.mat_software_aa_strength""		""0""
	""setting.mat_motion_blur_enabled""		""0""
	""setting.mat_texturestreaming""		""0""
	""setting.r_player_visibility_mode""		""0""
	""setting.mat_enable_uber_shaders""		""0""
	""setting.defaultres""		""640""
	""setting.defaultresheight""		""480""
	""setting.aspectratiomode""		""0""
	""setting.fullscreen""		""0""
	""setting.nowindowborder""		""0""
}";
			uservideodefaults = @"""config""
{
	""setting.csm_quality_level""     ""3""
	""setting.mat_software_aa_strength""      ""1""
	""VendorID""      ""4318""
	""DeviceID""      ""4546""
	""setting.fullscreen""        ""0""
	""setting.nowindowborder""        ""1""
	""setting.aspectratiomode""       ""1""
	""setting.mat_vsync""     ""0""
	""setting.mat_triplebuffered""        ""0""
	""setting.mat_monitorgamma""      ""2.200000""
	""setting.mat_queue_mode""        ""-1""
	""setting.mat_motion_blur_enabled""       ""0""
	""setting.gpu_mem_level""     ""2""
	""setting.gpu_level""     ""3""
	""setting.mat_antialias""     ""8""
	""setting.mat_aaquality""     ""0""
	""setting.mat_forceaniso""        ""1""
	""setting.cpu_level""     ""2""
	""setting.videoconfig_version""       ""1""
	""setting.mem_level""     ""2""
	""setting.defaultres""        ""1920""
	""setting.defaultresheight""      ""1080""
	""setting.r_player_visibility_mode""      ""1""
	""setting.mat_enable_uber_shaders""       ""1""
}";
	}
	}
}