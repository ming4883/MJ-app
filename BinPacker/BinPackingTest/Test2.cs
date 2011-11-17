using System.Collections.Generic;
using System.Drawing;

namespace MCD
{
	class Test2 : TestBase
	{
		public class Factory : TestBase.FactoryBase
		{
			public override TestBase Create()
			{
				return new Test2();
			}

			public override string ToString()
			{
				return typeof(Test2).Name;
			}
		}

		public override void Run(List<Bitmap> bmps)
		{
			// test with input w+1, w, w-1, w-2
			settings.Size = new Size(512, 512);
			settings.Border = 1;

			Input(new Size(513, 513), "");
			Input(new Size(512, 512), "");
			Input(new Size(511, 511), "");
			Input(new Size(510, 510), "");
			Input(new Size(64, 64), "");
			Input(new Size(32, 32), "");

			JimScottPacker packer = new JimScottPacker();
			packer.Pack(settings, inputs, outputs);

			OutputsToBitmaps(bmps);
		}
	}
}
