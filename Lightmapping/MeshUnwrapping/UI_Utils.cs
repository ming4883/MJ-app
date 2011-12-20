
using System.Windows.Forms;
using System.Drawing;
using System;
using System.IO;
using System.Text;
namespace MCD
{
	public class PackInputDialog : Form
	{
		private void initControl(Control c)
		{
			c.Dock = DockStyle.Left;
		}

		private void initPanel(Panel p)
		{
			p.Dock = DockStyle.Top;
			p.Height = 25;
		}

		private Label newLabel(string text)
		{
			Label lbl = new Label();
			initControl(lbl);
			lbl.Text = text;
			lbl.Width = 100;
			lbl.TextAlign = ContentAlignment.MiddleLeft;
			return lbl;
		}

		private ComboBox newComboBox(object[] items, object selected)
		{
			ComboBox cb = new ComboBox();
			initControl(cb);
			cb.DropDownStyle = ComboBoxStyle.DropDownList;
			cb.Items.AddRange(items);
			cb.SelectedItem = selected;
			cb.Width = 60;
			return cb;
		}

		private NumericUpDown newUpDown(int min, int max, int inc, int val)
		{
			NumericUpDown ud = new NumericUpDown();
			initControl(ud);
			ud.Minimum = min;
			ud.Maximum = max;
			ud.Value = val;
			ud.Increment = inc;
			ud.Width = 60;

			return ud;
		}

		private Button newResultButton(string text, DialogResult ret)
		{
			Button btn = new Button();
			btn.Dock = DockStyle.Right;
			btn.Text = text;
			btn.DialogResult = ret;
			btn.Width = 60;

			return btn;
		}

		ComboBox mapSize;

		private Panel newMapSizePanel()
		{
			Panel p = new Panel();
			initPanel(p);

			mapSize = newComboBox(new object[] { "128", "256", "512", "1024", "2048" }, "512");
			p.Controls.Add(mapSize);
			p.Controls.Add(newLabel("Texture Size = "));

			return p;
		}

		ComboBox pixelSizeUnit;
		NumericUpDown pixelSize;

		private Panel newPixelSizePanel()
		{
			Panel p = new Panel();
			initPanel(p);

			pixelSizeUnit = newComboBox(new object[] { "mm", "cm", "m" }, "m");
			pixelSize = newUpDown(0, 1000, 1, 2);

			p.Controls.Add(pixelSizeUnit);
			p.Controls.Add(pixelSize);
			p.Controls.Add(newLabel("1 Pixel = "));
			return p;
		}

		private Panel newResultButtonPanel()
		{
			Panel p = new Panel();
			initPanel(p);
			p.Dock = DockStyle.Bottom;

			p.Controls.Add(newResultButton("OK", DialogResult.OK));
			p.Controls.Add(newResultButton("Cancel", DialogResult.Cancel));
			return p;
		}

		public PackInputDialog()
		{
			SuspendLayout();

			Size = new Size(300, 150);
			StartPosition = FormStartPosition.CenterParent;
			FormBorderStyle = FormBorderStyle.FixedToolWindow;
			MinimizeBox = false;
			MaximizeBox = false;
			Padding = new Padding(5);

			Controls.Add(newResultButtonPanel());
			Controls.Add(newPixelSizePanel());
			Controls.Add(newMapSizePanel());

			ResumeLayout(true);
		}

		public int MapSize
		{
			get { return int.Parse(mapSize.SelectedItem.ToString()); }
		}

		public float PixelSize
		{
			get { return (float)pixelSize.Value; }
		}

		public string PixelSizeWithUnits
		{
			get { return string.Format("{0}{1}", pixelSize.Value, pixelSizeUnit.SelectedItem); }
		}

		public bool DoModal()
		{
			return DialogResult.OK == ShowDialog(Form.ActiveForm);
		}
	}

	public class TextBoxStreamWriter : TextWriter
	{
		RichTextBox _output = null;
		TextWriter _last = null;
		StringBuilder sb = null;

		public TextBoxStreamWriter(RichTextBox output)
		{
			_output = output;

			_last = Console.Out;
			Console.SetOut(this);
		}

		public void Set()
		{
			_last = Console.Out;
			Console.SetOut(this);
		}

		public void Reset()
		{
			if (null != _last)
			{
				Console.SetOut(_last);
				_last = null;
			}
		}

		public override void Write(char value)
		{
			//base.Write(value);
			if (value == '\r')
				return;

			if (null == sb)
				sb = new StringBuilder();

			sb.Append(value);

			if ('\n' == value)
			{
				string line = sb.ToString();
				sb = null;
				_output.BeginInvoke(new MethodInvoker(delegate()
				{
					_output.AppendText(line);
					_output.Select(_output.Text.Length - 1, 0);
					_output.ScrollToCaret();
				}));
			}
		}

		public override Encoding Encoding
		{
			get { return System.Text.Encoding.UTF8; }
		}
	}

	public class ConsoleViewer : Form
	{
		TextBoxStreamWriter writer;

		public ConsoleViewer()
		{
			SuspendLayout();

			Text = "System.Console";
			Size = new Size(600, 250);
			StartPosition = FormStartPosition.CenterScreen;
			FormBorderStyle = FormBorderStyle.SizableToolWindow;
			MinimizeBox = false;
			MaximizeBox = false;
			Padding = new Padding(2);

			RichTextBox rtb = new RichTextBox();
			rtb.Dock = DockStyle.Fill;
			Controls.Add(rtb);

			writer = new TextBoxStreamWriter(rtb);
			writer.Set();

			ResumeLayout(true);
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			writer.Reset();
			base.OnFormClosed(e);
		}

		public void Display()
		{
			Show(Form.ActiveForm);
		}
	}
}