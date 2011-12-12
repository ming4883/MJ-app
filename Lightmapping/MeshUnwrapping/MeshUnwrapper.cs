using System.Collections.Generic;

namespace MCD
{
	public interface IMeshUnwrapper
	{
		List<Mesh> Unwrap(Mesh mesh, int packSize, float worldScale);
	}
}
