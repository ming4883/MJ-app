using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MCD
{
	// implementing description on 
	// http://www.blackpawn.com/texts/lightmaps/default.html
	// ref implementation https://raw.github.com/mackstann/binpack/master/livedemo.html
	public class JimScottPacker : IPacker
	{
		class Node
		{
			public Rectangle Rect;
			public bool Rotated = false;
			public int Input = -1;
			public int Parent = -1;
			public int LeftChild = -1;
			public int RightChild = -1;
			
			public Node(Rectangle rect)
			{
				Rect = rect;
			}

			public bool IsFilled() { return Input != -1; }
			public bool IsLeaf() { return LeftChild == -1 && RightChild == -1; }

			public bool FitsIn(Size inner)
			{
				return Rect.Width >= inner.Width && Rect.Height >= inner.Height;
			}

			public bool SameSizeAs(Size other)
			{
				return Rect.Width == other.Width && Rect.Height == other.Height;
			}

		}

		class Tree
		{
			public List<Node> Nodes = new List<Node>();

			public Tree(Size sz)
			{
				Node n = new Node(new Rectangle(Point.Empty, sz));
				Nodes.Add(n);
			}

			public int Insert(Size sz)
			{
				return Insert(0, sz);
			}

			private int Insert(int nodeIndex, Size sz)
			{
				Node node = Nodes[nodeIndex];

				if (-1 != node.LeftChild)
				{
					int ret = Insert(node.LeftChild, sz);
					if (-1 != ret)
						return ret;

					return Insert(node.RightChild, sz);
				}

				if (node.IsFilled())
					return -1;

				if (!node.FitsIn(sz))
					return -1;

				if (node.SameSizeAs(sz))
				{
					return nodeIndex;
				}

				int wdiff = node.Rect.Width - sz.Width;
				int hdiff = node.Rect.Height - sz.Height;
				Rectangle lrect, rrect, me = node.Rect;

				if (wdiff > hdiff)
				{
					lrect = new Rectangle(me.X, me.Y, sz.Width, me.Height);
					rrect = new Rectangle(me.X + sz.Width, me.Y, me.Width - sz.Width, me.Height);
				}
				else
				{
					lrect = new Rectangle(me.X, me.Y, me.Width, sz.Height);
					rrect = new Rectangle(me.X, me.Y + sz.Height, me.Width, me.Height - sz.Height);
				}

				node.LeftChild = Nodes.Count;
				Nodes.Add(new Node(lrect));

				node.RightChild = Nodes.Count;
				Nodes.Add(new Node(rrect));

				return Insert(node.LeftChild, sz);
			}
		}

		public bool AllowRotate = false;
		public bool debug = false;
		
		public void Pack(PackSettings settings, List<PackInput> inputs, List<PackOutputList> outputs)
		{
			int X_LIMIT = settings.Size.Width - settings.Border;
			int Y_LIMIT = settings.Size.Height - settings.Border;

			List<Tree> trees = new List<Tree>();
			trees.Add(new Tree(settings.Size));

			// create a LUT for sort input
			List<int> sorted = new List<int>(inputs.Count);
			for (int i = 0; i < inputs.Count; ++i)
				sorted.Add(i);

			sorted.Sort(delegate(int a, int b)
			{
				Size sza = inputs[a].Size;
				Size szb = inputs[b].Size;

				int areaa = sza.Width * sza.Height;
				int areab = szb.Width * szb.Height;

				return areab - areaa;
			});

			// perform packing
			foreach(int i in sorted)
			{
				Size sz = inputs[i].Size;
				if (sz.Width > settings.Size.Width || sz.Height > settings.Size.Height)
				{
					if(debug)
						Console.WriteLine("warning: input {0}:{1} is too large!", i, sz);
					continue;
				}

				// apply borders, if the input is small enough
				if (sz.Width < X_LIMIT)
					sz.Width += settings.Border * 2;

				if (sz.Height < Y_LIMIT)
					sz.Height += settings.Border * 2;

				Size szr = new Size(sz.Height, sz.Width);

				int ret = -1;

				// find a tree to insert the input
				foreach (Tree tree in trees)
				{
					ret = tree.Insert(sz);

					if (-1 != ret) // packed
					{
						tree.Nodes[ret].Input = i;
						break;
					}
					else if (AllowRotate)
					{
						ret = tree.Insert(szr);
						if (-1 != ret) // packed
						{
							tree.Nodes[ret].Input = i;
							tree.Nodes[ret].Rotated = true;
							//Console.WriteLine("input {0} is rotated", i);
							break;
						}
					}
				}

				// if running out of spaces, create a new tree for packing
				if(-1 == ret)
				{
					if(debug)
						Console.WriteLine("Running out of space, allocate a new texture");
					Tree tree = new Tree(settings.Size);
					trees.Add(tree);

					ret = tree.Insert(sz);
					if (-1 != ret) // packed
						tree.Nodes[ret].Input = i;
					else
						if(debug) Console.WriteLine("still cannot pack input {0}:{1}!", i, sz);
				}
			}

			// output pack results
			foreach (Tree tree in trees)
			{
				PackOutputList polist = new PackOutputList();
				foreach (Node node in tree.Nodes)
				{
					if (!node.IsFilled())
						continue;

					PackOutput po = new PackOutput();
					PackInput pi = inputs[node.Input];

					if (pi.Size.Width < settings.Size.Width - 1)
						po.X = node.Rect.X + settings.Border;

					if (pi.Size.Height < settings.Size.Height - 1)
						po.Y = node.Rect.Y + settings.Border;

					po.Input = node.Input;
					po.Rotated = node.Rotated;

					polist.Add(po);
				}

				outputs.Add(polist);
			}
		}
	}
}
