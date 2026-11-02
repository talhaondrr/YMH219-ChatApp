namespace Lab;
internal static class Ui
{
 public static FlowLayoutPanel Bar() => new() { Dock=DockStyle.Top, AutoSize=true, Padding=new Padding(10), WrapContents=true };
 public static Button Button(string text, EventHandler action) { var b=new Button { Text=text, AutoSize=true, Height=34, Margin=new Padding(4) }; b.Click+=action; return b; }
 public static Label Label(string text) => new() { Text=text, AutoSize=true, Padding=new Padding(7) };
 public static void Setup(Form form,string title) { form.Text="YMH219 • "+title; form.ClientSize=new Size(1000,680); form.MinimumSize=new Size(800,580); form.StartPosition=FormStartPosition.CenterScreen; form.Font=new Font("Segoe UI",10); form.BackColor=Color.FromArgb(242,245,250); }
 public static void Error(Exception e) => MessageBox.Show(e.Message,"İşlem tamamlanamadı",MessageBoxButtons.OK,MessageBoxIcon.Warning);
}
