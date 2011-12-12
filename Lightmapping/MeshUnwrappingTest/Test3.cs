using System.Collections.Generic;

namespace MCD
{
	class Test3 : TestBase
	{
		public class Factory : TestBase.FactoryT<Test3> { }

		public override void Run(List<Mesh> outputs)
		{
			OpenTK.Vector3[] vtxOff = new OpenTK.Vector3[] {
				new OpenTK.Vector3(-32, 0, 0),
				new OpenTK.Vector3(32, 0, 0),
			};

			Mesh mesh = CreateBoxMesh(16.0f, vtxOff);
			GroupedFaceUnwrapper unwrapper = new GroupedFaceUnwrapper();
			outputs.AddRange(unwrapper.Unwrap(mesh, 512, 1.0f));
		}
	}
}
