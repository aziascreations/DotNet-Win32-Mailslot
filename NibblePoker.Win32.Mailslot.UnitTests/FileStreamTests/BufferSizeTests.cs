using NUnit.Framework;
using System;
using System.IO;

namespace NibblePoker.Win32.Mailslot.UnitTests.FileStreamTests;

public class BufferSizeTests {

    [Test]
    public void TestServerNonPositiveSize() {
        // We're kind of testing .NET's internal behaviour here, but I've had
        //  strange issues related to buffer sizes during development.

        Assert.Throws<ArgumentOutOfRangeException>(() => {
            FileStream fs = MailslotServer.CreateAsFileStream(
                null, "1a31ee34-0eea-40a4-80e5-94e779104deb", 0, 0,
                bufferSize: 0
            );
        }, "With individual path parts");

        Assert.Throws<ArgumentOutOfRangeException>(() => {
            FileStream fs = MailslotServer.CreateAsFileStream(
                null, "c81cd196-7e03-42e1-ae72-97dfcd963879", 0, 0,
                bufferSize: 0
            );
        }, "With full UNC path");
    }

    [Test]
    public void TestClientNonPositiveSize() {
        // TODO: Implement this !
    }
}
