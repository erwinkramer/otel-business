namespace IntegrationTest;

public class Test
{
    [Before(Class)]
    public static Task CreateContext()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Failing right after completing the "Analyzing {state} Price" span, 
    /// only this span should be (guaranteed to be) exported to Azure Monitor since the other spans did not complete yet.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task FailFastTest()
    {
        try
        {
            await Program.Main(["true"]); // Pass "true" to simulate a fail-fast scenario
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught exception: {ex.Message}. This shouldn't be happening.");
        }
        finally
        {
            Console.WriteLine("In finally block. This shouldn't be happening.");
        }
    }
}
