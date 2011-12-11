using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MCD
{
	public class PackInput
	{
		public Size Size;
		public object Userdata;
	}

	public class PackOutput
	{
		public int Input;
		public int X;
		public int Y;
		public bool Rotated;
	}

	public class PackOutputList : List<PackOutput>
	{
	}

	public struct PackSettings
	{
		public Size Size;
		public int Border;
	}

	public interface IPacker
	{
		void Pack(PackSettings settings, List<PackInput> inputs, List<PackOutputList> outputs);
	}
}
