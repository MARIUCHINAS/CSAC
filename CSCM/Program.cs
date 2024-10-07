using CSAC;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Loading ImGUI..");
        

        // Start ImGUI in difrent thread
        Renderer renderer = new Renderer();
        Thread renderThread = new Thread(renderer.Start().Wait);
        renderThread.Start();
        
    }
}