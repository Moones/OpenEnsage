using System.Drawing;
using System.Windows.Forms;

namespace Loader
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            int selectionStart = box.SelectionStart;
            int selectionLength = box.SelectionLength;
            box.SelectionStart = box.TextLength;
            box.SelectionLength = text.Length;
            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
            box.SelectionStart = selectionStart;
            box.SelectionLength = selectionLength;
        }

        public static void ScrollToEnd(this RichTextBox box)
        {
            WinAPI.SendMessage(box.Handle, 277U, 7U, 0U);
        }
    }
}
