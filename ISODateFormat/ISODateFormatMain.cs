using Base.Levels;
using HarmonyLib;
using PhoenixPoint.Common.Game;
using PhoenixPoint.Geoscape.View.ViewModules;
using PhoenixPoint.Modding;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace ISODateFormat
{
	/// <summary>
	/// This is the main mod class. Only one can exist per assembly.
	/// If no ModMain is detected in assembly, then no other classes/callbacks will be called.
	/// </summary>
	public class ISODateFormatMain : ModMain
	{
		/// Config is accessible at any time, if any is declared.
		public new ISODateFormatConfig Config => (ISODateFormatConfig)base.Config;

		/// This property indicates if mod can be Safely Disabled from the game.
		/// Safely sisabled mods can be reenabled again. Unsafely disabled mods will need game restart ot take effect.
		/// Unsafely disabled mods usually cannot revert thier changes in OnModDisabled
		public override bool CanSafelyDisable => false;

		/// <summary>
		/// Callback for when mod is enabled. Called even on game starup.
		/// </summary>
	public override void OnModEnabled()
	{
		Harmony harmony = (Harmony)HarmonyInstance;

		var original = AccessTools.Method(typeof(UIModuleTimeControl), "Update");
		var transpiler = AccessTools.Method(
			typeof(UIModuleTimeControl_Update_Patch),
			"Transpiler"
		);

		harmony.Patch(
			original,
			transpiler: new HarmonyMethod(transpiler)
		);

		Logger.LogInfo("ISO Date Format enabled.");
	}


		/// <summary>
		/// Callback for when mod is disabled. This will be called even if mod cannot be safely disabled.
		/// Guaranteed to have OnModEnabled before.
		/// </summary>
		public override void OnModDisabled() {
			/// Undo any game modifications if possible. Else "CanSafelyDisable" must be set to false.
			/// ModGO will be destroyed after OnModDisabled.
		}

		/// <summary>
		/// Callback for when any property from mod's config is changed.
		/// </summary>
		public override void OnConfigChanged() {
			/// Config is accessible at any time.
		}


		/// <summary>
		/// In Phoenix Point there can be only one active level at a time. 
		/// Levels go through different states (loading, unloaded, start, etc.).
		/// General puprose level state change callback.
		/// </summary>
		/// <param name="level">Level being changed.</param>
		/// <param name="prevState">Old state of the level.</param>
		/// <param name="state">New state of the level.</param>
		public override void OnLevelStateChanged(Level level, Level.State prevState, Level.State state) {
			/// Alternative way to access current level at any time.
			Level l = GetLevel();
		}

		/// <summary>
		/// Useful callback for when level is loaded, ready, and starts.
		/// Usually game setup is executed.
		/// </summary>
		/// <param name="level">Level that starts.</param>
		public override void OnLevelStart(Level level) {
		}

		/// <summary>
		/// Useful callback for when level is ending, before unloading.
		/// Usually game cleanup is executed.
		/// </summary>
		/// <param name="level">Level that ends.</param>
		public override void OnLevelEnd(Level level) {
		}
	}
	[HarmonyPatch(typeof(UIModuleTimeControl), "Update")]
	public static class UIModuleTimeControl_Update_Patch
	{
		public static IEnumerable<CodeInstruction> Transpiler(
			IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldstr &&
					instruction.operand as string == "dd.MM.yyyy")
				{
					instruction.operand = "yyyy-MM-dd";
				}

				yield return instruction;
			}
		}
	}
}
