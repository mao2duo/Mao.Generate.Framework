using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// 將指定類型的第一個服務替換成自訂方法的回傳值
        /// </summary>
        public static IServiceCollection Replace<TService>(this IServiceCollection services, Func<IServiceProvider, object> factory)
        {
            var serviceType = typeof(TService);
            var descriptor = services.FirstOrDefault(x => x.ServiceType == serviceType);
            if (descriptor == null)
            {
                throw new Exception($"{nameof(IServiceCollection)} 不存在類別 {serviceType.Name}，無法進行取代");
            }
            services.Replace(new ServiceDescriptor(serviceType, factory, descriptor.Lifetime));
            return services;
        }
        /// <summary>
        /// 將指定類型的第一個服務替換成指定的實作類型
        /// </summary>
        public static IServiceCollection Replace<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : TService
        {
            var serviceType = typeof(TService);
            var descriptor = services.FirstOrDefault(x => x.ServiceType == serviceType);
            if (descriptor == null)
            {
                throw new Exception($"{nameof(IServiceCollection)} 不存在類別 {serviceType.Name}，無法進行取代");
            }
            services.Replace(new ServiceDescriptor(
                serviceType,
                provider => (TService)ActivatorUtilities.CreateInstance(provider, typeof(TImplementation)),
                descriptor.Lifetime));
            return services;
        }
        /// <summary>
        /// 將指定類型的原服務作為自訂方法的參數，並將指定類型的第一個服務替換成自訂方法的回傳值
        /// </summary>
        public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, object> factory)
        {
            return services.Decorate<TService>((provider, service) => factory(service));
        }
        /// <summary>
        /// 將指定類型的原服務作為自訂方法的參數，並將指定類型的第一個服務替換成自訂方法的回傳值
        /// </summary>
        public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<IServiceProvider, TService, object> factory)
        {
            var serviceType = typeof(TService);
            var descriptor = services.FirstOrDefault(x => x.ServiceType == serviceType);
            if (descriptor == null)
            {
                throw new Exception($"{nameof(IServiceCollection)} 不存在類別 {serviceType.Name}，無法進行裝飾");
            }
            services.Replace(new ServiceDescriptor(
                serviceType,
                provider =>
                {
                    if (descriptor.ImplementationInstance != null)
                    {
                        return factory(provider, (TService)descriptor.ImplementationInstance);
                    }
                    if (descriptor.ImplementationFactory != null)
                    {
                        return factory(provider, (TService)descriptor.ImplementationFactory.Invoke(provider));
                    }
                    return factory(provider, (TService)ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType));
                },
                descriptor.Lifetime));
            return services;
        }
    }
}
