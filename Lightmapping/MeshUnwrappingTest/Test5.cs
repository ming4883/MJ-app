using System.Collections.Generic;
using System;

namespace MCD
{
	class Test5 : TestBase
	{
		public class Factory : TestBase.FactoryT<Test5> { }

		public override void Run(List<Mesh> outputs)
		{
			PackInputDialog dlg = new PackInputDialog();
			if (dlg.DoModal())
			{
				Console.WriteLine("map size: {0};  pixel size: {1}", dlg.MapSize, dlg.PixelSizeWithUnits);
			}

			Mesh mesh = MeshUtil.Load("Test5.data.txt");
			GroupedFaceUnwrapper unwrapper = new GroupedFaceUnwrapper();
			unwrapper.debug = true;
			outputs.AddRange(unwrapper.Unwrap(mesh, 512, 1.0f / 0.25f));
		}
	}
}
