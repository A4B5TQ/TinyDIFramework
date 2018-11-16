namespace TinyDIFramework.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TinyDIFramework.Attributes;
    public abstract class AbstractModule : IModule
    {
        private IDictionary<Type, Dictionary<string, Type>> implementations;

        private IDictionary<Type, object> instances;


        protected AbstractModule()
        {
            this.implementations = new Dictionary<Type, Dictionary<string, Type>>();
            this.instances = new Dictionary<Type, object>();
        }

        public abstract void Configure();

        public Type GetMapping(Type someClass, object attribute)
        {
            IDictionary<string, Type> impl = this.implementations[someClass];
          
            Type type = null;

            if (attribute is Inject)
            {
                    if (impl.Count == 1)
                    {
                        type = impl.Values.First();
                    }
                    else
                    {
                        throw new ArgumentException("No available mapping for class: " + someClass.FullName);
                    }           
            }
            else if (attribute is Named)
            {
                Named qualifier = attribute as Named;

                string dependencyName = qualifier.Name;
                return impl[dependencyName];
            }

           

            return type;
        }

        public object GetInstance(Type parameter)
        {
            this.instances.TryGetValue(parameter, out object value);
            return value;
        }

        public void SetInstance(Type implementation, object instance)
        {
            if (!this.instances.ContainsKey(implementation))
            {
                this.instances.Add(implementation, instance);
            }
        }

        protected void CreateMapping<TInter, TImpl>()
        {
            if (!this.implementations.ContainsKey(typeof(TInter)))
            {
                this.implementations[typeof(TInter)] = new Dictionary<string, Type>();
            }

            this.implementations[typeof(TInter)].Add(typeof(TImpl).Name, typeof(TImpl));
        }
    }
}