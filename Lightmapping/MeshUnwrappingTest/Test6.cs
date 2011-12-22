using System.Collections.Generic;
using System;

namespace MCD
{
	class Test6 : TestBase
	{
		public class Factory : TestBase.FactoryT<Test6> { }

		public override void Run(List<Mesh> outputs)
		{
			LightmapPackDialog dlg = new LightmapPackDialog();
			dlg.Pack += delegate(object s, EventArgs e)
			{
				Console.WriteLine("{0}; map size: {1};  pixel size: {2}; border size: {3}", dlg.OutputName, dlg.MapSize, dlg.PixelSizeWithUnits, dlg.BorderSize);
			};
			dlg.Show();
		}
	}
}
