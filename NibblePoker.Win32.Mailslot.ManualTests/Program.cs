using System.Text;

namespace NibblePoker.Win32.Mailslot.ManualTests;

#pragma warning disable IDE0090 // Use 'new(...)'
internal static class Program {

    public static int Main(string[] args) {

        /*
        string mailslotPath = Guid.NewGuid().ToString();
        string mailslotText = "Hello world !";
        
        // Setting up server
        Console.WriteLine("Starting server on `\\\\.\\mailslot\\{mailslotPath}`");
        using (var server = new MailslotServer(null, mailslotPath, 1024, 100)) {
            // Preparing sync client
            using (var clientSync = new MailslotClient(null, mailslotPath, isAsync: false)) {
                Console.WriteLine($"Sending: `{mailslotText}`");
                clientSync.SendSync(mailslotText);
            }

            // Reading data from the server
            if(server.GetInfo(out _, out uint nextMessageSize, out uint queuedMsgCount, out _)) {
                if (queuedMsgCount <= 0) {
                    Console.Error.WriteLine("No queued messages in the mailslot !");
                    return 1;
                }

                if (nextMessageSize <= 0) {
                    Console.Error.WriteLine($"The next message's size is invalid ! ({nextMessageSize})");
                    return 1;
                }

                byte[] buffer = new byte[nextMessageSize];
                int bytesRead = server.GetFileStream().Read(buffer, 0, buffer.Length);

                if (bytesRead > 0) {
                    Console.WriteLine($"Received: `{Encoding.ASCII.GetString(buffer)}`");
                } else {
                    Console.Error.WriteLine("Couldn't read from the mailslot !");
                    return 1;
                }
            }
        }*/


        /*string mailslotPath = Guid.NewGuid().ToString();
        string mailslotText = "Hello world !";
        uint mailslotSize = 1024;
        byte[] buffer = new byte[mailslotSize];

        // Server that waits forever
        Console.WriteLine($"Starting server on `\\\\.\\mailslot\\{mailslotPath}`");
        var server = new MailslotServer(
            null, mailslotPath, mailslotSize,
            MailslotServer.MAILSLOT_WAIT_FOREVER
        );

        // Client that will send its data after 2 second.
        Task.Factory.StartNew( () => {
            Thread.Sleep(TimeSpan.FromSeconds(2.0));

            Console.WriteLine($"Sending: `{mailslotText}`");
            var client = new MailslotClient(null, mailslotPath, isAsync: false);
            client.SendSync(mailslotText);

            // Flushing is handled in `MailslotClient.Send`
        });

        Console.WriteLine($"Waiting for data...");
        int bytesRead = server.GetFileStream().Read(buffer, 0, buffer.Length);

        if (bytesRead > 0) {
            Console.WriteLine($"Received: `{Encoding.ASCII.GetString(buffer)}`");
        } else {
            Console.Error.WriteLine("Couldn't read from the mailslot !");
            return 1;
        }/**/



        //FileStream clientStream = clientSync.GetFileStream();
        //clientStream.Write(Encoding.ASCII.GetBytes(mailslotText));
        //clientStream.Flush();
        /**/

        /*MailslotClient clientAsync = new MailslotClient(".", mailslotPath, true);
        clientAsync.GetFileStream().WriteAsync(Encoding.ASCII.GetBytes(mailslotText));
        /**/


        return 0;
    }

}

