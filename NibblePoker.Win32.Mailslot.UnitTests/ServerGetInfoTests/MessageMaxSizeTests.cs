using NUnit.Framework;

namespace NibblePoker.Win32.Mailslot.UnitTests.ServerGetInfoTests;

internal class MessageMaxSizeTests {

    [Test]
    public void TestInitialValues() {
        MailslotServer ms1 = new MailslotServer(".", "bda68567-6399-4715-8839-5e7ba39e6028", 0, 0);
        Assert.Multiple(() => {
            Assert.That(ms1.MailslotHandle.IsInvalid, Is.False);
            Assert.That(ms1.MaxMessageSize, Is.EqualTo(0));
        });

        MailslotServer ms2 = new MailslotServer(".", "7eaa9cb9-9b19-47c1-b38c-526d3e467930", 4096, 0);
        Assert.Multiple(() => {
            Assert.That(ms2.MailslotHandle.IsInvalid, Is.False);
            Assert.That(ms2.MaxMessageSize, Is.EqualTo(4096));
        });
    }

}
