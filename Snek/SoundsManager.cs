using System;
using System.Collections.Generic;
using System.Media;
using System.Text;

namespace Snek
{
	public static class SoundsManager
	{
		public static void PlayPointSound()
		{
			SoundPlayer pointSound = new SoundPlayer(SoundsResource.point);
			pointSound.PlaySync();
		}

		public static void PlayGameOverSound()
		{
			SoundPlayer gameOverSound = new SoundPlayer(SoundsResource.gameOver);
			gameOverSound.PlaySync();
		}
	}
}
