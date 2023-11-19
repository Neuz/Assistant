using Assistant.Services;

namespace Assistant.Test;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public async Task TestMethod1()
    {
        var wss = new WinSvcService();

        try
        {
            var rs = await wss.Stop("neuz.rediz");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}