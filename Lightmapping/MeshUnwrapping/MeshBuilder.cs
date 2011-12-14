using System.Collections.Generic;
using OpenTK;

namespace MCD
{
	public class MeshBuilder
	{
		List<int> faceProps = new List<int>();
		List<Vector3> positions = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> texcrds0 = new List<Vector2>();
		List<Vector2> texcrds1 = new List<Vector2>();

		int faceProp;
		Vector3 position;
		Vector3 normal;
		Vector2 texcrd0;
		Vector2 texcrd1;

		public MeshBuilder()
		{
		}

		public void FaceProp(int val)
		{
			faceProp = val;
		}

		public virtual void AddFace()
		{
			faceProps.Add(faceProp);
		}

		public void Position(float x, float y, float z)
		{
			position.X = x; position.Y = y; position.Z = z;
		}

		public void Normal(float x, float y, float z)
		{
			normal.X = x; normal.Y = y; normal.Z = z;
		}

		public void Texcrd0(float x, float y)
		{
			texcrd0.X = x; texcrd0.Y = y;
		}

		public void Texcrd1(float x, float y)
		{
			texcrd1.X = x; texcrd1.Y = y;
		}

		public virtual void AddVertex()
		{
			positions.Add(position);
			normals.Add(normal);
			texcrds0.Add(texcrd0);
			texcrds1.Add(texcrd1);
		}

		public virtual Mesh ToMesh()
		{
			Mesh mesh = new Mesh();
			mesh.Init(positions.Count);

			mesh.FaceProps.Clear();
			mesh.FaceProps.AddRange(faceProps);

			mesh.Positions.Raw.Clear();
			mesh.Positions.Raw.AddRange(positions);

			mesh.Normals.Raw.Clear();
			mesh.Normals.Raw.AddRange(normals);

			mesh.Texcrds0.Raw.Clear();
			mesh.Texcrds0.Raw.AddRange(texcrds0);

			mesh.Texcrds1.Raw.Clear();
			mesh.Texcrds1.Raw.AddRange(texcrds1);

			return mesh;
		}
	}
}
