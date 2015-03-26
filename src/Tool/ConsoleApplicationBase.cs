using System;
using System.IO;
using System.Linq;
using Umbraco.Core;

namespace Pihalve.Tdu.Tool
{
    /// <summary>
    /// Extends the UmbracoApplicationBase, which is needed to start the application with out own BootManager.
    /// </summary>
    public class ConsoleApplicationBase : UmbracoApplicationBase
    {
        //public string BaseDirectory { get; private set; }
        public string DataDirectory { get; private set; }

        protected override IBootManager GetBootManager()
        {
            var binDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); //@"D:\Projects\SANDBOX\Pihalve.Tdu\Pihalve.Tdu.TestSite\bin"
            var baseDirectory = ResolveBasePath(binDirectory);
            DataDirectory = Path.Combine(baseDirectory, "app_data");
            var appDomainConfigPath = new DirectoryInfo(Path.Combine(binDirectory.FullName, "config"));

            //Copy config files to AppDomain's base directory
            if (binDirectory.FullName.Equals(baseDirectory) == false &&
                appDomainConfigPath.Exists == false)
            {
                appDomainConfigPath.Create();
                var baseConfigPath = new DirectoryInfo(Path.Combine(baseDirectory, "config"));
                var sourceFiles = baseConfigPath.GetFiles("*.config", SearchOption.TopDirectoryOnly);
                foreach (var sourceFile in sourceFiles)
                {
                    sourceFile.CopyTo(sourceFile.FullName.Replace(baseConfigPath.FullName, appDomainConfigPath.FullName), true);
                }
            }

            ////Copy web.config to app.config of Tool exe
            //var webConfig = new FileInfo(Path.Combine(baseDirectory, "web.config"));
            //if (webConfig.Exists)
            //{
            //    webConfig.CopyTo(Path.Combine(binDirectory.FullName, Assembly.GetExecutingAssembly().GetName().Name + ".exe.config"));
            //}

            AppDomain.CurrentDomain.SetData("DataDirectory", DataDirectory);

            return new ConsoleBootManager(this, baseDirectory);
        }

        public void Start(object sender, EventArgs e)
        {
            base.Application_Start(sender, e);
        }

        private string ResolveBasePath(DirectoryInfo currentFolder)
        {
            var folders = currentFolder.GetDirectories();
            if (folders.Any(x => x.Name.Equals("app_data", StringComparison.OrdinalIgnoreCase)) &&
                folders.Any(x => x.Name.Equals("config", StringComparison.OrdinalIgnoreCase)))
            {
                return currentFolder.FullName;
            }

            if (currentFolder.Parent == null)
                throw new Exception("Base directory containing an 'App_Data' and 'Config' folder was not found." +
                    " These folders are required to run this console application as it relies on the normal umbraco configuration files.");

            return ResolveBasePath(currentFolder.Parent);
        }
    }
}
