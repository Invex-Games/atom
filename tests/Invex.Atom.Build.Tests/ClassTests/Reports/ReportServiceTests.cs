namespace Invex.Atom.Build.Tests.ClassTests.Reports;

[TestFixture]
internal sealed class ReportServiceTests
{
    [Test]
    public void GetReportData_GroupsByTargetInFirstReportedOrder()
    {
        var service = new ReportService();
        var a1 = new TextReportData("a1");
        var b1 = new TextReportData("b1");
        var a2 = new TextReportData("a2");
        var global = new TextReportData("global");

        service.AddReportData(a1, "A");
        service.AddReportData(b1, "B");
        service.AddReportData(a2, "A");
        service.AddReportData(global);

        service
            .GetReportData()
            .ShouldBe([a1, a2, b1, global]);

        service
            .GetReportData("A")
            .ShouldBe([a1, a2]);

        service
            .GetReportData(null)
            .ShouldBe([global]);

        service
            .GetReportData("Missing")
            .ShouldBeEmpty();
    }

    [Test]
    public void AddReportData_IsThreadSafe()
    {
        var service = new ReportService();

        Parallel.For(0, 1000, i => service.AddReportData(new TextReportData(i.ToString()), $"T{i % 7}"));

        service
            .GetReportData()
            .Count
            .ShouldBe(1000);
    }
}
