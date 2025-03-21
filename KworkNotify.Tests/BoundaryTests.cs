using System.Text.RegularExpressions;
using KworkNotify.Core;
using KworkNotify.Core.Service;

namespace KworkNotify.Tests;

[TestFixture]
public class BoundaryTests
{
    private Boundary _boundary;

    [SetUp]
    public void Setup()
    {
        _boundary = new Boundary();
    }
    
    [Test]
    public void GetBoundaryHeader_ReturnsBoundaryWithoutFirstTwoChars()
    {
        Assert.That(_boundary.BoundaryBody, Is.Not.Null);
        Assert.That(_boundary.BoundaryBody, Does.StartWith("------"));
        Assert.That(_boundary.BoundaryBody, Does.Contain("WebKitFormBoundary"));
        Assert.That(_boundary.BoundaryBody, Has.Length.GreaterThan(16));
    }
    
    [Test]
    public void GetBoundaryData_ReturnsCorrectFormat()
    {
        const int pageNumber = 5;
        var result = _boundary.GetBoundaryData(pageNumber);
    
        var lines = result.Split(["\r\n"], StringSplitOptions.None);
    
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(lines, Has.Length.EqualTo(9));
        });
    
        Assert.Multiple(() =>
        {
            Assert.That(lines[0], Is.EqualTo(_boundary.BoundaryBody));
            Assert.That(lines[1], Is.EqualTo("Content-Disposition: form-data; name=\"a\""));
            Assert.That(lines[2], Is.Empty);
            Assert.That(lines[3], Is.EqualTo("1"));
            Assert.That(lines[4], Is.EqualTo(_boundary.BoundaryBody));
            Assert.That(lines[5], Is.EqualTo("Content-Disposition: form-data; name=\"page\""));
            Assert.That(lines[6], Is.Empty);
            Assert.That(lines[7], Is.EqualTo(pageNumber.ToString()));
            Assert.That(lines[8], Is.EqualTo($"{_boundary.BoundaryBody}--"));
        });
    }
    
    [Test]
    public void GetBoundaryData_DifferentPageNumbers_ReturnsDifferentValues()
    {
        const int page1 = 1;
        const int page2 = 2;
    
        var result1 = _boundary.GetBoundaryData(page1);
        var result2 = _boundary.GetBoundaryData(page2);
    
        Assert.That(result1, Is.Not.EqualTo(result2));
        Assert.Multiple(() =>
        {
            Assert.That(result1, Does.Contain(page1.ToString()));
            Assert.That(result2, Does.Contain(page2.ToString()));
        });
    }
}