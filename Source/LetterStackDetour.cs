using System;
using System.Collections.Generic;
using HugsLib;
using HugsLib.Settings;
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
					//handle can't be saved as a SettingHandle<> type; otherwise the compiler generated closure class will throw a typeloadexception
					object handle = settings.GetHandle("yellowLetterPause", "Pause on yellow letters",
						"When the RimWorld Pause on urgent letters setting is set, also pause on yellow letters?", true);
					DoNonUrgentPause = () => (SettingHandle<bool>)handle;
				}))();
				return;
			}
			catch (TypeLoadException)
			{ }
			DoNonUrgentPause = () => true;
		}

		private static Func<bool> DoNonUrgentPause;

		private static readonly Func<LetterStack, List<Letter>> lettersGet = Utils.GetFieldAccessor<LetterStack, List<Letter>>("letters");
		

		[DetourMember]
		public static void ReceiveLetter(this LetterStack @this, Letter let, string debugText = null)
		{
			var soundDef = let.LetterType == LetterType.BadUrgent ? SoundDefOf.LetterArriveBadUrgent : SoundDefOf.LetterArrive;
			soundDef.PlayOneShotOnCamera();
			if (Prefs.PauseOnUrgentLetter && !Find.TickManager.Paused)
			{
				if (let.LetterType == LetterType.BadUrgent || (let.LetterType == LetterType.BadNonUrgent && DoNonUrgentPause()) )
				{
					Find.TickManager.TogglePaused();
				}
			}
			lettersGet(@this).Add(let);
			let.arrivalTime = Time.time;
		}
	}
}
