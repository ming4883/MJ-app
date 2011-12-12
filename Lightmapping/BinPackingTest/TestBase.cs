using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MCD
{
	abstract class TestBase
	{
		public abstract void Run(List<Bitmap> bmps);

		public abstract class FactoryBase
		{
			public abstract TestBase Create();
		}

		public class FactoryT<T> : FactoryBase where T : TestBase, new()
		{
			public override TestBase Create() { return new T(); }
			public override string ToString() { return typeof(T).Name; }
		}

		protected PackSettings settings = new PackSettings();
		protected List<PackOutputList> outputs = new List<PackOutputList>();
		protected List<PackInput> inputs = new List<PackInput>();

		protected void Input(Size sz, string userdata)
		{
			PackInput i = new PackInput();
			i.Size = sz;
			i.Userdata = userdata;

			inputs.Add(i);
		}

		protected void OutputsToBitmaps(List<Bitmap> bmps)
		{
			using(Font fnt = new Font("Small Fonts", 6.0f))
			using (StringFormat strfmt = new StringFormat())
			{
				strfmt.Alignment = StringAlignment.Center;
				strfmt.LineAlignment = StringAlignment.Center;

				foreach (PackOutputList polist in outputs)
				{
					Bitmap bmp = new Bitmap(settings.Size.Width+1, settings.Size.Height+1);

					using (Graphics g = Graphics.FromImage(bmp))
					{
						g.Clear(Color.White);

						foreach (PackOutput po in polist)
						{
							Rectangle r = new Rectangle(new Point(po.X, po.Y), inputs[po.Input].Size);

							if(po.Rotated)
							{
								r.Width = inputs[po.Input].Size.Height;
								r.Height = inputs[po.Input].Size.Width;
							}

							g.DrawRectangle(Pens.Blue, r);
							g.DrawString(
								string.Format("{0}\n{1},{2}\n{3}x{4}", po.Input, r.X, r.Y, r.Width, r.Height),
								fnt, po.Rotated ? Brushes.Red : Brushes.Blue, r, strfmt);
						}
					}

					bmps.Add(bmp);
				}
			}
		}
	}
}
