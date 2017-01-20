using System;
using System.Collections.Generic;
using HugsLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ZhentarTweaks
{
	static class LetterStackDetour
	{

		static LetterStackDetour()
		{
			try
			{	//Need a wrapper method/lambda to be able to catch the TypeLoadException when HugsLib isn't present
				((Action)(() =>
				{
					var settings = HugsLibController.Instance.Settings.GetModSettings("ZhentarTweaks");
					doYellowLetterHandle = settings.GetHandle("yellowLetterPause", "Pause on yellow letters",
						"When the RimWorld Pause on urgent letters setting is set, also pause on yellow letters?", true);
				}))();
			}
			catch (TypeLoadException)
			{ }
		}

		private static /*HugsLib.Settings.SettingHandle<bool>*/ object doYellowLetterHandle;

		//Calling this when HugsLib isn't loaded will throw an exception
		private static T GetSettingsHandleValue<T>(object handle) => handle as HugsLib.Settings.SettingHandle<T> ?? default(T);

		private static bool DoNonUrgentPause => doYellowLetterHandle == null || GetSettingsHandleValue<bool>(doYellowLetterHandle);

		private static readonly Func<LetterStack, List<Letter>> lettersGet = Utils.GetFieldAccessor<LetterStack, List<Letter>>("letters");
		

		[DetourMember]
		public static void ReceiveLetter(this LetterStack @this, Letter let, string debugText = null)
		{
			var soundDef = let.LetterType == LetterType.BadUrgent ? SoundDefOf.LetterArriveBadUrgent : SoundDefOf.LetterArrive;
			soundDef.PlayOneShotOnCamera();
			if (Prefs.PauseOnUrgentLetter && !Find.TickManager.Paused)
			{
				if (let.LetterType == LetterType.BadUrgent || ( let.LetterType == LetterType.BadNonUrgent && DoNonUrgentPause) )
				{
					Find.TickManager.TogglePaused();
				}
			}
			lettersGet(@this).Add(let);
			let.arrivalTime = Time.time;
		}
	}
}
