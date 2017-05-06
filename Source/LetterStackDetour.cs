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
		private class TweaksMod : Mod
		{
			public TweaksMod(ModContentPack content) : base(content)
			{
				LetterStackDetour.settings = GetSettings<TweaksSettings>();
			}

			public override string SettingsCategory() => "Zhentar's Vanilla Tweaks";

			public override void DoSettingsWindowContents(Rect inRect)
			{
				Listing_Standard listing_Standard = new Listing_Standard();
				listing_Standard.ColumnWidth = inRect.width / 3;
				listing_Standard.Begin(inRect);

				listing_Standard.CheckboxLabeled("Pause on yellow letters", ref settings.DoNonUrgentPause, "When the RimWorld Pause on urgent letters setting is set, also pause on yellow letters?");
				listing_Standard.End();
			}
		}
		
		private class TweaksSettings : ModSettings
		{
			public bool DoNonUrgentPause = false;

			public override void ExposeData()
			{
				Scribe_Values.Look(ref DoNonUrgentPause, "doNonUrgentPause");
			}
		}

		private static TweaksSettings settings;

		private static readonly Func<LetterStack, List<Letter>> lettersGet = Utils.GetFieldAccessor<LetterStack, List<Letter>>("letters");
		
		[DetourMember]
		public static void ReceiveLetter(this LetterStack @this, Letter let, string debugInfo = null)
		{
			let.def.arriveSound.PlayOneShotOnCamera();
			if (Prefs.PauseOnUrgentLetter && !Find.TickManager.Paused)
			{
				if (let.def == LetterDefOf.BadUrgent || (let.def == LetterDefOf.BadNonUrgent && settings.DoNonUrgentPause) )
				{
					Find.TickManager.TogglePaused();
				}
			}
			lettersGet(@this).Add(let);
			let.arrivalTime = Time.time;
			let.debugInfo = debugInfo;
			let.Received();
		}
	}
}
