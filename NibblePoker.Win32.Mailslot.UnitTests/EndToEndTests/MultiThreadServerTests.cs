using NUnit.Framework;
using System;
using System.Threading;

namespace NibblePoker.Win32.Mailslot.UnitTests.EndToEndTests;

internal class MultiThreadServerTests {

    private static readonly string SERVER_UUID = "b9e1de4f-3353-44c7-8943-108dc9b3c4fe";

    private static readonly byte[] TEST_DATA = [0xDE, 0xAD, 0xBE, 0xEF];

    [Test]
    public void TestMultiThreadedServer() {
        // Preparing the server
        MailslotServer ms = new MailslotServer(
            ".", SERVER_UUID,
            MailslotServer.MAILSLOT_ANY_MESSAGE_SIZE,
            MailslotServer.MAILSLOT_WAIT_FOREVER
        );

        // Starting the threads
        bool threadExited1 = false;
        bool threadExited2 = false;

        Thread thread1 = new Thread(() => {
            byte[] buffer = new byte[TEST_DATA.Length];
            int bytesRead = ms.Read(buffer, 0, buffer.Length);
            threadExited1 = true;
        });
        thread1.Start();

        Thread thread2 = new Thread(() => {
            byte[] buffer = new byte[TEST_DATA.Length];
            int bytesRead = ms.Read(buffer, 0, buffer.Length);
            threadExited2 = true;
        });
        thread2.Start();

        // Sending data
        MailslotClient mc1 = new MailslotClient(".", SERVER_UUID);
        mc1.Write(TEST_DATA, 0, TEST_DATA.Length);
        mc1.Flush();

        // Waiting a bit
        Thread threadWaiter1 = new Thread(() => {
            while(true) {
                Thread.Sleep(1);
            }
        });
        threadWaiter1.Start();
        threadWaiter1.Join(TimeSpan.FromSeconds(1.0));

        // Checking if a server got data
        Assert.That(threadExited1, Is.Not.EqualTo(threadExited2));

        // Sending some more
        MailslotClient mc2 = new MailslotClient(".", SERVER_UUID);
        mc2.Write(TEST_DATA, 0, TEST_DATA.Length);
        mc2.Flush();

        // Waiting again
        Thread threadWaiter2 = new Thread(() => {
            while (true) {
                Thread.Sleep(1);
            }
        });
        threadWaiter2.Start();
        threadWaiter2.Join(TimeSpan.FromSeconds(1.0));

        // Checking that both server got data
        Assert.Multiple(() => {
            Assert.That(threadExited1, Is.EqualTo(threadExited2));
            Assert.That(threadExited1, Is.True);
            Assert.That(threadExited2, Is.True);
        });
    }
}
