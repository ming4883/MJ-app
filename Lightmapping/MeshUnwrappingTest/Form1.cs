using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MCD
{
	public partial class Form1 : Form
	{
		List<Bitmap> bmps;

		public class TextBoxStreamWriter : TextWriter
		{
			RichTextBox _output = null;

			public TextBoxStreamWriter(RichTextBox output)
			{
				_output = output;
			}

			public override void Write(char value)
			{
				//base.Write(value);
				if (value == '\r')
					return;

				_output.BeginInvoke(new MethodInvoker(delegate()
				{
					_output.AppendText(value.ToString());
					_output.Select(_output.Text.Length - 1, 0);
					_output.ScrollToCaret();
				}));
			}

			public override Encoding Encoding
			{
				get { return System.Text.Encoding.UTF8; }
			}
		}

		public Form1()
		{
			InitializeComponent();
			Text = Application.ProductName;
			panel1.Paint += new PaintEventHandler(panel1_Paint);
			numericUpDown1.ValueChanged += delegate(object s, EventArgs a)
			{
				panel1.Invalidate();
			};

			/*
			listBox1.Items.Add(new Test1.Factory());
			listBox1.Items.Add(new Test2.Factory());
			listBox1.Items.Add(new Test3.Factory());
			listBox1.Items.Add(new Test4.Factory());
			listBox1.Items.Add(new Test5.Factory());
			listBox1.Items.Add(new Test6.Factory());
			*/

			listBox1.DoubleClick += new EventHandler(button1_Click);

			Console.SetOut(new TextBoxStreamWriter(logger));
		}

		void panel1_Paint(object sender, PaintEventArgs e)
		{
			if (null == bmps || 0 == bmps.Count)
				return;

			e.Graphics.DrawImage(bmps[(int)numericUpDown1.Value], Point.Empty);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			//if (null == listBox1.SelectedItem)
			//	return;

			logger.Clear();

			Test1 test = new Test1();
			test.Run();
			/*
			TestBase test = (listBox1.SelectedItem as TestBase.FactoryBase).Create();
			
			bmps = new List<Bitmap>();
			test.Run(bmps);

			Console.WriteLine("{0} texture is generated.", bmps.Count);

			if (numericUpDown1.Value > bmps.Count - 1)
				numericUpDown1.Value = 0;

			numericUpDown1.Maximum = bmps.Count - 1;
			*/
			panel1.Invalidate();
		}
	}
}
