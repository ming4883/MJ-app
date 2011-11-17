using System.Collections.Generic;
using System.Drawing;

namespace MCD
{
	class Test5 : TestBase
	{
		public class Factory : TestBase.FactoryBase
		{
			public override TestBase Create()
			{
				return new Test5();
			}

			public override string ToString()
			{
				return typeof(Test5).Name;
			}
		}

		public override void Run(List<Bitmap> bmps)
		{
			// test with randomly generated medium sized inputs
			settings.Size = new Size(512, 512);
			settings.Border = 1;

			System.Random rand = new System.Random(1357);

			for (int i = 0; i < 2048; ++i)
				Input(new Size(rand.Next(8, 64), rand.Next(8, 64)), "");

			JimScottPacker packer = new JimScottPacker();
			packer.Pack(settings, inputs, outputs);

			OutputsToBitmaps(bmps);
		}
	}
}
