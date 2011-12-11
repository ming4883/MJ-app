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

		public Attribute<Vector3> positionsA;
		public Attribute<Vector3> normalsA;
		public Attribute<Vector2> texcrds0A;
		public Attribute<Vector2> texcrds1A;

		public List<int> Indices { get { return indices; } }
		public Attribute<Vector3> Positions { get { return positionsA; } }
		public Attribute<Vector3> Normals { get { return normalsA; } }
		public Attribute<Vector2> Texcrds0 { get { return texcrds0A; } }
		public Attribute<Vector2> Texcrds1 { get { return texcrds1A; } }

		public bool Indexed { get { return null != indices; } }
		public int FaceCount { get { return (null == indices) ? positions.Count / 3 : indices.Count / 3; } }
		public int VertexCount { get { return (null == positions) ? 0 : positions.Count; } }

		public class Attribute<T>
		{
			Mesh mesh;
			List<T> attributes;

			public Attribute(Mesh m, List<T> a)
			{
				mesh = m;
				attributes = a;
			}

			public List<T> Raw { get { return attributes; } }

			public void GetFace(out T a0, out T a1, out T a2, int faceIndex)
			{
				int off = faceIndex * 3;

				if (mesh.Indexed)
				{
					a0 = attributes[mesh.indices[off + 0]];
					a1 = attributes[mesh.indices[off + 1]];
					a2 = attributes[mesh.indices[off + 2]];
				}
				else
				{
					a0 = attributes[off + 0];
					a1 = attributes[off + 1];
					a2 = attributes[off + 2];
				}
			}

			public void SetFace(int faceIndex, T a0, T a1, T a2)
			{
				int off = faceIndex * 3;

				if (mesh.Indexed)
				{
					attributes[mesh.indices[off + 0]] = a0;
					attributes[mesh.indices[off + 1]] = a1;
					attributes[mesh.indices[off + 2]] = a2;
				}
				else
				{
					attributes[off + 0] = a0;
					attributes[off + 1] = a1;
					attributes[off + 2] = a2;
				}
			}
		}

		public void Init(int icnt, int vcnt)
		{
			indices = new List<int>();
			for (int i = 0; i < icnt; ++i)
				indices.Add(0);

			Init(vcnt);
		}

		public void Init(int vcnt)
		{
			positions = new List<Vector3>(vcnt);
			normals = new List<Vector3>(vcnt);
			texcrds0 = new List<Vector2>(vcnt);
			texcrds1 = new List<Vector2>(vcnt);

			positionsA = new Attribute<Vector3>(this, positions);
			normalsA = new Attribute<Vector3>(this, normals);
			texcrds0A = new Attribute<Vector2>(this, texcrds0);
			texcrds1A = new Attribute<Vector2>(this, texcrds1);

			for (int i = 0; i < vcnt; ++i)
			{
				positions.Add(Vector3.Zero);
				normals.Add(Vector3.Zero);
				texcrds0.Add(Vector2.Zero);
				texcrds1.Add(Vector2.Zero);
			}
		}
	}
}
