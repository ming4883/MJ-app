using System;
using System.Collections.Generic;
using OpenTK;

namespace MCD
{
	public class PreFaceUnwrapper : IMeshUnwrapper
	{
		protected delegate Vector2 TC(Vector3 p, float scale);

		protected static TC X_MAJOR = delegate(Vector3 p, float scale) { return new Vector2(p.Z * scale, p.Y * scale); };
		protected static TC Y_MAJOR = delegate(Vector3 p, float scale) { return new Vector2(p.X * scale, p.Z * scale); };
		protected static TC Z_MAJOR = delegate(Vector3 p, float scale) { return new Vector2(p.X * scale, p.Y * scale); };

		protected class FaceUV
		{
			public Vector2[] Texcrd = new Vector2[3];
			
			public void Translate(Vector2 offset)
			{
				for (int i = 0; i < Texcrd.Length; ++i)
					Texcrd[i] += offset;
			}

			public void Scale(Vector2 scale)
			{
				for (int i = 0; i < Texcrd.Length; ++i)
				{
					Texcrd[i].X *= scale.X;
					Texcrd[i].Y *= scale.Y;
				}
			}

			public void Bounding(out Vector2 min, out Vector2 max)
			{
				min = Texcrd[0];
				max = Texcrd[0];

				for (int i = 1; i < Texcrd.Length; ++i)
				{
					min.X = Math.Min(min.X, Texcrd[i].X);
					min.Y = Math.Min(min.Y, Texcrd[i].Y);
					max.X = Math.Max(max.X, Texcrd[i].X);
					max.Y = Math.Max(max.Y, Texcrd[i].Y);
				}
			}

			public void ZeroOffset()
			{
				Vector2 min, max;
				Bounding(out min, out max);
				Translate(-min);
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

		public virtual List<Mesh> Unwrap(Mesh mesh, int packSize, float worldScale)
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

				// get major axis & assign texcoord
				TC tc = GetMajorAxis(ref n);

				FaceUV faceuv = new FaceUV();
				faceuv.Texcrd[0] = tc(p0, worldScale);
				faceuv.Texcrd[1] = tc(p1, worldScale);
				faceuv.Texcrd[2] = tc(p2, worldScale);

				//Console.WriteLine("{0}, {1}, {2}", faceuv.Texcrd[0], faceuv.Texcrd[1], faceuv.Texcrd[2]);

				faceuvs.Add(faceuv);
			}

			// packing
			PackSettings packSettings = new PackSettings();
			List<PackOutputList> packOutputs = new List<PackOutputList>();
			List<PackInput> packInputs = new List<PackInput>();

			for (int i = 0; i < fcnt; ++i)
			{
				FaceUV fuv = faceuvs[i];
				Vector2 min, max;
				fuv.Bounding(out min, out max);

				PackInput pi = new PackInput();
				pi.Size.Width = (int)Math.Ceiling(max.X - min.X);
				pi.Size.Height = (int)Math.Ceiling(max.Y - min.Y);
				pi.Userdata = fuv;
				packInputs.Add(pi);
			}

			packSettings.Size.Width = packSize;
			packSettings.Size.Height = packSize;
			packSettings.Border = 1;
			Vector2 scale = new Vector2(1.0f / packSize, 1.0f / packSize);

			JimScottPacker packer = new JimScottPacker();
			packer.Pack(packSettings, packInputs, packOutputs);

			// convert pack outputs back to faceuv
			foreach (PackOutputList polist in packOutputs)
			{
				foreach (PackOutput po in polist)
				{
					FaceUV fuv = packInputs[po.Input].Userdata as FaceUV;
					fuv.ZeroOffset();
					fuv.Translate(new Vector2(po.X, po.Y));
					fuv.Scale(scale);
				}
			}

			// create output mesh
			List<Mesh> output = new List<Mesh>();

			foreach (PackOutputList polist in packOutputs)
			{
				Mesh omesh = new Mesh();
				omesh.Init(polist.Count * 3);

				for (int dst = 0; dst < polist.Count; ++dst)
				{
					int src = polist[dst].Input;

					mesh.Positions.CopyFaceTo(omesh.Positions, src, dst);
					mesh.Normals.CopyFaceTo(omesh.Normals, src, dst);
					mesh.Texcrds0.CopyFaceTo(omesh.Texcrds0, src, dst);

					FaceUV fuv = faceuvs[src];
					omesh.Texcrds1.SetFace(dst, fuv.Texcrd[0], fuv.Texcrd[1], fuv.Texcrd[2]);
				}

				output.Add(omesh);
			}

			return output;
		}
	}
}
