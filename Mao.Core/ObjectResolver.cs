using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System
{
    public class ObjectResolver
    {
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
        /// 透過 TypeConverter 轉換物件類型
        /// </summary>
        public static T TypeConvert<T>(object value)
        {
            if (value == null)
            {
                return default(T);
            }
            return (T)TypeConvert(typeof(T), value);
        }
        /// <summary>
        /// 透過 TypeConverter 轉換物件類型
        /// </summary>
        public static object TypeConvert(Type destinationType, object value)
        {
            if (value == null)
            {
                return null;
            }
            var sourceType = value.GetType();
            TypeConverter sourceTypeConverter = TypeDescriptor.GetConverter(sourceType);
            if (sourceTypeConverter != null && sourceTypeConverter.CanConvertTo(destinationType))
            {
                return sourceTypeConverter.ConvertTo(value, destinationType);
            }
            TypeConverter destinationTypeConverter = TypeDescriptor.GetConverter(destinationType);
            if (destinationTypeConverter != null && destinationTypeConverter.CanConvertFrom(sourceType))
            {
                return destinationTypeConverter.ConvertFrom(value);
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
