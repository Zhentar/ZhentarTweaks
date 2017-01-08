using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ZhentarTweaks
{
	static class LetterStackDetour
	{

		private static readonly Func<LetterStack, List<Letter>> lettersGet = Utils.GetFieldAccessor<LetterStack, List<Letter>>("letters");

		[DetourMember]
		public static void ReceiveLetter(this LetterStack @this, Letter let, string debugText = null)
		{
			SoundDef soundDef;
			soundDef = let.LetterType == LetterType.BadUrgent ? SoundDefOf.LetterArriveBadUrgent : SoundDefOf.LetterArrive;
			soundDef.PlayOneShotOnCamera();
			if ( let.LetterType != LetterType.Good && Prefs.PauseOnUrgentLetter && !Find.TickManager.Paused)
			{
				Find.TickManager.TogglePaused();
			}
			lettersGet(@this).Add(let);
			let.arrivalTime = Time.time;
		}
	}
}
