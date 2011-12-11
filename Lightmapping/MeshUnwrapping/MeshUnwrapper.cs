using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace MCD
{
	public interface IMeshUnwrapper
	{
		Mesh Unwrap(Mesh mesh, float scale);
	}

	public class PreFaceUnwrapper : IMeshUnwrapper
	{
		protected delegate Vector2 TC(Vector3 p, float scale);

		protected static TC X_MAJOR = delegate(Vector3 p, float scale) { return new Vector2(p.Z * scale, p.Y * scale); };
		protected static TC Y_MAJOR = delegate(Vector3 p, float scale) { return new Vector2(p.X * scale, p.Z * scale); };
		protected static TC Z_MAJOR = delegate(Vector3 p, float scale) { return new Vector2(p.X * scale, p.Y * scale); };

		protected class FaceUV
		{
			public Vector2[] Texcrd = new Vector2[3];

			public void ZeroOffset()
			{
				Vector2 min = Texcrd[0], max = Texcrd[0];

				for (int i = 1; i < Texcrd.Length; ++i)
				{
					min = Vector2.Min(min, Texcrd[i]);
					max = Vector2.Max(max, Texcrd[i]);
				}

				for (int i = 0; i < Texcrd.Length; ++i)
					Texcrd[i] -= min;
			}
		}

		protected static TC GetMajorAxis(ref Vector3 n)
		{
			float absx = Math.Abs(n.X);
			float absy = Math.Abs(n.Y);
			float absz = Math.Abs(n.Z);

			if (absx > absy)
			{
				if (absx > absz)
					return X_MAJOR;
				else
					return Z_MAJOR;
			}
			else
			{
				if (absy > absz)
					return Y_MAJOR;
				else
					return Z_MAJOR;
			}
		}

		public Mesh Unwrap(Mesh mesh, float scale)
		{
			List<FaceUV> faceuvs = new List<FaceUV>();

			int fcnt = mesh.FaceCount;
			for (int i = 0; i < fcnt; ++i)
			{
				Vector3 p0, p1, p2, e1, e2;
				
				// computer the face normal
				mesh.Positions.GetFace(out p0, out p1, out p2, i);

				e1 = p1 - p0; e1.Normalize();
				e2 = p2 - p0; e2.Normalize();

				Vector3 n = Vector3.Cross(e1, e2);
				n.Normalize();
				Console.Write("n = {0}\n", n);

				// get major axis & assign texcoord
				TC tc = GetMajorAxis(ref n);

				FaceUV faceuv = new FaceUV();
				faceuv.Texcrd[0] = tc(p0, scale);
				faceuv.Texcrd[1] = tc(p1, scale);
				faceuv.Texcrd[2] = tc(p2, scale);
				faceuv.ZeroOffset();

				faceuvs.Add(faceuv);
			}

			Mesh output = new Mesh();
			output.Init(mesh.Indices.Count);

			for (int i = 0; i < fcnt; ++i)
			{
				Vector3 p0, p1, p2;
				mesh.Positions.GetFace(out p0, out p1, out p2, i);
				output.Positions.SetFace(i, p0, p1, p2);

				Vector3 n0, n1, n2;
				mesh.Normals.GetFace(out n0, out n1, out n2, i);
				output.Normals.SetFace(i, n0, n1, n2);

				Vector2 tc0, tc1, tc2;
				mesh.Texcrds0.GetFace(out tc0, out tc1, out tc2, i);
				output.Texcrds0.SetFace(i, tc0, tc1, tc2);

				FaceUV fuv = faceuvs[i];
				output.Texcrds1.SetFace(i, fuv.Texcrd[0], fuv.Texcrd[1], fuv.Texcrd[2]);
			}

			return output;
		}
	}
}
