using Mao.Core.Interfaces;
using Mao.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System
{
    public class ObjectResolver
    {
        /// <summary>
        /// 注入服務的接口
        /// </summary>
        public static IDependencyResolver DependencyResolver { get; set; }

        private static T GetDefaultValue<T>()
        {
            return default(T);
        }
        /// <summary>
        /// 取得類型的初始值
        /// </summary>
        public static object GetDefaultValue(Type type)
        {
            return typeof(ObjectResolver)
                .GetMethod(nameof(GetDefaultValue), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(type)
                .Invoke(null, null);
        }
        /// <summary>
        /// 透過 TypeConverter 將類型 TSource 的物件轉換成類型 TDestination
        /// </summary>
        /// <param name="instance">來源的所有者</param>
        /// <param name="sourcePropertyName">存放來源的屬性名稱</param>
        /// <param name="destination">已存在的目標參照物件</param>
        public static TDestination TypeConvert<TInstance, TSource, TDestination>(TInstance instance, string sourcePropertyName, TDestination destination = default(TDestination))
        {
            IServiceProvider serviceProvider = null;
            if (DependencyResolver != null)
            {
                serviceProvider = (IServiceProvider)DependencyResolver.GetService(typeof(IServiceProvider));
            }
            TypeDescriptorContext<TSource, TDestination> context = new TypeDescriptorContext<TSource, TDestination>(serviceProvider, instance, sourcePropertyName, destination);
            TSource source = (TSource)context.PropertyDescriptor.GetValue(instance);
            return TypeConvert(context, source);
        }
        /// <summary>
        /// 透過 TypeConverter 將類型 TSource 的物件轉換成類型 TDestination
        /// </summary>
        /// <param name="instance">來源的所有者</param>
        /// <param name="source">來源</param>
        /// <param name="destination">已存在的目標參照物件</param>
        public static TDestination TypeConvert<TInstance, TSource, TDestination>(TInstance instance, TSource source, TDestination destination = default(TDestination))
        {
            IServiceProvider serviceProvider = null;
            if (DependencyResolver != null)
            {
                serviceProvider = (IServiceProvider)DependencyResolver.GetService(typeof(IServiceProvider));
            }
            TypeDescriptorContext<TSource, TDestination> context = new TypeDescriptorContext<TSource, TDestination>(serviceProvider, instance, null, destination);
            return TypeConvert(context, source);
        }
        /// <summary>
        /// 透過 TypeConverter 將類型 TSource 的物件轉換成類型 TDestination
        /// </summary>
        public static TDestination TypeConvert<TSource, TDestination>(TSource source, TDestination destination = default(TDestination))
        {
            IServiceProvider serviceProvider = null;
            if (DependencyResolver != null)
            {
                serviceProvider = (IServiceProvider)DependencyResolver.GetService(typeof(IServiceProvider));
            }
            TypeDescriptorContext<TSource, TDestination> context = new TypeDescriptorContext<TSource, TDestination>(serviceProvider, null, null, destination);
            return TypeConvert(context, source);
        }
        /// <summary>
        /// 透過 TypeConverter 將類型 TSource 的物件轉換成類型 TDestination
        /// </summary>
        public static TDestination TypeConvert<TSource, TDestination>(TypeDescriptorContext<TSource, TDestination> context, TSource source)
        {
            Type sourceType = typeof(TSource);
            Type destinationType = typeof(TDestination);
            TypeConverter sourceTypeConverter = TypeDescriptor.GetConverter(sourceType);
            if (sourceTypeConverter != null && sourceTypeConverter.CanConvertTo(context, destinationType))
            {
                return (TDestination)sourceTypeConverter.ConvertTo(context, CultureInfo.CurrentCulture, source, destinationType);
            }
            TypeConverter destinationTypeConverter = TypeDescriptor.GetConverter(destinationType);
            if (destinationTypeConverter != null && destinationTypeConverter.CanConvertFrom(context, sourceType))
            {
                return (TDestination)destinationTypeConverter.ConvertFrom(context, CultureInfo.CurrentCulture, source);
            }
            throw new NotImplementedException($"沒有找到 TypeConverter 可以將 {sourceType.Name} 轉換成 {destinationType.Name}");
        }

        /// <summary>
        /// 從特定物件中尋找可以轉換類別的方法進行轉換
        /// </summary>
        /// <param name="provider">提供轉換方法的物件</param>
        /// <param name="source">轉換前的值</param>
        /// <param name="type">要轉換的類別</param>
        /// <param name="destination">轉換成功時的值</param>
        /// <param name="allowNull">轉換過程中如果得到 null 的結果是否視為成功</param>
        /// <returns>是否轉換成功</returns>
        public static bool TryConvert(object provider, object source, Type type, out object destination, bool allowNull = false)
        {
            var converts = provider.GetType().GetMethods()
                .Where(x => type.IsAssignableFrom(x.ReturnType))
                .Where(x => x.GetParameters().Length == 1)
                .ToArray();
            {
                var convert = converts.FirstOrDefault(x => x.GetParameters()[0].ParameterType.IsAssignableFrom(source.GetType()));
                if (convert != null)
                {
                    destination = convert.Invoke(provider, new object[] { source });
                    return destination != null || allowNull;
                }
            }
            foreach (var convert in converts)
            {
                if (TryConvert(provider, source, convert.GetParameters()[0].ParameterType, out object repeater, allowNull))
                {
                    destination = convert.Invoke(provider, new object[] { repeater });
                    return destination != null || allowNull;
                }
            }
            destination = null;
            return false;
        }
        /// <summary>
        /// 從特定物件中尋找可以轉換類別的方法進行轉換
        /// </summary>
        /// <typeparam name="T">要轉換的類別</typeparam>
        /// <param name="provider">提供轉換方法的物件</param>
        /// <param name="source">轉換前的值</param>
        /// <param name="destination">轉換成功時的值</param>
        /// <param name="allowNull">轉換過程中如果得到 null 的結果是否視為成功</param>
        /// <returns>是否轉換成功</returns>
        public static bool TryConvert<T>(object provider, object source, out T destination, bool allowNull = false)
        {
            if (TryConvert(provider, source, typeof(T), out object repeater, allowNull))
            {
                destination = (T)repeater;
                return true;
            }
            destination = default(T);
            return false;
        }
    }
}
