using Terminal.Gui;

class Program 
{
    static void Main()
    {
        Application.Init();
        MainWindow mw = new MainWindow();
        Application.AddTimeout(TimeSpan.FromMilliseconds(1000), mw.Update);
        Application.Run(mw);
        Application.Shutdown();
    }
}
