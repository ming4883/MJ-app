﻿using System;
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
		List<Mesh> meshes = null;

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

			listBox1.Items.Add(new Test1.Factory());
			listBox1.Items.Add(new Test2.Factory());

			listBox1.DoubleClick += new EventHandler(button1_Click);

			Console.SetOut(new TextBoxStreamWriter(logger));
		}

		void panel1_Paint(object sender, PaintEventArgs e)
		{
			if (null == meshes || 0 == meshes.Count)
				return;

			MeshVisualizer.DrawTexcrd1(meshes[(int)numericUpDown1.Value], e.Graphics, panel1.Size);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (null == listBox1.SelectedItem)
				return;

			logger.Clear();

			meshes = new List<Mesh>();
			(listBox1.SelectedItem as TestBase.FactoryBase).Create().Run(meshes);

			if (numericUpDown1.Value > meshes.Count - 1)
				numericUpDown1.Value = 0;

			numericUpDown1.Maximum = meshes.Count - 1;
			
			panel1.Invalidate();
		}
	}
}
