using NUnit.Framework;
using System.IO;

namespace NibblePoker.Win32.Mailslot.UnitTests.ParametersTests;

internal class FailedConnectionTests {

    [Test]
    public void TestNonExistantResourceConnection() {
        const string badHost = "null";
        const string badPath = "34afbb64-2c41-47f6-965e-b0e698f0363d";

        Assert.Throws<IOException>(() => {
            new MailslotClient(badHost, badPath, mustExist: true);
        }, "Non-existant resource with check enabled");

        Assert.DoesNotThrow(() => {
            new MailslotClient(badHost, badPath, mustExist: false);
        }, "Non-existant resource without check enabled");
    }

}
