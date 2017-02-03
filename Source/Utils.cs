using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ZhentarTweaks
{
	public class Utils
	{
		public static Func<TValue> GetStaticFieldAccessor<TObject, TValue>(string fieldName)
		{
			var fieldInfo = typeof(TObject).GetField(fieldName, Detours.UniversalBindingFlags);
			var member = Expression.Field(null, fieldInfo);
			var lambda = Expression.Lambda(typeof(Func<TValue>), member);
			var compiled = (Func<TValue>)lambda.Compile();
			return compiled;
		}
		
		public static Func<TValue> GetStaticFieldAccessor<TValue>(Type tObject, string fieldName)
		{
			var fieldInfo = tObject.GetField(fieldName, Detours.UniversalBindingFlags);
			var member = Expression.Field(null, fieldInfo);
			var lambda = Expression.Lambda(typeof(Func<TValue>), member);
			var compiled = (Func<TValue>)lambda.Compile();
			return compiled;
		}

		public static Func<TObject, TValue> GetFieldAccessor<TObject, TValue>(string fieldName)
		{
			var param = Expression.Parameter(typeof(TObject), "arg");
			var member = Expression.Field(param, fieldName);
			var lambda = Expression.Lambda(typeof(Func<TObject, TValue>), member, param);
			var compiled = (Func<TObject, TValue>)lambda.Compile();
			return compiled;
		}

		public static Func<object, TValue> GetFieldAccessorNoInherit<TObject, TValue>(string fieldName)
		{
			var param = Expression.Parameter(typeof(object), "arg");
			var castParam = Expression.Convert(param, typeof(TObject));
			var member = Expression.Field(castParam, fieldName);
			var lambda = Expression.Lambda(typeof(Func<object, TValue>), member, param);
			var compiled = (Func<object, TValue>)lambda.Compile();
			return compiled;
		}

		public static Action<TObject, TArgs1, TArgs2> GetMethodInvoker<TObject, TArgs1, TArgs2>(string methodName)
		{
			var methodInfo = typeof(TObject).GetMethod(methodName, Detours.UniversalBindingFlags, null, new[] { typeof(TArgs1), typeof(TArgs2) }, null);

			var param = Expression.Parameter(typeof(TObject), "thisArg");

			var argParams = new[]
				{ Expression.Parameter(typeof(TArgs1), "arg1"), Expression.Parameter(typeof(TArgs2), "arg2")};

			var call = Expression.Call(param, methodInfo, argParams);

			var lambda = Expression.Lambda(typeof(Action<TObject, TArgs1, TArgs2>), call, param, argParams[0], argParams[1]);
			var compiled = (Action<TObject, TArgs1, TArgs2>)lambda.Compile();
			return compiled;
		}

		public static Action<TObject, TArgs1> GetMethodInvoker<TObject, TArgs1>(string methodName)
		{
			var methodInfo = typeof(TObject).GetMethod(methodName, Detours.UniversalBindingFlags, null, new[] { typeof(TArgs1) }, null);

			var param = Expression.Parameter(typeof(TObject), "thisArg");

			var argParams = new[] { Expression.Parameter(typeof(TArgs1), "arg1") };

			var call = Expression.Call(param, methodInfo, argParams);

			var lambda = Expression.Lambda(typeof(Action<TObject, TArgs1>), call, param, argParams[0]);
			var compiled = (Action<TObject, TArgs1>)lambda.Compile();
			return compiled;

		}

		public static Action<TObject> GetMethodInvoker<TObject>(string methodName)
		{
			var methodInfo = typeof(TObject).GetMethod(methodName, Detours.UniversalBindingFlags, null, new Type[] { }, null);

			var param = Expression.Parameter(typeof(TObject), "thisArg");
			

			var call = Expression.Call(param, methodInfo);

			var lambda = Expression.Lambda(typeof(Action<TObject>), call, param);
			var compiled = (Action<TObject>)lambda.Compile();
			return compiled;
		}

		public static Action GetStaticMethodInvoker(Type type, string methodName)
		{
			var methodInfo = type.GetMethod(methodName, Detours.UniversalBindingFlags, null, new Type[] { }, null);

			var call = Expression.Call(null, methodInfo);

			var lambda = Expression.Lambda(typeof(Action), call);
			var compiled = (Action)lambda.Compile();
			return compiled;
		}

	}
}
