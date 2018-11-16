namespace TinyDIFramework.Injectors
{
    using System;
    using TinyDIFramework.Modules;
    using TinyDIFramework.Attributes;
    using System.Linq;
    using System.Reflection;
    public class Injector
    {
        private IModule module;

        public Injector(IModule module)
        {
            this.module = module;
        }

        public TClass Inject<TClass>()
        {
            var hasConstructorAttribute = this.CheckForConstructorInjection<TClass>();
            var hasFieldAttribute = this.CheckForFieldInjection<TClass>();

            if (hasConstructorAttribute && hasFieldAttribute)
            {
                throw new ArgumentException("There must be only field or constructor annotated with Inject attribute");
            }

            if (hasConstructorAttribute)
            {
                return this.CreateConstructorInjection<TClass>();
            }
            else if (hasFieldAttribute)
            {
                return this.CreateFieldInjection<TClass>();
            }

            return default(TClass);
        }

        private TClass CreateConstructorInjection<TClass>()
        {
            var desireClass = typeof(TClass);
            if (desireClass == null) return default(TClass);
            var constructors = desireClass.GetConstructors();
            foreach (var constructor in constructors)
            {
                if (!constructor.GetCustomAttributes(typeof(Inject), true).Any()) continue;

                var inject = (Inject)constructor
                    .GetCustomAttributes(typeof(Inject), true)
                    .FirstOrDefault();
                var parameterTypes = constructor.GetParameters();
                var objArr = new object[parameterTypes.Length];

                var i = 0;

                foreach (var parameterType in parameterTypes)
                {
                    var qualifier = parameterType.GetCustomAttribute(typeof(Named));
                    Type dependency = null;

                    if (qualifier == null)
                    {
                        dependency = this.module.GetMapping(parameterType.ParameterType, inject);
                    }
                    else
                    {
                        dependency = this.module.GetMapping(parameterType.ParameterType, qualifier);
                    }



                    this.GetType().GetMethod(nameof(Inject)).MakeGenericMethod(dependency).Invoke(this, null);

                    if (parameterType.ParameterType.IsAssignableFrom(dependency))
                    {

                        object instance = this.module.GetInstance(dependency);
                        if (instance != null)
                        {
                            objArr[i++] = instance;
                        }
                        else
                        {
                            instance = Activator.CreateInstance(dependency);
                            objArr[i++] = instance;
                            this.module.SetInstance(parameterType.ParameterType, instance);
                        }
                    }
                }

                return (TClass)Activator.CreateInstance(desireClass, objArr);
            }

            return default(TClass);
        }

        private TClass CreateFieldInjection<TClass>()
        {
            var desireClass = typeof(TClass);
            var desireClassInstance = this.module.GetInstance(desireClass);

            if (desireClassInstance == null)
            {
                desireClassInstance = Activator.CreateInstance(desireClass);
                this.module.SetInstance(desireClass, desireClassInstance);
            }

            foreach (var field in desireClass.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (field.GetCustomAttributes(typeof(Inject), true).Any())
                {
                    var injection = (Inject)field.GetCustomAttributes(typeof(Inject), true).FirstOrDefault();
                    Type dependency = null;

                    var qualifier = field.GetCustomAttribute(typeof(Named), true);
                    var type = field.FieldType;
                    if (qualifier == null)
                    {
                        dependency = this.module.GetMapping(type, injection);
                    }
                    else
                    {
                        dependency = this.module.GetMapping(type, qualifier);
                    }
                    if (type.IsAssignableFrom(dependency))
                    {
                        object instance = this.module.GetInstance(dependency);
                        if (instance == null)
                        {
                            instance = Activator.CreateInstance(dependency);
                            this.module.SetInstance(dependency, instance);
                        }

                        field.SetValue(desireClassInstance, instance);
                    }



                }
            }

            return (TClass)desireClassInstance;
        }

        private bool CheckForFieldInjection<TClass>()
        {
            return typeof(TClass)
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Any(field => field.GetCustomAttributes(typeof(Inject), true).Any());
        }

        private bool CheckForConstructorInjection<TClass>()
        {
            return typeof(TClass).GetConstructors().Any(
                constructor => constructor.GetCustomAttributes(typeof(Inject), true).Any());
        }
    }
}