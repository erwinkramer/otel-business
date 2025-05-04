namespace IntegrationTest;

public class Test
{
    [Before(Class)]
    public static Task CreateContext()
    {
        return Task.CompletedTask;
    }

    [Test]
    public async Task CreateBankReturnsCreated()
    {
        await Program.Main(["true"]); // Pass "true" to simulate an unhandled exception
    }
}
