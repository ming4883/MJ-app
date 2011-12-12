using System;
using System.Collections.Generic;
using OpenTK;
using System.Drawing;

namespace MCD
{
	public class GroupedFaceUnwrapper : PreFaceUnwrapper
	{
		protected class GroupedFaceUV : FaceUV
		{
			public struct VtxHash
			{
				public int X, Y, Z;

				public override bool  Equals(object obj)
				{
					VtxHash b = (VtxHash)obj;
					return X == b.X && Y == b.Y && Z == b.Z;
				}
			}

			public int GroupId = -1;
			public Vector3 Normal;
			VtxHash[] vtxHash = new VtxHash[3];

			public void ComputeVtxHash(Vector3 p0, Vector3 p1, Vector3 p2, float scale)
			{
				vtxHash[0].X = (int)Math.Ceiling(p0.X * scale);
				vtxHash[0].Y = (int)Math.Ceiling(p0.Y * scale);
				vtxHash[0].Z = (int)Math.Ceiling(p0.Z * scale);

				vtxHash[1].X = (int)Math.Ceiling(p1.X * scale);
				vtxHash[1].Y = (int)Math.Ceiling(p1.Y * scale);
				vtxHash[1].Z = (int)Math.Ceiling(p1.Z * scale);

				vtxHash[2].X = (int)Math.Ceiling(p2.X * scale);
				vtxHash[2].Y = (int)Math.Ceiling(p2.Y * scale);
				vtxHash[2].Z = (int)Math.Ceiling(p2.Z * scale);
			}

			static float threshold = (float)Math.Cos(5.0);

			public static bool Connected(GroupedFaceUV a, GroupedFaceUV b)
			{
				if (Vector3.Dot(a.Normal, b.Normal) < threshold) // non-coplanar faces
					return false;

				int commonCnt = 0;

				for (int i = 0; i < 3; ++i)
				{
					for (int j = 0; j < 3; ++j)
					{
						if (a.vtxHash[i].Equals(b.vtxHash[j]))
							++commonCnt;
					}
				}

				return commonCnt > 1;
			}

			public static bool Bounding(out Vector2 min, out Vector2 max, List<GroupedFaceUV> faceuvs, int groupid)
			{
				bool inited = false;
				min = max = Vector2.Zero;

				foreach (GroupedFaceUV fuv in faceuvs)
				{
					if (fuv.GroupId != groupid)
						continue;

					Vector2 mi, ma;
					fuv.Bounding(out mi, out ma);

					if (!inited)
					{
						min = mi; max = ma;
						inited = true;
					}
					else
					{
						min.X = Math.Min(min.X, mi.X);
						min.Y = Math.Min(min.Y, mi.Y);

						max.X = Math.Max(max.X, ma.X);
						max.Y = Math.Max(max.Y, ma.Y);
					}
				}

				return inited;
			}

			public static void Dump(List<GroupedFaceUV> faceuvs, int groupid)
			{
				bool first = true;
				for(int i=0; i<faceuvs.Count; ++i)
				{
					if (faceuvs[i].GroupId == groupid)
					{
						Console.Write(first ? "{0}" : ", {0}", i);
						first = false;
					}
				}

				Console.Write("\n");
			}
		}

		public override Mesh Unwrap(Mesh mesh, int packSize, float worldScale)
		{
			List<GroupedFaceUV> faceuvs = new List<GroupedFaceUV>();

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

				GroupedFaceUV fuv = new GroupedFaceUV();
				fuv.Texcrd[0] = tc(p0, worldScale);
				fuv.Texcrd[1] = tc(p1, worldScale);
				fuv.Texcrd[2] = tc(p2, worldScale);
				fuv.Normal = n;
				fuv.ComputeVtxHash(p0, p1, p2, 1000);

				//Console.WriteLine("{0}, {1}, {2}", fuv.Texcrd[0], fuv.Texcrd[1], fuv.Texcrd[2]);

				faceuvs.Add(fuv);
			}

			// group the faces
			int gCnt = 0;
			
			for (int i = 0; i < fcnt; ++i)
			{
				GroupedFaceUV src = faceuvs[i];

				// create a new group if needed
				if (-1 == src.GroupId)
					src.GroupId = gCnt++;

				for (int j = 0; j < fcnt; ++j)
				{
					if (i == j) continue; // skip self

					GroupedFaceUV dst = faceuvs[j];
					if (-1 != dst.GroupId) continue; // already added to group

					if (GroupedFaceUV.Connected(src, dst))
						dst.GroupId = src.GroupId;
				}
			}

			Console.WriteLine("{0} faces created {1} groups", fcnt, gCnt);

			// packing
			PackSettings packSettings = new PackSettings();
			List<PackOutputList> packOutputs = new List<PackOutputList>();
			List<PackInput> packInputs = new List<PackInput>();

			for (int i = 0; i < gCnt; ++i)
			{
				Vector2 min, max;
				GroupedFaceUV.Bounding(out min, out max, faceuvs, i);

				PackInput pi = new PackInput();
				pi.Size.Width = (int)Math.Ceiling(max.X - min.X);
				pi.Size.Height = (int)Math.Ceiling(max.Y - min.Y);
				packInputs.Add(pi);

				Console.Write("gp{0}: ", i);
				GroupedFaceUV.Dump(faceuvs, i);
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
					Vector2 min, max;
					GroupedFaceUV.Bounding(out min, out max, faceuvs, po.Input);

					foreach (GroupedFaceUV fuv in faceuvs)
					{
						if (fuv.GroupId != po.Input) continue;

						fuv.Translate(new Vector2(po.X, po.Y) - min);
						fuv.Scale(scale);
					}
				}
			}

			// create output mesh
			Mesh output = new Mesh();
			output.Init(mesh.Indices.Count);

			for (int i = 0; i < fcnt; ++i)
			{
				mesh.Positions.CopyFaceTo(output.Positions, i);
				mesh.Normals.CopyFaceTo(output.Normals, i);
				mesh.Texcrds0.CopyFaceTo(output.Texcrds0, i);

				GroupedFaceUV fuv = faceuvs[i];
				output.Texcrds1.SetFace(i, fuv.Texcrd[0], fuv.Texcrd[1], fuv.Texcrd[2]);
			}

			return output;
		}
	}
}
