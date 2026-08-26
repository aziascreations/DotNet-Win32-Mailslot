using NUnit.Framework;
using System;

namespace NibblePoker.Win32.Mailslot.UnitTests.ParametersTests;

public class InvalidClientTests {

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

    [Test]
    public void TestNullDomain() {
        MailslotServer ms = new MailslotServer(null, "e5bc2350-4136-4218-a09c-88ce16f59114", 0, 0);

        Assert.DoesNotThrow(() => {
            new MailslotClient(null, "e5bc2350-4136-4218-a09c-88ce16f59114", mustExist: false);
        }, "Null host shouldn't throw");
    }

    [Test]
    public void TestNullPath() {
        Assert.Throws<ArgumentException>(() => {
            new MailslotClient(".", null);
        }, "Null path should throw");
    }

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    [Test]
    public void TestInvalidCharInDomain() {
        Assert.Throws<ArgumentException>(() => {
            new MailslotClient("test\\test", "test");
        }, "Invalid char in host #1");
    }

    [Test]
    public void TestInvalidCharInPath() {
        Assert.Throws<ArgumentException>(() => {
            new MailslotClient(".", "test>test");
        }, "Invalid char in path #1");
    }

}
