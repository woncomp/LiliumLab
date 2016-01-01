using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiliumLab
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Lilium.Config.DebugMode = true;
			Lilium.Game.Run<SecondMetaballGame>();
		}
	}
}
