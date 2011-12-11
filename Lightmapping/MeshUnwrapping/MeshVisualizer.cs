using System.Drawing;
using OpenTK;

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
					p[j].X = v[j].X * uvSize.Width;
					p[j].Y = (1-v[j].Y) * uvSize.Height - 2;
				}

				g.DrawPolygon(Pens.Blue, p);
			}
		}
	}
}
