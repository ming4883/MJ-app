﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace MCD
{
	public class GroupedFaceUnwrapper : PreFaceUnwrapper
	{
		public override Mesh Unwrap(Mesh mesh, int packSize, float worldScale)
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
				faceuv.Normal = n;
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
			Mesh output = new Mesh();
			output.Init(mesh.Indices.Count);

			for (int i = 0; i < fcnt; ++i)
			{
				mesh.Positions.CopyFaceTo(output.Positions, i);
				mesh.Normals.CopyFaceTo(output.Normals, i);
				mesh.Texcrds0.CopyFaceTo(output.Texcrds0, i);

				FaceUV fuv = faceuvs[i];
				output.Texcrds1.SetFace(i, fuv.Texcrd[0], fuv.Texcrd[1], fuv.Texcrd[2]);
			}

			return output;
		}
	}
}
