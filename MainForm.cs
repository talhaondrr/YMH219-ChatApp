namespace Lab;
public sealed class MainForm:Form
{
 readonly TextBox host=new(){Text="127.0.0.1",Width=120}, name=new(){Text="Ogrenci",Width=110}, message=new(){Dock=DockStyle.Fill,MaxLength=4000};
 readonly NumericUpDown port=new(){Minimum=1024,Maximum=65535,Value=5000,Width=80};
 readonly ListBox log=new(){Dock=DockStyle.Fill,HorizontalScrollbar=true};
 readonly Label peer=Ui.Label("Bağlı istemci: yok");
 readonly Button server,connect,disconnect;
 ChatConnection? connection;
 public MainForm()
 {
  Ui.Setup(this,"TCP Chat"); var bar=Ui.Bar();
  server=Ui.Button("Sunucu başlat",async(_,_)=>await Open(true)); connect=Ui.Button("Bağlan",async(_,_)=>await Open(false)); disconnect=Ui.Button("Bağlantıyı kes",(_,_)=>connection?.Dispose());
  bar.Controls.AddRange([Ui.Label("Ad"),name,Ui.Label("Adres"),host,Ui.Label("Port"),port,server,connect,disconnect]);
  peer.Dock=DockStyle.Bottom;
  var send=Ui.Button("Mesaj gönder",async(_,_)=>await Send()); send.Dock=DockStyle.Right;
  var bottom=new Panel{Dock=DockStyle.Bottom,Height=48,Padding=new Padding(6)}; bottom.Controls.Add(message);bottom.Controls.Add(send);
  Controls.Add(log); Controls.Add(peer);Controls.Add(bottom); Controls.Add(bar);
  message.KeyDown+=async(_,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;await Send();}};
  FormClosing+=(_,_)=>connection?.Dispose();
 }
 void OnUi(Action action) { if(IsDisposed||!IsHandleCreated)return; if(InvokeRequired)BeginInvoke(action);else action(); }
 void Log(string text)=>OnUi(()=>{log.Items.Add($"[{DateTime.Now:HH:mm:ss}] {text}");log.TopIndex=log.Items.Count-1;});
 async Task Open(bool asServer)
 {
  if(string.IsNullOrWhiteSpace(name.Text)){MessageBox.Show("Kullanıcı adı girin.");return;}
  server.Enabled=connect.Enabled=false;name.Enabled=false;host.Enabled=false;port.Enabled=false;
  var current=new ChatConnection();connection=current;current.Message+=Log;current.Peer+=text=>OnUi(()=>peer.Text="Bağlı istemci: "+(text.Length==0?"yok":text));
  try { if(asServer)await current.StartServer((int)port.Value,name.Text.Trim());else await current.Connect(host.Text.Trim(),(int)port.Value,name.Text.Trim()); }
  catch(OperationCanceledException){Log("Bağlantı işlemi durdu veya zaman aşımına uğradı.");}
  catch(Exception e){Log("Bağlantı hatası: "+e.Message);}
  finally { current.Dispose(); if(!IsDisposed){connection=null;server.Enabled=connect.Enabled=true;name.Enabled=host.Enabled=port.Enabled=true;Log("Bağlantı kapalı.");} }
 }
 async Task Send() { if(string.IsNullOrWhiteSpace(message.Text))return;try{if(connection is null)throw new InvalidOperationException("Önce bağlanın.");var text=message.Text;await connection.Send(text);Log(name.Text+": "+text);message.Clear();}catch(Exception e){Ui.Error(e);} }
}
