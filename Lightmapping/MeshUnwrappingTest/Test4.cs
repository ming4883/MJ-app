using System.Collections.Generic;

namespace MCD
{
	class Test4 : TestBase
	{
		public class Factory : TestBase.FactoryT<Test4> { }

		public override void Run(List<Mesh> outputs)
		{
			Mesh mesh = MeshUtil.Load("Test4.data.txt");
			GroupedFaceUnwrapper unwrapper = new GroupedFaceUnwrapper();
			outputs.AddRange(unwrapper.Unwrap(mesh, 512, 1.0f / 0.5f));
		}
	}
}
