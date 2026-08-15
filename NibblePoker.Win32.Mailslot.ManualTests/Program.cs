using System.Text;

namespace NibblePoker.Win32.Mailslot.ManualTests;

#pragma warning disable IDE0090 // Use 'new(...)'
internal static class Program {

    public static int Main(string[] args) {
        string mailslotPath = Guid.NewGuid().ToString();
        string mailslotText = "Hello world !";

        MailslotServer server = new MailslotServer(mailslotPath, 1024, 99999);

        MailslotClient clientSync = new MailslotClient(".", mailslotPath, false);
        clientSync.GetFileStream().Write(Encoding.ASCII.GetBytes(mailslotText));
        /**/

        /*MailslotClient clientAsync = new MailslotClient(".", mailslotPath, true);
        clientAsync.GetFileStream().WriteAsync(Encoding.ASCII.GetBytes(mailslotText));
        /**/

        byte[] buffer = new byte[server.MaxMessageSize];
        int bytesRead = server.GetFileStream().Read(buffer, 0, buffer.Length);

        if(bytesRead > 0) {
            Console.WriteLine(Encoding.ASCII.GetString(buffer));
        } else {
            Console.Error.WriteLine("Couldn't read from the mailslot !");
        }

        return 0;
    }

}

