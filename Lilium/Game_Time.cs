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
		public static ulong FrameCount { get; private set; }

		public static double FrameTime { get; private set; }

		private DateTime startupTime;
		private double lastRealtime;

		private double _beginTime2;
		private double _beginTime;
		private double _frameCount2;
		private double _frameCount;
		private double _frameTime;


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

			// Update Frame Time
			++FrameCount;
			++_frameCount;
			var t = Time;
			if (_beginTime + 1 <= t)
			{
				_frameTime = (t - _beginTime2) / (_frameCount + _frameCount2);
				_beginTime2 = _beginTime;
				_frameCount2 = _frameCount;
				_beginTime = t;
				_frameCount = 0;
				FrameTime = _frameTime;
			}

			lastRealtime = seconds;
		}
	}
}
