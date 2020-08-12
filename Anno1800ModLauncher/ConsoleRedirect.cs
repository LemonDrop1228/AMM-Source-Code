using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Anno1800ModLauncher
{
    public class ControlWriter : TextWriter
    {
        private TextBox rtb;
        private MainWindow Parent;
        public ControlWriter(TextBox richTextBox, MainWindow parent)
        {
            this.rtb = richTextBox;
            this.Parent = parent;
        }

        public override void Write(char value)
        {
            rtb.Text += value;
        }

        public override void Write(string value)
        {
            Parent.Dispatcher.BeginInvoke((Action)(() => {
                var sourceContent = rtb.Text;
                string[] collection = sourceContent.Split(new string[] { Environment.NewLine },
                           StringSplitOptions.None);
                var contentSplit = collection.ToList(); 
                contentSplit[contentSplit.FindIndex(n => n == contentSplit.Last())] = $"{DateTime.Now.ToString("MM.dd.yyyy-hh:mm:ss")} | <Log>: {value}";
                var contentUpdated = String.Join(Environment.NewLine, contentSplit);
                rtb.Text = contentUpdated;

            }));
        }

        public override void WriteLine(string value)
        {
            Parent.Dispatcher.BeginInvoke((Action)(() => {
                rtb.Text += Environment.NewLine + $"{DateTime.Now.ToString("MM.dd.yyyy-hh:mm:ss")} | <Log>: {value}";
            })); 
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
