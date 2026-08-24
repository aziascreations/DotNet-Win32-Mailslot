using NUnit.Framework;
using System;

namespace NibblePoker.Win32.Mailslot.UnitTests.ParametersTests;

public class InvalidClientTests {

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

    [Test]
    public void TestNullDomain() {
        Assert.Throws<ArgumentException>(() => {
            new MailslotClient(null, "test");
        }, "Null host");
    }

    [Test]
    public void TestNullPath() {
        Assert.Throws<ArgumentException>(() => {
            new MailslotClient(".", null);
        }, "Null path");
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
            new MailslotClient(".", "test");
        }, "Invalid char in path #1");
    }

}
