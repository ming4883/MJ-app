using System.Collections.Generic;
using System.Drawing;

namespace MCD
{
	class Test1 : TestBase
	{
		public class Factory : TestBase.FactoryBase
		{
			public override TestBase Create()
			{
				return new Test1();
			}

			public override string ToString()
			{
				return typeof(Test1).Name;
			}
		}

		public override void Run(List<Bitmap> bmps)
		{
			// simple test case
			settings.Size = new Size(512, 512);
			settings.Border = 1;

			Input(new Size(64, 64), "");
			Input(new Size(64, 64), "");
			Input(new Size(32, 32), "");
			Input(new Size(32, 32), "");
			Input(new Size(16, 16), "");
			Input(new Size(16, 16), "");

			JimScottPacker packer = new JimScottPacker();
			packer.Pack(settings, inputs, outputs);

			OutputsToBitmaps(bmps);
		}
	}
}
