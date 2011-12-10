using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCD
{
	class Test1
	{
		protected Mesh CreateBoxMesh()
		{
			Mesh ret = new Mesh();

			ret.Init(36, 8);
			ret.Positions[0] = new OpenTK.Vector3(-1, 1, 1);
			ret.Positions[1] = new OpenTK.Vector3(-1,-1, 1);
			ret.Positions[2] = new OpenTK.Vector3( 1, 1, 1);
			ret.Positions[3] = new OpenTK.Vector3( 1,-1, 1);

			ret.Positions[4] = new OpenTK.Vector3(-1, 1,-1);
			ret.Positions[5] = new OpenTK.Vector3(-1,-1,-1);
			ret.Positions[6] = new OpenTK.Vector3( 1, 1,-1);
			ret.Positions[7] = new OpenTK.Vector3( 1,-1,-1);

			for (int i = 0; i < ret.VertexCount; ++i)
			{
				ret.Normals[i] = OpenTK.Vector3.Zero;
				ret.Texcrds0[i] = OpenTK.Vector2.Zero;
				ret.Texcrds1[i] = OpenTK.Vector2.Zero;
			}

			int[] idx = new int[] {
				0,1,2,3,2,1,
				2,3,6,7,6,3,
				6,7,4,5,4,7,
				4,5,0,1,0,5,
				4,0,6,2,6,0,
				1,5,3,7,3,5,
			};

			ret.Indices.Clear();
			ret.Indices.AddRange(idx);
			return ret;
		}
		public void Run()
		{
			Mesh mesh = CreateBoxMesh();
			PreFaceUnwrapper unwrapper = new PreFaceUnwrapper();
			unwrapper.Unwrap(mesh, 0.125f);
		}
	}
}
