using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MCD
{
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

	public class PackDialog : Form
	{
		ComboBox mapSize;
		int mapSizeVal = 256;
		
		ComboBox pixelSizeUnit;
		string pixelSizeUnitVal = "cm";

		NumericUpDown pixelSize;
		float pixelSizeVal = 20;

		TextBox outputName;
		string outputNameVal = "PackResult";

		NumericUpDown borderSize;
		int borderSizeVal = 1;

		RichTextBox logger;

		Label info;
		
		TextBoxStreamWriter writer;

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

		private NumericUpDown newUpDown(float min, float max, float inc, float val)
		{
			NumericUpDown ud = new NumericUpDown();
			initControl(ud);
			ud.Minimum = (decimal)min;
			ud.Maximum = (decimal)max;
			ud.Value = (decimal)val;
			ud.Increment = (decimal)inc;
			ud.Width = 60;

			if (inc < 1)
				ud.DecimalPlaces = 1;

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

		private TextBox newTextBox(string text)
		{
			TextBox tb = new TextBox();
			initControl(tb);
			tb.Text = text;
			tb.Width = 100;
			tb.Multiline = false;
			tb.ReadOnly = false;
			return tb;
		}

		private Panel newMapSizePanel()
		{
			Panel p = new Panel();
			initPanel(p);

			p.Controls.Add(newLabel("pixels"));
			p.Controls.Add(mapSize = newComboBox(new object[] { "16", "32", "64", "128", "256", "512", "1024", "2048" }, mapSizeVal.ToString()));
			p.Controls.Add(newLabel("Texture Size = "));

			mapSize.SelectedIndexChanged += delegate(object s, EventArgs e)
			{
				int.TryParse(mapSize.SelectedItem.ToString(), out mapSizeVal);
			};

			return p;
		}

		private Panel newPixelSizePanel()
		{
			Panel p = new Panel();
			initPanel(p);

			p.Controls.Add(pixelSizeUnit = newComboBox(new object[] { "mm", "cm", "m" }, pixelSizeUnitVal));
			p.Controls.Add(pixelSize = newUpDown(0, 1000, 0.1f, (int)pixelSizeVal));
			p.Controls.Add(newLabel("Pixel Size = "));

			pixelSize.ValueChanged += delegate(object s, EventArgs e)
			{
				pixelSizeVal = (float)pixelSize.Value;
			};

			pixelSizeUnit.SelectedIndexChanged += delegate(object s, EventArgs e)
			{
				pixelSizeUnitVal = pixelSizeUnit.SelectedItem.ToString();
			};

			return p;
		}

		private Panel newInfoPanel()
		{
			Panel p = new Panel();
			initPanel(p);
			//p.Height = 50;

			p.Controls.Add(info = new Label());
			info.AutoSize = false;
			info.Dock = DockStyle.Fill;
			info.BorderStyle = BorderStyle.FixedSingle;
			info.TextAlign = ContentAlignment.TopLeft;
			return p;
		}

		private Panel newOutputNamePanel()
		{
			Panel p = new Panel();
			initPanel(p);

			p.Controls.Add(outputName = newTextBox(outputNameVal));
			p.Controls.Add(newLabel("Output Name = "));

			outputName.TextChanged += delegate(object s, EventArgs e)
			{
				outputNameVal = outputName.Text;
			};

			return p;
		}

		private Panel newBorderSizePanel()
		{
			Panel p = new Panel();
			initPanel(p);

			p.Controls.Add(newLabel("pixels"));
			p.Controls.Add(borderSize = newUpDown(1, 16, 1, borderSizeVal));
			p.Controls.Add(newLabel("Border = "));

			borderSize.ValueChanged += delegate(object s, EventArgs e)
			{
				borderSizeVal = (int)borderSize.Value;
			};

			return p;
		}

		private Panel newButtonPanel()
		{
			Panel p = new Panel();
			initPanel(p);
			p.Dock = DockStyle.Bottom;

			Button btn;

			p.Controls.Add(btn = newResultButton("Pack", DialogResult.None));
			//p.Controls.Add(newResultButton("Close", DialogResult.OK));

			btn.Click += delegate(object s, EventArgs e)
			{
				if (null == Pack) return;

				Enabled = false;
				logger.Clear();

				ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object h)
				{
					Pack(this, null);
					Invoke(new MethodInvoker(delegate() { Enabled = true; }));
				}));
			};
			
			return p;
		}

		private RichTextBox newLogger()
		{
			RichTextBox r = new RichTextBox();
			r.Dock = DockStyle.Fill;
			r.BorderStyle = BorderStyle.Fixed3D;
			r.ReadOnly = true;

			return r;
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			writer.Reset();
			base.OnFormClosed(e);
		}

		public PackDialog()
		{
			SuspendLayout();

			Size = new Size(600, 350);
			StartPosition = FormStartPosition.CenterScreen;
			FormBorderStyle = FormBorderStyle.FixedToolWindow;
			MinimizeBox = false;
			MaximizeBox = false;
			Padding = new Padding(5);

			Controls.Add(logger = newLogger());
			Controls.Add(newButtonPanel());
			Controls.Add(newInfoPanel());
			Controls.Add(newBorderSizePanel());
			Controls.Add(newPixelSizePanel());
			Controls.Add(newMapSizePanel());
			Controls.Add(newOutputNamePanel());

			ResumeLayout(true);

			writer = new TextBoxStreamWriter(logger);
			writer.Set();
		}

		public event EventHandler<EventArgs> Pack;

		public int MapSize { get { return mapSizeVal; } }

		public float PixelSize { get { return pixelSizeVal; } }

		public string PixelSizeWithUnits { get { return string.Format("{0}{1}", pixelSizeVal, pixelSizeUnitVal); } }

		public int BorderSize { get { return borderSizeVal; } }

		public string OutputName { get { return outputNameVal; } }

		public string Info
		{
			set
			{
				Invoke(new MethodInvoker(delegate() { info.Text = value; }));
			}
		}

		public void SetInfo(string fmt, object arg0)
		{
			Info = string.Format(fmt, arg0);
		}

		public void SetInfo(string fmt, object arg0, object arg1)
		{
			Info = string.Format(fmt, arg0, arg1);
		}

		public void SetInfo(string fmt, object arg0, object arg1, object arg2)
		{
			Info = string.Format(fmt, arg0, arg1, arg2);
		}

		public void SetInfo(string fmt, object arg0, object arg1, object arg2, object arg3)
		{
			Info = string.Format(fmt, arg0, arg1, arg2, arg3);
		}

		public bool DoModel()
		{
			Show(Form.ActiveForm);
			return true;
		}
	}
}