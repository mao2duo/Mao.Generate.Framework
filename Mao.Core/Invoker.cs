using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public class Invoker
    {
        /// <summary>
        /// 將運算結果作為方法的參數執行
        /// </summary>
        public static void Using<TParameter>(TParameter parameter, Action<TParameter> resolve)
        {
            resolve.Invoke(parameter);
        }
        /// <summary>
        /// 將運算結果作為方法的參數執行
        /// </summary>
        public static TReturn Using<TParameter, TReturn>(TParameter parameter, Func<TParameter, TReturn> resolve)
        {
            return resolve.Invoke(parameter);
        }

        /// <summary>
        /// 將符合條件的運算結果作為方法的參數執行
        /// </summary>
        public static void UsingIf<TParameter>(TParameter parameter, Func<TParameter, bool> filter, Action<TParameter> resolve, Action @else = null)
        {
            if (filter.Invoke(parameter))
            {
                resolve.Invoke(parameter);
            }
            else if (@else != null)
            {
                @else.Invoke();
            }
        }
        /// <summary>
        /// 將符合條件的運算結果作為方法的參數執行
        /// </summary>
        public static void UsingIf<TParameter>(TParameter parameter, Func<TParameter, bool> filter, Action<TParameter> resolve, Action<TParameter> @else)
        {
            if (filter.Invoke(parameter))
            {
                resolve.Invoke(parameter);
            }
            else if (@else != null)
            {
                @else.Invoke(parameter);
            }
        }
        /// <summary>
        /// 將符合條件的運算結果作為方法的參數執行
        /// </summary>
        public static TReturn UsingIf<TParameter, TReturn>(TParameter parameter, Func<TParameter, bool> filter, Func<TParameter, TReturn> resolve, Func<TReturn> @else = null)
        {
            if (filter.Invoke(parameter))
            {
                return resolve.Invoke(parameter);
            }
            if (@else != null)
            {
                return @else.Invoke();
            }
            return default(TReturn);
        }
        /// <summary>
        /// 將符合條件的運算結果作為方法的參數執行
        /// </summary>
        public static TReturn UsingIf<TParameter, TReturn>(TParameter parameter, Func<TParameter, bool> filter, Func<TParameter, TReturn> resolve, Func<TParameter, TReturn> @else)
        {
            if (filter.Invoke(parameter))
            {
                return resolve.Invoke(parameter);
            }
            if (@else != null)
            {
                return @else.Invoke(parameter);
            }
            return default(TReturn);
        }

        /// <summary>
        /// 依照條件是否滿足執行方法 1 或方法 2
        /// </summary>
        public static void If(bool filter, Action resolve, Action @else = null)
        {
            if (filter)
            {
                resolve.Invoke();
            }
            else if (@else != null)
            {
                @else.Invoke();
            }
        }
        /// <summary>
        /// 依照條件是否滿足執行方法 1 或方法 2
        /// </summary>
        public static TReturn If<TReturn>(bool filter, Func<TReturn> resolve, Func<TReturn> @else = null)
        {
            if (filter)
            {
                return resolve.Invoke();
            }
            if (@else != null)
            {
                return @else.Invoke();
            }
            return default(TReturn);
        }

        /// <summary>
        /// 執行方法 1 如果發生異常則執行方法 2
        /// </summary>
        public static void Try(Action resolve, Action @catch = null)
        {
            try
            {
                resolve.Invoke();
            }
            catch
            {
                @catch?.Invoke();
            }
        }
        /// <summary>
        /// 執行方法 1 如果發生異常則執行方法 2
        /// </summary>
        public static void Try(Action resolve, Action<Exception> @catch)
        {
            try
            {
                resolve.Invoke();
            }
            catch (Exception e)
            {
                @catch?.Invoke(e);
            }
        }
        /// <summary>
        /// 執行方法 1 如果發生異常則執行方法 2
        /// </summary>
        public static void Try<TException>(Action resolve, Action<TException> @catch)
            where TException : Exception
        {
            try
            {
                resolve.Invoke();
            }
            catch (TException e)
            {
                @catch?.Invoke(e);
            }
        }
        /// <summary>
        /// 執行方法 1 如果發生異常則執行方法 2
        /// </summary>
        public static TReturn Try<TReturn>(Func<TReturn> resolve, Func<TReturn> @catch = null)
        {
            try
            {
                return resolve.Invoke();
            }
            catch
            {
                if (@catch != null)
                {
                    return @catch.Invoke();
                }
                return default(TReturn);
            }
        }
        /// <summary>
        /// 執行方法 1 如果發生異常則執行方法 2
        /// </summary>
        public static TReturn Try<TReturn>(Func<TReturn> resolve, Func<Exception, TReturn> @catch)
        {
            try
            {
                return resolve.Invoke();
            }
            catch (Exception e)
            {
                if (@catch != null)
                {
                    return @catch.Invoke(e);
                }
                return default(TReturn);
            }
        }
        /// <summary>
        /// 執行方法 1 如果發生異常則執行方法 2
        /// </summary>
        public static TReturn Try<TException, TReturn>(Func<TReturn> resolve, Func<TException, TReturn> @catch)
            where TException : Exception
        {
            try
            {
                return resolve();
            }
            catch (TException e)
            {
                if (@catch != null)
                {
                    return @catch.Invoke(e);
                }
                return default(TReturn);
            }
        }

        /// <summary>
        /// 從多個方法尋找第一個符合條件的結果
        /// </summary>
        public static TReturn Case<TReturn>(Func<TReturn, bool> filter, IEnumerable<Func<TReturn>> factories, Func<TReturn> @default, out int index)
        {
            if (factories != null)
            {
                index = 0;
                foreach (var factory in factories)
                {
                    var current = factory.Invoke();
                    if (filter.Invoke(current))
                    {
                        return current;
                    }
                    index++;
                }
            }
            index = -1;
            if (@default != null)
            {
                return @default.Invoke();
            }
            return default(TReturn);
        }
        /// <summary>
        /// 從多個方法尋找第一個符合條件的結果
        /// </summary>
        public static TReturn Case<TReturn>(Func<TReturn, bool> filter, IEnumerable<Func<TReturn>> factories, Func<TReturn> @default = null)
        {
            return Case(filter, factories, null, out _);
        }
        /// <summary>
        /// 從多個方法尋找第一個符合條件的結果
        /// </summary>
        public static TReturn Case<TReturn>(Func<TReturn, bool> filter, IEnumerable<Func<TReturn>> factories, out int index)
        {
            return Case(filter, factories, null, out index);
        }
    }
}
