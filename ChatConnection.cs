using System.Net;
using System.Net.Sockets;
using System.Text;
namespace Lab;
// Bir bağlantı tek satırlık mesaj çerçeveleri kullanır. İlk satır kullanıcı adıdır.
public sealed class ChatConnection:IDisposable
{
 TcpClient? client;
 TcpListener? listener;
 StreamWriter? writer;
 CancellationTokenSource? cancellation;
 readonly SemaphoreSlim sendLock=new(1,1);
 public event Action<string>? Message;
 public event Action<string>? Peer;
 public async Task StartServer(int port,string username)
 {
  cancellation=new(); listener=new TcpListener(IPAddress.Any,port); listener.Start();
  Message?.Invoke("Sunucu dinliyor; istemci bekleniyor.");
  try { client=await listener.AcceptTcpClientAsync(cancellation.Token); await Run(username); }
  finally { Dispose(); }
 }
 public async Task Connect(string host,int port,string username)
 {
  cancellation=new(); client=new TcpClient();
  try { using var timeout=CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token); timeout.CancelAfter(TimeSpan.FromSeconds(10)); await client.ConnectAsync(host,port,timeout.Token); await Run(username); }
  finally { Dispose(); }
 }
 async Task Run(string username)
 {
  var stream=client!.GetStream(); using var reader=new StreamReader(stream,Encoding.UTF8,false,1024,true);
  writer=new StreamWriter(stream,new UTF8Encoding(false),1024,true){AutoFlush=true};
  await writer.WriteLineAsync(username);
  var remoteName=await reader.ReadLineAsync(cancellation!.Token);
  if(string.IsNullOrWhiteSpace(remoteName))throw new IOException("Karşı taraf kullanıcı adı göndermedi.");
  Peer?.Invoke($"{remoteName} | {client.Client.RemoteEndPoint}");
  Message?.Invoke("Bağlantı kuruldu.");
  while(!cancellation.IsCancellationRequested) { var line=await reader.ReadLineAsync(cancellation.Token); if(line is null)break; Message?.Invoke($"{remoteName}: {line}"); }
  Message?.Invoke("Karşı taraf bağlantıyı kapattı.");
 }
 public async Task Send(string text)
 {
  await sendLock.WaitAsync();
  try { if(writer is null)throw new InvalidOperationException("Önce bir bağlantı kurun."); await writer.WriteLineAsync(text.Replace("\r"," ").Replace("\n"," ")); }
  finally { sendLock.Release(); }
 }
 public void Dispose() { cancellation?.Cancel(); client?.Close(); listener?.Stop(); writer=null; Peer?.Invoke(""); }
}
