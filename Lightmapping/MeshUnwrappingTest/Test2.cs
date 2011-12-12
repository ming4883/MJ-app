﻿using System.Collections.Generic;

namespace MCD
{
	class Test2 : TestBase
	{
		public class Factory : TestBase.FactoryT<Test2> { }

		public override void Run(List<Mesh> outputs)
		{
			Mesh mesh = CreateBoxMesh(8.0f);
			GroupedFaceUnwrapper unwrapper = new GroupedFaceUnwrapper();
			outputs.Add(unwrapper.Unwrap(mesh, 512, 1.0f));
		}
	}
}
