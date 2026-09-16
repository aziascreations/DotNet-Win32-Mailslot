using NUnit.Framework;
using System;
using System.IO;
using System.Threading;

namespace NibblePoker.Win32.Mailslot.UnitTests.EndToEndTests;

internal class ServerTimeoutChangeTests {

    private static readonly string SERVER_UUID = "36640f89-14fb-449e-8f9a-f6ac60b4f0e1";

    private static readonly byte[] TEST_DATA = [0xDE, 0xAD, 0xBE, 0xEF];

    [Test]
    public void TestServerTimeoutChange() {
        // Preparing the server
        MailslotServer ms = new MailslotServer(
            ".", SERVER_UUID,
            MailslotServer.MAILSLOT_ANY_MESSAGE_SIZE,
            100
        );

        Assert.Multiple(() => {
            Assert.That(ms.MailslotHandle.IsInvalid, Is.False);
            Assert.That(ms.MaxMessageSize, Is.EqualTo(MailslotServer.MAILSLOT_ANY_MESSAGE_SIZE));
        });


        // Ensuring timeouts work properly
        Thread thread1 = new Thread(() => {
            int bytesRead = -1;

            Assert.Throws<IOException>(() => {
                byte[] buffer = new byte[TEST_DATA.Length];
                bytesRead = ms.Read(buffer, 0, buffer.Length);
            }, "Timeouts should raise exceptions");

            Assert.That(bytesRead, Is.EqualTo(-1), "No data read on timeout");
        });
        thread1.Start();

        Assert.That(
            thread1.Join(TimeSpan.FromSeconds(1.0)),
            Is.True,
            "Thread 1 should have exited"
        );


        // Re-checking with data waiting
        MailslotClient mc = new MailslotClient(".", SERVER_UUID);
        mc.Write(TEST_DATA, 0, TEST_DATA.Length);
        mc.Flush();

        Thread thread2 = new Thread(() => {
            int bytesRead = -1;

            Assert.DoesNotThrow(() => {
                byte[] buffer = new byte[TEST_DATA.Length];
                bytesRead = ms.Read(buffer, 0, buffer.Length);
            }, "Should read without errors");

            Assert.That(bytesRead, Is.EqualTo(TEST_DATA.Length));
        });
        thread2.Start();

        Assert.That(
            thread2.Join(TimeSpan.FromSeconds(1.0)),
            Is.True,
            "Thread 2 should have exited"
        );


        // Changing the timeout to be infinite
        ms.SetReadTimeoutMs(MailslotServer.MAILSLOT_WAIT_FOREVER);


        // Ensuring the new timeout work properly
        Thread thread3 = new Thread(() => {
            byte[] buffer = new byte[TEST_DATA.Length];
            int bytesRead = ms.Read(buffer, 0, buffer.Length);

            // Should never hit.
            Assert.Fail("The thread for infinite timeouts is exiting !");
        });
        thread3.Start();

        Assert.That(
            thread3.Join(TimeSpan.FromSeconds(1.0)),
            Is.False,
            "Thread 3 has not exited and was still waiting"
        );
    }
}
