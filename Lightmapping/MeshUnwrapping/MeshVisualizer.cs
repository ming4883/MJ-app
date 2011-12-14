using System.Drawing;
using OpenTK;
using System.Collections.Generic;

namespace MCD
{
	public static class MeshVisualizer
	{
		public static void DrawTexcrd1(Mesh mesh, Graphics g, Size uvSize)
		{
			g.Clear(Color.White);

			if (null == mesh) return;

			PointF[] p = new PointF[3];
			Vector2[] v = new Vector2[3];

			int fcnt = mesh.FaceCount;

			for (int i = 0; i < fcnt; ++i)
			{
				mesh.Texcrds1.GetFace(out v[0], out v[1], out v[2], i);

				for (int j = 0; j < 3; ++j)
				{
					p[j].X = v[j].X * uvSize.Width - 1;
					p[j].Y = (1-v[j].Y) * uvSize.Height - 1;
				}

				g.DrawPolygon(Pens.Blue, p);
			}
		}

		public static void DrawTexcrd1(List<Mesh> meshes, string path, int width, int height)
		{
			int i = 0;
			string ext = System.IO.Path.GetExtension(path);

			foreach (Mesh m in meshes)
			{
				using (Bitmap b = new Bitmap(width, height))
				using (Graphics g = Graphics.FromImage(b))
				{
					string filename = System.IO.Path.ChangeExtension(path, string.Format("{0}{1}", ++i, ext));
					DrawTexcrd1(m, g, new Size(width, height));
					b.Save(filename);
				}
			}
		}
	}
}
