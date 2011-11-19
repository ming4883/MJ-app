using System.Collections.Generic;
using System.Drawing;

namespace MCD
{
	class Test4 : TestBase
	{
		public class Factory : TestBase.FactoryBase
		{
			public override TestBase Create()
			{
				return new Test4();
			}

			public override string ToString()
			{
				return typeof(Test4).Name;
			}
		}

		public override void Run(List<Bitmap> bmps)
		{
			// test with randomly generated huge & tiny inputs, with AllowRotate = true
			settings.Size = new Size(512, 512);
			settings.Border = 1;

			System.Random rand = new System.Random(1357);

			// add some huge inputs
			for (int i = 0; i < 64; ++i)
				Input(new Size(rand.Next(16, 256), rand.Next(16, 256)), "");

			// add some tiny inputs
			for (int i = 0; i < 64; ++i)
				Input(new Size(rand.Next(8, 16), rand.Next(8, 16)), "");

			JimScottPacker packer = new JimScottPacker();
			packer.AllowRotate = true;
			packer.Pack(settings, inputs, outputs);

			OutputsToBitmaps(bmps);
		}
	}
}
