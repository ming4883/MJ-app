using System.Collections.Generic;

namespace MCD
{
	abstract class TestBase
	{
		public abstract void Run(List<Mesh> outputs);

		public abstract class FactoryBase
		{
			public abstract TestBase Create();
		}

		public class FactoryT<T> : FactoryBase where T : TestBase, new()
		{
			public override TestBase Create() { return new T(); }
			public override string ToString() { return typeof(T).Name; }
		}

		protected Mesh CreateBoxMesh(float hs, OpenTK.Vector3[] vtxOff)
		{
			int[] idx = new int[] {
				0,1,2,3,2,1,
				2,3,6,7,6,3,
				6,7,4,5,4,7,
				4,5,0,1,0,5,
				4,0,6,2,6,0,
				1,5,3,7,3,5,
			};

			OpenTK.Vector3[] vtx = new OpenTK.Vector3[] {
				new OpenTK.Vector3(-hs, hs, hs),
				new OpenTK.Vector3(-hs, -hs, hs),
				new OpenTK.Vector3(hs, hs, hs),
				new OpenTK.Vector3(hs, -hs, hs),

				new OpenTK.Vector3(-hs, hs, -hs),
				new OpenTK.Vector3(-hs, -hs, -hs),
				new OpenTK.Vector3(hs, hs, -hs),
				new OpenTK.Vector3(hs, -hs, -hs),
			};

			int instCnt = vtxOff.Length;

			Mesh ret = new Mesh();
			ret.Init(idx.Length * instCnt, vtx.Length * instCnt);

			ret.Positions.Raw.Clear();
			ret.Indices.Clear();

			for (int i = 0; i < instCnt; ++i)
			{
				int ioff = i * vtx.Length;
				OpenTK.Vector3 voff = vtxOff[i];

				for (int j = 0; j < idx.Length; ++j)
					ret.Indices.Add(idx[j] + ioff);

				for (int j = 0; j < vtx.Length; ++j)
					ret.Positions.Raw.Add(vtx[j] + voff);
			}
			
			return ret;
		}

		protected Mesh CreateBoxMesh(float hs)
		{
			OpenTK.Vector3[] vtxOff = new OpenTK.Vector3[] {
				OpenTK.Vector3.Zero,
			};

			return CreateBoxMesh(hs, vtxOff);
		}

	}
}
