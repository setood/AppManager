using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppManager
{
    public static class Manager
    {
        private static bool isInitialized = false;
        private static bool isRunned = false;
        private static bool isShoutdown = false;
        private static bool isLoaded = false;
        private static IEnumerable<Assembly> Assemblies { get; set; } = new Assembly[] { typeof(IModuleController).Assembly };
        public static CompositionContainer Container { get; private set; }
        #region private methods
        private static List<Assembly> ImportAssemblyFromDirectory(string folderPath)
        {
            // Assembly asm = Assembly.LoadFrom(assemblyFile);
            var files = Directory.EnumerateFiles(folderPath,"*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
            List<Assembly> assemblies = new List<Assembly>();
            foreach (var file in files)
            {
                try
                {
                    Assembly asm = Assembly.LoadFrom(file);
                    if (asm != null)
                    {
                        assemblies.Add(asm);
                    }
                }
                catch (Exception ex)
                {
                }

            }
            return assemblies;
        }
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;

            // check for assemblies already loaded
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            try
            {
                return System.Reflection.Assembly.LoadFrom(args.Name);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static IOrderedEnumerable<IModuleController> GetModules()
        {
            return Container.GetExportedValues<IModuleController>()
                .OrderBy(mc => mc.BootPriority)
                .ThenBy(mc => mc.BootOrder);
        }

        #endregion

        #region public methods
        public static void LoadAssemblies(IEnumerable<Assembly> assemblies)
        {
            if (isLoaded == false)
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                //Assemblies = assemblies;
                Container = new CompositionContainer(CompositionOptions.DisableSilentRejection);
                isLoaded = true;
            }
            Assemblies = Assemblies.Concat(assemblies).Distinct().ToArray();
            AggregateCatalog catalog = new AggregateCatalog();
            foreach (var assembly in Assemblies)
            {
                catalog.Catalogs.Add(new AssemblyCatalog(assembly));
            }
            
            Container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection);
            var batch = new CompositionBatch();
            batch.AddExportedValue(Container);
            Container.Compose(batch);

            return;
        }
        public static void LoadAssemblieFromFolder(string folderPath)
        {

            IEnumerable<Assembly> assemblies = ImportAssemblyFromDirectory(folderPath);
            LoadAssemblies(assemblies);
        }
        public static async Task InitializeAsync()
        {
            if (isInitialized)
            {
                return;
            }
            IOrderedEnumerable<IModuleController> moduleControllers = GetModules();
            foreach (var moduleController in moduleControllers) { await moduleController.InitializeAsync(); }
            isInitialized = true;
        }

        public static async Task RunAsync()
        {
            if (isRunned)
            {
                return;
            }
            var moduleControllers = GetModules();
            foreach (var moduleController in moduleControllers) { await moduleController.RunAsync(); }
            isRunned = true;
        }
        public static async Task ShutdownAsync()
        {
            if (isShoutdown)
            {
                return;
            }
            var moduleControllers = GetModules();
            foreach (var moduleController in moduleControllers) { await moduleController.ShutdownAsync(); }
            isShoutdown = true;
        }
        #endregion

    }
}
