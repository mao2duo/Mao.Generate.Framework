using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    }
}
