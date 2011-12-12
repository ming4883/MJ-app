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

		protected Mesh CreateBoxMesh(float hs)
		{
			Mesh ret = new Mesh();

			ret.Init(36, 8);
			ret.Positions.Raw[0] = new OpenTK.Vector3(-hs, hs, hs);
			ret.Positions.Raw[1] = new OpenTK.Vector3(-hs, -hs, hs);
			ret.Positions.Raw[2] = new OpenTK.Vector3(hs, hs, hs);
			ret.Positions.Raw[3] = new OpenTK.Vector3(hs, -hs, hs);

			ret.Positions.Raw[4] = new OpenTK.Vector3(-hs, hs, -hs);
			ret.Positions.Raw[5] = new OpenTK.Vector3(-hs, -hs, -hs);
			ret.Positions.Raw[6] = new OpenTK.Vector3(hs, hs, -hs);
			ret.Positions.Raw[7] = new OpenTK.Vector3(hs, -hs, -hs);

			for (int i = 0; i < ret.VertexCount; ++i)
			{
				ret.Normals.Raw[i] = OpenTK.Vector3.Zero;
				ret.Texcrds0.Raw[i] = OpenTK.Vector2.Zero;
				ret.Texcrds1.Raw[i] = OpenTK.Vector2.Zero;
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

		
	}
}
