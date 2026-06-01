using AudioMate.Core.Runtime;

namespace AudioMate.Core.Tests;

public sealed class SingleInstanceGuardTests
{
    [Fact]
    public void TryAcquireRejectsSecondOwnerForSameInstanceName()
    {
        var instanceName = $"AudioMate.Tests.{Guid.NewGuid():N}";

        using var firstOwner = SingleInstanceGuard.TryAcquire(instanceName);
        using var secondOwner = SingleInstanceGuard.TryAcquire(instanceName);

        Assert.True(firstOwner.HasOwnership);
        Assert.False(secondOwner.HasOwnership);
    }

    [Fact]
    public void TryAcquireAllowsNewOwnerAfterCurrentOwnerIsDisposed()
    {
        var instanceName = $"AudioMate.Tests.{Guid.NewGuid():N}";

        using (var firstOwner = SingleInstanceGuard.TryAcquire(instanceName))
        {
            Assert.True(firstOwner.HasOwnership);
        }

        using var nextOwner = SingleInstanceGuard.TryAcquire(instanceName);

        Assert.True(nextOwner.HasOwnership);
    }
}
