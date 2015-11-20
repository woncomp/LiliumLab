using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lilium
{
	public partial class Game
	{
		public static double Time { get; private set; }
		public static double DeltaTime { get; private set; }

		private DateTime startupTime;
		private double lastRealtime;

		void Time_Init()
		{
			startupTime = DateTime.Now;

			Time = 0;
		}
		
		void Time_Update(bool running)
		{
			var timeSpan = DateTime.Now - startupTime;
			var seconds = timeSpan.TotalSeconds;

			DeltaTime = seconds - lastRealtime;
			if(running) Time += DeltaTime;

			lastRealtime = seconds;
		}
	}
}
