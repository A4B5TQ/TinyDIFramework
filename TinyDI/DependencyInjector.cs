namespace TinyDIFramework
{
    using TinyDIFramework.Injectors;
    using TinyDIFramework.Modules;

    public class DInjector
    {
        public static Injector CreateInjector(IModule module)
        {
            module.Configure();
            return new Injector(module);
        }
    }
}
