using FakeItEasy;
using Invex.Atom.Build;

namespace Invex.Atom.Workflows.Tests;

[TestFixture]
internal sealed class BuildCacheTests
{
    private static IBuildAccessor NewKey() =>
        A.Fake<IBuildAccessor>();

    [Test]
    public void TryGetValue_MissingKey_ReturnsFalse()
    {
        var cache = new BuildCache<string>();

        cache
            .TryGetValue(NewKey(), out _)
            .ShouldBeFalse();
    }

    [Test]
    public void Set_ThenTryGetValue_ReturnsValue()
    {
        var cache = new BuildCache<string>();
        var key = NewKey();

        cache
            .Set(key, "value")
            .ShouldBe("value");

        cache
            .TryGetValue(key, out var result)
            .ShouldBeTrue();

        result.ShouldBe("value");
    }

    [Test]
    public void Set_ExistingKey_Overwrites()
    {
        var cache = new BuildCache<string>();
        var key = NewKey();

        cache.Set(key, "a");
        cache.Set(key, "b");

        cache.TryGetValue(key, out var result);
        result.ShouldBe("b");
    }

    [Test]
    public void GetOrAdd_CreatesOnce()
    {
        var cache = new BuildCache<int>();
        var key = NewKey();
        var calls = 0;

        cache
            .GetOrAdd(key, _ => ++calls)
            .ShouldBe(1);

        cache
            .GetOrAdd(key, _ => ++calls)
            .ShouldBe(1);

        calls.ShouldBe(1);
    }

    [Test]
    public void Remove_ExistingKey_RemovesIt()
    {
        var cache = new BuildCache<string>();
        var key = NewKey();
        cache.Set(key, "value");

        cache
            .Remove(key)
            .ShouldBeTrue();

        cache
            .Remove(key)
            .ShouldBeFalse();

        cache
            .TryGetValue(key, out _)
            .ShouldBeFalse();
    }

    [Test]
    public void Clear_RemovesAllEntries()
    {
        var cache = new BuildCache<string>();
        var key1 = NewKey();
        var key2 = NewKey();
        cache.Set(key1, "a");
        cache.Set(key2, "b");

        cache.Clear();

        cache
            .TryGetValue(key1, out _)
            .ShouldBeFalse();

        cache
            .TryGetValue(key2, out _)
            .ShouldBeFalse();
    }
}
