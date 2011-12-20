using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenTK;

namespace MCD
{
	public static class MeshUtil
	{
		public static void DrawTexcrd1(Mesh mesh, Graphics g, Size uvSize)
		{
			g.Clear(Color.White);

			if (null == mesh) return;

			PointF[] p = new PointF[3];
			Vector2[] v = new Vector2[3];
			PointF c = PointF.Empty;

			int fcnt = mesh.FaceCount;

			Font fnt = new Font("Small Fonts", 7.0f, FontStyle.Bold);

			for (int i = 0; i < fcnt; ++i)
			{
				mesh.Texcrds1.GetFace(out v[0], out v[1], out v[2], i);

				c = PointF.Empty;
				for (int j = 0; j < 3; ++j)
				{
					p[j].X = v[j].X * uvSize.Width - 1;
					p[j].Y = (1-v[j].Y) * uvSize.Height - 1;

					c.X += p[j].X;
					c.Y += p[j].Y;
				}

				g.DrawPolygon(Pens.Blue, p);

				c.X /= 3.0f;
				c.Y /= 3.0f;
				g.DrawString(mesh.FaceProps[i].ToString(), fnt, Brushes.Red, c);
			}
			fnt.Dispose();
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

		private static void Save(StreamWriter sw, string name, List<int> list)
		{
			sw.Write("{0}\n", name);
			for (int i = 0; i < list.Count; ++i)
				sw.Write("{0}\n", list[i]);
		}

		private static void Save(StreamWriter sw, string name, List<Vector2> list)
		{
			sw.Write("{0}\n", name);
			for (int i = 0; i < list.Count; ++i)
				sw.Write("{0} {1}\n", list[i].X, list[i].Y);
		}

		private static void Save(StreamWriter sw, string name, List<Vector3> list)
		{
			sw.Write("{0}\n", name);
			for (int i = 0; i < list.Count; ++i)
				sw.Write("{0} {1} {2}\n", list[i].X, list[i].Y, list[i].Z);
		}

		public static void Save(Mesh mesh, string path)
		{
			try
			{
				using (StreamWriter sw = new StreamWriter(path))
				{
					sw.Write("{0} {1}\n", mesh.VertexCount, mesh.IndexCount);

					if (mesh.Indexed)
						Save(sw, "index", mesh.Indices);

					Save(sw, "faceprop", mesh.FaceProps);
					Save(sw, "position", mesh.Positions.Raw);
					Save(sw, "normal", mesh.Normals.Raw);
					Save(sw, "texcrd0", mesh.Texcrds0.Raw);
					Save(sw, "texcrd1", mesh.Texcrds1.Raw);
				}
			}
			catch (Exception)
			{
			}
		}

		private static string[] Split(string str)
		{
			return str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}

		private static string Load(StreamReader sr, List<int> list)
		{
			string line = sr.ReadLine();
			int val;

			list.Clear();

			while (null != line)
			{
				if (!int.TryParse(line, out val)) break;

				list.Add(val);
				line = sr.ReadLine();
			}

			return line;
		}

		private static string Load(StreamReader sr, List<Vector2> list)
		{
			string line = sr.ReadLine();
			Vector2 val;

			list.Clear();

			while (null != line)
			{
				string[] t = Split(line);
				if (t.Length != 2) break;
				if (!float.TryParse(t[0], out val.X)) break;
				if (!float.TryParse(t[1], out val.Y)) break;

				list.Add(val);
				line = sr.ReadLine();
			}

			return line;
		}

		private static string Load(StreamReader sr, List<Vector3> list)
		{
			string line = sr.ReadLine();
			Vector3 val;

			list.Clear();

			while (null != line)
			{
				string[] t = Split(line);
				if (t.Length != 3) break;
				if (!float.TryParse(t[0], out val.X)) break;
				if (!float.TryParse(t[1], out val.Y)) break;
				if (!float.TryParse(t[2], out val.Z)) break;

				list.Add(val);
				line = sr.ReadLine();
			}

			return line;
		}

		public static Mesh Load(string path)
		{
			try
			{
				string line;
				using (StreamReader sr = new StreamReader(path))
				{
					line = sr.ReadLine();
					int vcnt, icnt;
					{
						string[] t = Split(line);
						if (t.Length != 2) throw new Exception("failed to read vcnt & icnt");
						if (!int.TryParse(t[0], out vcnt)) throw new Exception("failed to read vcnt");
						if (!int.TryParse(t[1], out icnt)) throw new Exception("failed to read icnt");
					}

					Mesh mesh = new Mesh();
					if (icnt > 0)
						mesh.Init(icnt, vcnt);
					else
						mesh.Init(vcnt);

					line = sr.ReadLine();

					do
					{
						if (string.Compare(line, "index") == 0)
							line = Load(sr, mesh.Indices);

						else if (string.Compare(line, "faceprop") == 0)
							line = Load(sr, mesh.FaceProps);

						else if (string.Compare(line, "position") == 0)
							line = Load(sr, mesh.Positions.Raw);

						else if (string.Compare(line, "normal") == 0)
							line = Load(sr, mesh.Normals.Raw);

						else if (string.Compare(line, "texcrd0") == 0)
							line = Load(sr, mesh.Texcrds0.Raw);

						else if (string.Compare(line, "texcrd1") == 0)
							line = Load(sr, mesh.Texcrds1.Raw);

						else
							throw new Exception(string.Format("failed to read {0}", line));

					} while (null != line);

					return mesh;
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
	}
}
