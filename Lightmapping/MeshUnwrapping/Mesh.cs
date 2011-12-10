using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace MCD
{
	public class Mesh
	{
		List<int> indices;
		List<Vector3> positions;
		List<Vector3> normals;
		List<Vector2> texcrds0;
		List<Vector2> texcrds1;

		public List<int> Indices { get { return indices; } }
		public List<Vector3> Positions { get { return positions; } }
		public List<Vector3> Normals { get { return normals; } }
		public List<Vector2> Texcrds0 { get { return texcrds0; } }
		public List<Vector2> Texcrds1 { get { return texcrds1; } }

		public bool Indexed { get { return null == indices; } }
		public int FaceCount { get { return (null == indices) ? positions.Count / 3 : indices.Count / 3; } }
		public int VertexCount { get { return (null == positions) ? 0 : positions.Count; } }

		public void Init(int icnt, int vcnt)
		{
			indices = new List<int>();
			for (int i = 0; i < icnt; ++i)
				indices.Add(0);

			positions = new List<Vector3>(vcnt);
			normals = new List<Vector3>(vcnt);
			texcrds0 = new List<Vector2>(vcnt);
			texcrds1 = new List<Vector2>(vcnt);

			for (int i = 0; i < vcnt; ++i)
			{
				positions.Add(Vector3.Zero);
				normals.Add(Vector3.Zero);
				texcrds0.Add(Vector2.Zero);
				texcrds1.Add(Vector2.Zero);
			}
		}

		public void Init(int vcnt)
		{
			positions = new List<Vector3>(vcnt);
			normals = new List<Vector3>(vcnt);
			texcrds0 = new List<Vector2>(vcnt);
			texcrds1 = new List<Vector2>(vcnt);

			for (int i = 0; i < vcnt; ++i)
			{
				positions.Add(Vector3.Zero);
				normals.Add(Vector3.Zero);
				texcrds0.Add(Vector2.Zero);
				texcrds1.Add(Vector2.Zero);
			}
		}

		public int FaceOffset(int faceIndex)
		{
			return faceIndex * 3;
		}

		public void FacePositions(out Vector3 p0, out Vector3 p1, out Vector3 p2, int faceIndex)
		{
			int off = FaceOffset(faceIndex);
			p0 = positions[indices[off + 0]];
			p1 = positions[indices[off + 1]];
			p2 = positions[indices[off + 2]];
		}

		public void FaceNormals(out Vector3 p0, out Vector3 p1, out Vector3 p2, int faceIndex)
		{
			int off = FaceOffset(faceIndex);
			p0 = normals[indices[off + 0]];
			p1 = normals[indices[off + 1]];
			p2 = normals[indices[off + 2]];
		}
	}
}
