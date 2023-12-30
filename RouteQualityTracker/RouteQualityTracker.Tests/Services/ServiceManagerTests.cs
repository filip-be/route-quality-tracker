using FluentAssertions;
using NUnit.Framework;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Tests.Services;

public class ServiceManagerTests
{
    private IServiceManager _serviceManager;

    [SetUp]
    public void SetUp()
    {
        _serviceManager = new ServiceManager();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SetStatus_SetsRunningStatus(bool isRunning)
    {
        _serviceManager.SetStatus(isRunning);

        (_serviceManager as ServiceManager)!.IsServiceRunning.Should().Be(isRunning);
    }

    [Test]
    public void SetStatus_True_InvokesStartedEvent()
    {
        var hasBeenCalled = false;
        _serviceManager.OnServiceStarted += (_, _) => { hasBeenCalled = true; };

        _serviceManager.SetStatus(true);

        hasBeenCalled.Should().BeTrue();
    }

    [Test]
    public void SetStatus_False_InvokesStoppedEvent()
    {
        var hasBeenCalled = false;
        _serviceManager.OnServiceStopped += (_, _) => { hasBeenCalled = true; };

        _serviceManager.SetStatus(false);

        hasBeenCalled.Should().BeTrue();
    }

    [Test]
    public void SetStatus_FalseWithException_InvokesStartedWithErrorEvent()
    {
        var hasBeenCalled = false;
        _serviceManager.OnServiceStartError += (_, _) => { hasBeenCalled = true; };

        _serviceManager.SetStatus(false, new Exception());

        hasBeenCalled.Should().BeTrue();
    }

    [Test]
    public void ToggleService_InvokesOnServiceStart_Event()
    {
        var hasBeenCalled = false;
        _serviceManager.OnServiceStart += (_, _) => { hasBeenCalled = true; };

        _serviceManager.ToggleService();

        hasBeenCalled.Should().BeTrue();
    }

    [Test]
    public void ToggleService_InvokesOnServiceStop_Event_When_ServiceIsAlreadyRunning()
    {
        var hasBeenCalled = false;
        _serviceManager.OnServiceStop += (_, _) => { hasBeenCalled = true; };
        _serviceManager.SetStatus(true);

        _serviceManager.ToggleService();

        hasBeenCalled.Should().BeTrue();
    }
}
