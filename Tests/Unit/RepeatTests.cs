using System;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class RepeatTests
{
    // TryMethod (void)

    [TestMethod]
    public void TryMethod_SucceedsFirstTry_CallsOnce()
    {
        int calls = 0;

        Repeat.TryMethod(() => calls++, maxTries: 3, waitTime: TimeSpan.Zero);

        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public void TryMethod_FailsThenSucceeds_RetriesAndSucceeds()
    {
        int calls = 0;

        Repeat.TryMethod(() =>
        {
            calls++;
            if (calls < 3)
            {
                throw new InvalidOperationException("not yet");
            }
        }, maxTries: 5, waitTime: TimeSpan.Zero);

        Assert.AreEqual(3, calls);
    }

    [TestMethod]
    public void TryMethod_AlwaysFails_RethrowsLastException()
    {
        int calls = 0;

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            Repeat.TryMethod(() =>
            {
                calls++;
                throw new InvalidOperationException("always");
            }, maxTries: 3, waitTime: TimeSpan.Zero));

        Assert.AreEqual(3, calls);
    }

    // TryMethod<TResult>

    [TestMethod]
    public void TryMethodResult_SucceedsFirstTry_ReturnsResult()
    {
        int result = Repeat.TryMethod(() => 42, maxTries: 3, waitTime: TimeSpan.Zero);

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void TryMethodResult_FailsThenSucceeds_ReturnsResult()
    {
        int calls = 0;

        int result = Repeat.TryMethod(() =>
        {
            calls++;
            if (calls < 2)
            {
                throw new InvalidOperationException();
            }
            return 99;
        }, maxTries: 3, waitTime: TimeSpan.Zero);

        Assert.AreEqual(99, result);
    }

    [TestMethod]
    public void TryMethodResult_AlwaysFails_RethrowsLastException()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            Repeat.TryMethod<int>(() => throw new InvalidOperationException("always"),
                maxTries: 3, waitTime: TimeSpan.Zero));
    }

    // CallMethod

    [TestMethod]
    public void CallMethod_ThreeTimes_CallsThreeTimes()
    {
        int count = 0;

        Repeat.CallMethod(() => count++, times: 3);

        Assert.AreEqual(3, count);
    }

    [TestMethod]
    public void CallMethod_ZeroTimes_NeverCalls()
    {
        int count = 0;

        Repeat.CallMethod(() => count++, times: 0);

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void CallMethod_NegativeTimes_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Repeat.CallMethod(() => { }, times: -1));
    }
}
