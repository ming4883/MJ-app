using System.Collections.Generic;

namespace MCD
{
	class Test1 : TestBase
	{
		public class Factory : TestBase.FactoryT<Test1> { }

		public override void Run(List<Mesh> outputs)
		{
			Mesh mesh = CreateBoxMesh(16.0f);
			PreFaceUnwrapper unwrapper = new PreFaceUnwrapper();
			outputs.AddRange(unwrapper.Unwrap(mesh, 512, 1.0f));
		}
	}
}
