﻿namespace IntegrationTest;

public class Test
{
    [Before(Class)]
    public static Task CreateContext()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Basic test to ensure that the application runs without exceptions.
    /// </summary>
    [Test]
    public async Task BasicTest()
    {
        try
        {
            await Program.Main(["false"]); // Pass "false" to simulate a happy path
        }
        catch (Exception ex)
        {
            Assert.Fail($"Caught exception: {ex.Message}.");
        }
    }

    /// <summary>
    /// Failing right after completing the "Analyzing {state} Price" span, 
    /// only this span should be (guaranteed to be) exported to Azure Monitor since the other spans did not complete yet.
    /// </summary>
    [Test]
    public async Task FailFastTest()
    {
        try
        {
            await Program.Main(["true"]); // Pass "true" to simulate a fail-fast scenario
        }
        catch (Exception ex)
        {
            Assert.Fail($"Caught exception: {ex.Message}. This shouldn't be happening, it should simulate a disaster without catching exceptions.");
        }
        finally
        {
            Assert.Fail("In finally block. This shouldn't be happening, it should simulate a disaster without graceful dispose.");
        }
    }
}
