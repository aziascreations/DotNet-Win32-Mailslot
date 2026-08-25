using NUnit.Framework;

namespace NibblePoker.Win32.Mailslot.UnitTests.ServerGetInfoTests;

internal class ReadTimeoutTests {

    [Test]
    public void TestValueChangeViaProperties() {
        MailslotServer ms = new MailslotServer(".", "99790723-b102-4bee-9f4b-aeaa1e9355a4", 0, 0);
        Assert.That(ms.ReadTimeoutMs, Is.EqualTo(0));

        ms.ReadTimeoutMs = 42;
        Assert.That(ms.ReadTimeoutMs, Is.EqualTo(42));
    }

    [Test]
    public void TestValueChangeViaWinApi() {
        MailslotServer ms = new MailslotServer(".", "4deaf0be-4d75-48a7-9bb1-c1558f3992f8", 0, 0);
        Assert.That(ms.ReadTimeoutMs, Is.EqualTo(0));

        MailslotBindings.SetMailslotInfo(ms.MailslotHandle, 42);
        Assert.That(ms.ReadTimeoutMs, Is.EqualTo(42));
    }

}
