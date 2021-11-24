using Mao.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Core.Models
{
    public class TypeDescriptorContext<TSource, TDestination> : ITypeDescriptorContext
    {
        private readonly IServiceProvider _serviceProvider;
        public TypeDescriptorContext(IServiceProvider serviceProvider, object instance, string propertyName, TDestination destination)
        {
            _serviceProvider = serviceProvider;
            if (instance != null && !string.IsNullOrEmpty(propertyName))
            {
                PropertyDescriptor = TypeDescriptor.GetProperties(instance)[propertyName];
            }
            Instance = instance;
            Destination = destination;
        }

        public virtual IContainer Container { get; }

        public virtual object Instance { get; }
        public virtual PropertyDescriptor PropertyDescriptor { get; }

        public virtual TDestination Destination { get; }

        public virtual object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public virtual void OnComponentChanged()
        {
        }
        public virtual bool OnComponentChanging()
        {
            return true;
        }
    }
}
