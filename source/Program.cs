namespace JarJarLauncher;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            
            // Load and validate configuration on startup
            var config = ShardConfig.Load();
            var errors = config.Validate();
            
            if (errors.Count > 0)
            {
                var errorMessage = "Configuration errors found in shard_config.json:\n\n" + 
                                 string.Join("\n", errors) + 
                                 "\n\nThe launcher may not work correctly. Please fix the configuration file.";
                
                MessageBox.Show(
                    errorMessage,
                    "Configuration Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Fatal error starting JarJar Launcher:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "Fatal Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
