using System.Collections.Generic;

namespace MCD
{
	class Test2 : TestBase
	{
		public class Factory : TestBase.FactoryT<Test2> { }

		public override void Run(List<Mesh> outputs)
		{
			Mesh mesh = CreateBoxMesh(16.0f);
			GroupedFaceUnwrapper unwrapper = new GroupedFaceUnwrapper();
			outputs.AddRange(unwrapper.Unwrap(mesh, 512, 1.0f));
		}
	}
}
