using System;
using System.Collections.Generic;
using OpenTK;
using System.Drawing;

namespace MCD
{
	public class GroupedFaceUnwrapper : PreFaceUnwrapper
	{
		public bool debug = false;
		public float Precision = 1000;
		
		protected class GroupedFaceUV : FaceUV
		{
			public struct VtxHash
			{
				public int X, Y, Z;

				public override int GetHashCode() { return base.GetHashCode(); }

				public override bool Equals(object obj)
				{
					VtxHash b = (VtxHash)obj;
					return (X == b.X && Y == b.Y && Z == b.Z);
				}

				public override string ToString()
				{
					return string.Format("[{0}, {1}, {2}]", X, Y, Z);
				}
			}

			public int GroupId = -1;
			public int ConnectId = -1;
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

			static float DOT_THRESHOLD = (float)Math.Cos(5.0 / 180.0 * Math.PI);
			static int[] e0LUT = new int[] { 0, 1, 2 };
			static int[] e1LUT = new int[] { 1, 2, 0 };

			public static bool Connected(GroupedFaceUV a, GroupedFaceUV b)
			{
				float dot = Vector3.Dot(a.Normal, b.Normal);
				if (dot < DOT_THRESHOLD) // non-coplanar faces
					return false;

				int commonEdgeCnt = 0;

				for (int i = 0; i < 3; ++i)
				{
					VtxHash ae0 = a.vtxHash[e0LUT[i]];
					VtxHash ae1 = a.vtxHash[e1LUT[i]];

					for (int j = 0; j < 3; ++j)
					{
						VtxHash be0 = b.vtxHash[e0LUT[j]];
						VtxHash be1 = b.vtxHash[e1LUT[j]];

						if(	(ae0.Equals(be0) && ae1.Equals(be1))
						||	(ae0.Equals(be1) && ae1.Equals(be0)))
							++commonEdgeCnt;
					}
				}

				return commonEdgeCnt > 0;
			}

			public static int Bounding(out Vector2 min, out Vector2 max, List<GroupedFaceUV> faceuvs, int groupid)
			{
				int cnt = 0;
				min = max = Vector2.Zero;

				foreach (GroupedFaceUV fuv in faceuvs)
				{
					if (fuv.GroupId != groupid)
						continue;

					Vector2 mi, ma;
					fuv.Bounding(out mi, out ma);

					if (0 == cnt)
					{
						min = mi; max = ma;
					}
					else
					{
						min.X = Math.Min(min.X, mi.X);
						min.Y = Math.Min(min.Y, mi.Y);

						max.X = Math.Max(max.X, ma.X);
						max.Y = Math.Max(max.Y, ma.Y);
					}
					++cnt;
				}

				return cnt;
			}

			public static void Dump(List<GroupedFaceUV> faceuvs, int groupid)
			{
				bool first = true;
				for(int i=0; i<faceuvs.Count; ++i)
				{
					if (faceuvs[i].GroupId == groupid)
					{
						string fmt = first ? "{0}=>{1}" : ", {0}=>{1}";
						Console.Write(fmt, i, faceuvs[i].ConnectId);
						first = false;
					}
				}

				Console.Write("\n");
			}
		}

		protected struct Group
		{
			public Vector2 Min, Max;
			public int Count;
		}

		public override List<Mesh> Unwrap(Mesh mesh, int packSize, float worldScale)
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
				fuv.ComputeVtxHash(p0, p1, p2, Precision);

				faceuvs.Add(fuv);
			}

			// group the faces
			int gCnt = 0;
			List<Group> groups = new List<Group>();
			
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
					{
						dst.GroupId = src.GroupId;
						dst.ConnectId = i;
					}
				}
			}

			Console.WriteLine("{0} faces created {1} groups", fcnt, gCnt);

			// packing
			PackSettings packSettings = new PackSettings();
			List<PackOutputList> packOutputs = new List<PackOutputList>();
			List<PackInput> packInputs = new List<PackInput>();

			for (int i = 0; i < gCnt; ++i)
			{
				Group gp = new Group();
				gp.Count = GroupedFaceUV.Bounding(out gp.Min, out gp.Max, faceuvs, i);
				groups.Add(gp);

				PackInput pi = new PackInput();
				pi.Size.Width = (int)Math.Ceiling(gp.Max.X - gp.Min.X);
				pi.Size.Height = (int)Math.Ceiling(gp.Max.Y - gp.Min.Y);
				packInputs.Add(pi);

				if (debug)
				{
					Console.Write("gp{0}: ", i);
					GroupedFaceUV.Dump(faceuvs, i);
				}
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
					Group gp = groups[po.Input];

					foreach (GroupedFaceUV fuv in faceuvs)
					{
						if (fuv.GroupId != po.Input) continue;

						fuv.Translate(new Vector2(po.X, po.Y) - gp.Min);
						fuv.Scale(scale);
					}
				}
			}

			// create output meshes
			List<Mesh> output = new List<Mesh>();

			foreach (PackOutputList polist in packOutputs)
			{
				Mesh omesh = new Mesh();

				int ofcnt = 0;
				for (int i = 0; i < polist.Count; ++i)
					ofcnt += groups[polist[i].Input].Count;

				omesh.Init(ofcnt * 3);

				int dst = 0;

				for (int i = 0; i < polist.Count; ++i)
				{
					for (int src = 0; src < fcnt; ++src)
					{
						GroupedFaceUV fuv = faceuvs[src];

						if (fuv.GroupId != polist[i].Input) continue;

						mesh.Positions.CopyFaceTo(omesh.Positions, src, dst);
						mesh.Normals.CopyFaceTo(omesh.Normals, src, dst);
						mesh.Texcrds0.CopyFaceTo(omesh.Texcrds0, src, dst);

						omesh.Texcrds1.SetFace(dst, fuv.Texcrd[0], fuv.Texcrd[1], fuv.Texcrd[2]);

						if(debug)
							omesh.FaceProps[dst] = src;
						else
							omesh.FaceProps[dst] = mesh.FaceProps[src];

						++dst;
					}
				}
				output.Add(omesh);
			}

			return output;
		}
	}
}
