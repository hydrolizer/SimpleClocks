using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleClocks.Utils.Extensions
{
	public enum MemberType
	{
		Method,
		Property,
		Field
	}

	public static class ExpressionEx
	{
		public static TDelegate InstanceMethodInvoke<TDelegate>(this MethodInfo methodInfo)
			where TDelegate : class
		{
			var delegateType = typeof(TDelegate);
			if (!delegateType.IsSubclassOf(typeof(Delegate)))
				throw new InvalidOperationException(delegateType.Name + " is not a delegate type");
			var genericArgs = delegateType.GenericTypeArguments;
			if (!genericArgs.Any())
				throw new InvalidOperationException($"Type {delegateType} has no generic parameter");
			return methodInfo.InstanceMethodInvoke<TDelegate>(genericArgs.First());
		}

		public static TDelegate InstanceMethodInvoke<TDelegate>(string methodName, bool throwOnNotfound = true)
			where TDelegate : class
		{
			var delegateType = typeof(TDelegate);
			if (!delegateType.IsSubclassOf(typeof(Delegate)))
				throw new InvalidOperationException(delegateType.Name + " is not a delegate type");
			var genericArgs = delegateType.GenericTypeArguments;
			if (!genericArgs.Any())
				throw new InvalidOperationException($"Type {delegateType} has no generic parameter");
			return InstanceMethodInvoke<TDelegate>(methodName, genericArgs.First(), throwOnNotfound);
		}

		public static TDelegate InstanceMethodInvoke<TOwner, TDelegate>(string methodName, bool throwOnNotfound = true)
			where TDelegate : class
			=> InstanceMethodInvoke<TDelegate>(methodName, typeof(TOwner), throwOnNotfound);

		public static TDelegate InstanceMethodInvoke<TDelegate>(string methodName, Type ownerType, bool throwOnNotfound = true)
			where TDelegate : class
		{
			if (!typeof(TDelegate).IsSubclassOf(typeof(Delegate)))
				throw new InvalidOperationException(typeof(TDelegate).Name + " is not a delegate type");
			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentNullException(nameof(methodName));
			var methodInfo = (MethodInfo) ownerType.FindInstace(MemberType.Method, methodName);
			if (methodInfo != null)
				return methodInfo.InstanceMethodInvoke<TDelegate>(ownerType);
			if (throwOnNotfound)
				throw new InvalidOperationException($"Method {methodName} of type {ownerType} notfound");
			return null;
		}

		public static TDelegate InstanceMethodInvoke<TOwner, TDelegate>(this MethodInfo methodInfo)
			=> methodInfo.InstanceMethodInvoke<TDelegate>(typeof(TOwner));

		public static TDelegate InstanceMethodInvoke<TDelegate>(this MethodInfo methodInfo, Type ownerType, Type parameterType = null)
		{
			if (!typeof(TDelegate).IsSubclassOf(typeof(Delegate)))
				throw new InvalidOperationException(typeof(TDelegate).Name + " is not a delegate type");
			if (methodInfo==null)
				throw new ArgumentNullException(nameof(methodInfo));
			var objParameterExpr = Expression.Parameter(parameterType ?? ownerType, "instance");
			var instanceExpr = Expression.TypeAs(objParameterExpr, ownerType);
			var @params = methodInfo.GetParameters()
				.Select(pi => Expression.Parameter(pi.ParameterType, pi.Name)).ToList();
			var call = Expression.Call(instanceExpr, methodInfo, @params);
			return Expression.Lambda<TDelegate>(call, new []{objParameterExpr}.Concat(@params)).Compile();
		}

		public static TDelegate StaticMethodInvoke<TOwner, TDelegate>(string methodName, bool throwOnNotfound = true)
			where TDelegate : class
			=> StaticMethodInvoke<TDelegate>(methodName, typeof(TOwner), throwOnNotfound);

		public static TDelegate StaticMethodInvoke<TDelegate>(string methodName, Type ownerType, bool throwOnNotfound = true)
			where TDelegate : class
		{
			if (!typeof(TDelegate).IsSubclassOf(typeof(Delegate)))
				throw new InvalidOperationException(typeof(TDelegate).Name + " is not a delegate type");
			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentNullException(nameof(methodName));
			var methodInfo = (MethodInfo) ownerType.FindStatic(MemberType.Method, methodName);
			if (methodInfo != null)
				return methodInfo.StaticMethodInvoke<TDelegate>();
			if (throwOnNotfound)
				throw new InvalidOperationException($"Method {methodName} of type {ownerType} notfound");
			return null;
		}

		public static TDelegate StaticMethodInvoke<TDelegate>(this MethodInfo methodInfo)
			where TDelegate: class
		{
			if (!typeof(TDelegate).IsSubclassOf(typeof(Delegate)))
				throw new InvalidOperationException(typeof(TDelegate).Name + " is not a delegate type");
			if (methodInfo==null)
				throw new ArgumentNullException(nameof(methodInfo));
			var @params = methodInfo.GetParameters()
				.Select(pi => Expression.Parameter(pi.ParameterType, pi.Name)).ToList();
			var call = Expression.Call(methodInfo, @params);
			return Expression.Lambda<TDelegate>(call, @params).Compile();
		}

		public static Func<TOwner, TValue> PropertyGet<TOwner, TValue>(string propertyName, bool throwOnNotfound = true)
		{
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentNullException(nameof(propertyName));
			var ownerType = typeof(TOwner);
			var propertyInfo = (PropertyInfo) ownerType.FindInstace(MemberType.Property, propertyName);
			if (propertyInfo != null)
				return propertyInfo.PropertyGet<TOwner, TValue>();
			if (throwOnNotfound)
				throw new InvalidOperationException($"Property {propertyName} of type {ownerType} notfound");
			return null;
		}

		public static Func<TOwner, TValue> PropertyGet<TOwner, TValue>(this PropertyInfo propertyInfo)
		{
			if (propertyInfo==null)
				throw new ArgumentNullException(nameof(propertyInfo));
			var objParameterExpr = Expression.Parameter(typeof(TOwner), "instance");
			var instanceExpr = Expression.TypeAs(objParameterExpr, propertyInfo.DeclaringType ?? throw new InvalidOperationException());
			var propertyExpr = Expression.Property(instanceExpr, propertyInfo);
			return Expression.Lambda<Func<TOwner, TValue>>(propertyExpr, objParameterExpr).Compile();
		}

		public static Func<TValue> StaticPropertyGet<TOwner, TValue>(string propertyName, bool throwOnNotfound = true)
			=> StaticPropertyGet<TValue>(propertyName, typeof(TOwner), throwOnNotfound);

		public static Func<TValue> StaticPropertyGet<TValue>(string propertyName, Type ownerType, bool throwOnNotfound = true)
		{
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentNullException(nameof(propertyName));
			var propertyInfo = (PropertyInfo) ownerType.FindStatic(MemberType.Property, propertyName);
			if (propertyInfo != null)
				return propertyInfo.StaticPropertyGet<TValue>();
			if (throwOnNotfound)
				throw new InvalidOperationException($"Property {propertyName} of type {ownerType} notfound");
			return null;
		}

		public static Func<TValue> StaticPropertyGet<TValue>(this PropertyInfo propertyInfo)
		{
			if (propertyInfo==null)
				throw new ArgumentNullException(nameof(propertyInfo));
			var propertyExpr = Expression.Property(null, propertyInfo);
			return Expression.Lambda<Func<TValue>>(propertyExpr).Compile();
		}

		public static Action<TValue> StaticPropertySet<TOwner, TValue>(string propertyName, bool throwOnNotfound = true)
			=> StaticPropertySet<TValue>(propertyName, typeof(TOwner), throwOnNotfound);

		public static Action<TValue> StaticPropertySet<TValue>(string propertyName, Type ownerType, bool throwOnNotfound = true)
		{
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentNullException(nameof(propertyName));
			var propertyInfo = (PropertyInfo) ownerType.FindStatic(MemberType.Property, propertyName);
			if (propertyInfo != null)
				return propertyInfo.StaticPropertySet<TValue>();
			if (throwOnNotfound)
				throw new InvalidOperationException($"Property {propertyName} of type {ownerType} notfound");
			return null;
		}

		public static Action<TValue> StaticPropertySet<TValue>(this PropertyInfo propertyInfo)
		{
			if (propertyInfo==null)
				throw new ArgumentNullException(nameof(propertyInfo));
			var propertyExpr = Expression.Property(null, propertyInfo);
			var valueExpr = Expression.Parameter(propertyInfo.PropertyType, "value");
			var assignExpr = Expression.Assign(propertyExpr, valueExpr);
			return Expression.Lambda<Action<TValue>>(assignExpr, valueExpr).Compile();
		}

		public static Action<TOwner, TValue> PropertySet<TOwner, TValue>(string propertyName, bool throwOnNotfound = true)
		{
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentNullException(nameof(propertyName));
			var ownerType = typeof(TOwner);
			var propertyInfo = (PropertyInfo) ownerType.FindInstace(MemberType.Property, propertyName);
			if (propertyInfo != null)
				return propertyInfo.PropertySet<TOwner, TValue>();
			if (throwOnNotfound)
				throw new InvalidOperationException($"Property {propertyName} of type {ownerType} notfound");
			return null;
		}

		public static Action<TOwner, TValue> PropertySet<TOwner, TValue>(this PropertyInfo propertyInfo)
		{
			if (propertyInfo==null)
				throw new ArgumentNullException(nameof(propertyInfo));
			var objParameterExpr = Expression.Parameter(typeof(TOwner), "instance");
			var instanceExpr = Expression.TypeAs(objParameterExpr, propertyInfo.DeclaringType ?? throw new InvalidOperationException());
			var propertyExpr = Expression.Property(instanceExpr, propertyInfo);
			var valueExpr = Expression.Parameter(propertyInfo.PropertyType, "value");
			var assignExpr = Expression.Assign(propertyExpr, valueExpr);
			return Expression.Lambda<Action<TOwner, TValue>>(assignExpr, objParameterExpr, valueExpr).Compile();
		}

		public static Func<TOwner, TValue> FieldGet<TOwner, TValue>(string fieldName, bool throwOnNotfound = true)
		{
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentNullException(nameof(fieldName));
			var ownerType = typeof(TOwner);
			var fieldInfo = (FieldInfo) ownerType.FindInstace(MemberType.Field, fieldName);
			if (fieldInfo != null)
				return fieldInfo.FieldGet<TOwner, TValue>();
			if (throwOnNotfound)
				throw new InvalidOperationException($"Field {fieldName} of type {ownerType} notfound");
			return null;
		}

		public static Func<TOwner, TValue> FieldGet<TOwner, TValue>(this FieldInfo fieldInfo)
		{
			if (fieldInfo==null)
				throw new ArgumentNullException(nameof(fieldInfo));
			var objParameterExpr = Expression.Parameter(typeof(TOwner), "instance");
			var instanceExpr = Expression.TypeAs(objParameterExpr, fieldInfo.DeclaringType ?? throw new InvalidOperationException());
			var fieldAccess = Expression.Field(instanceExpr, fieldInfo);
			return Expression.Lambda<Func<TOwner, TValue>>(fieldAccess, objParameterExpr).Compile();
		}

		public static Action<TOwner, TValue> FieldSet<TOwner, TValue>(string fieldName, bool throwOnNotfound = true)
		{
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentNullException(nameof(fieldName));
			var ownerType = typeof(TOwner);
			var fieldInfo = (FieldInfo) ownerType.FindInstace(MemberType.Field, fieldName);
			if (fieldInfo != null)
				return fieldInfo.FieldSet<TOwner, TValue>();
			if (throwOnNotfound)
				throw new InvalidOperationException($"Field {fieldName} of type {ownerType} notfound");
			return null;
		}

		public static Action<TOwner, TValue> FieldSet<TOwner, TValue>(this FieldInfo fieldInfo)
		{
			if (fieldInfo==null)
				throw new ArgumentNullException(nameof(fieldInfo));
			var objParameterExpr = Expression.Parameter(typeof(TOwner), "instance");
			var instanceExpr = Expression.TypeAs(objParameterExpr, fieldInfo.DeclaringType ?? throw new InvalidOperationException());
			var fieldAccess = Expression.Field(instanceExpr, fieldInfo);
			var valueExpr = Expression.Parameter(fieldInfo.FieldType, "value");
			var assignExpr = Expression.Assign(fieldAccess, valueExpr);
			return Expression.Lambda<Action<TOwner, TValue>>(assignExpr, objParameterExpr, valueExpr).Compile();
		}

		public static Func<TValue> StaticFieldGet<TOwner, TValue>(string fieldName, bool throwOnNotfound = true)
			=> StaticFieldGet<TValue>(fieldName, typeof(TOwner), throwOnNotfound);

		public static Func<TValue> StaticFieldGet<TValue>(string fieldName, Type ownerType, bool throwOnNotfound = true)
		{
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentNullException(nameof(fieldName));
			var fieldInfo = (FieldInfo) ownerType.FindStatic(MemberType.Field, fieldName);
			if (fieldInfo != null)
				return fieldInfo.StaticFieldGet<TValue>();
			if (throwOnNotfound)
				throw new InvalidOperationException($"Field {fieldName} of type {ownerType} notfound");
			return null;
		}

		public static Func<TValue> StaticFieldGet<TValue>(this FieldInfo fieldInfo)
		{
			if (fieldInfo==null)
				throw new ArgumentNullException(nameof(fieldInfo));
			var fieldExpr = Expression.Field(null, fieldInfo);
			return Expression.Lambda<Func<TValue>>(fieldExpr).Compile();
		}

		public static Action<TValue> StaticFieldSet<TOwner, TValue>(string propertyName, bool throwOnNotfound = true)
			=> StaticFieldSet<TValue>(propertyName, typeof(TOwner), throwOnNotfound);

		public static Action<TValue> StaticFieldSet<TValue>(string propertyName, Type ownerType, bool throwOnNotfound = true)
		{
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentNullException(nameof(propertyName));
			var fieldInfo = (FieldInfo) ownerType.FindStatic(MemberType.Field, propertyName);
			if (fieldInfo != null)
				return fieldInfo.StaticFieldSet<TValue>();
			if (throwOnNotfound)
				throw new InvalidOperationException($"Property {propertyName} of type {ownerType} notfound");
			return null;
		}

		public static Action<TValue> StaticFieldSet<TValue>(this FieldInfo fieldInfo)
		{
			if (fieldInfo==null)
				throw new ArgumentNullException(nameof(fieldInfo));
			var fieldExpr = Expression.Field(null, fieldInfo);
			var valueExpr = Expression.Parameter(fieldInfo.FieldType, "value");
			var assignExpr = Expression.Assign(fieldExpr, valueExpr);
			return Expression.Lambda<Action<TValue>>(assignExpr, valueExpr).Compile();
		}

		public static MemberInfo FindInstace(this Type type, MemberType memberType, string memberName)
			=> type.FindMember(memberType, memberName, true);
		public static MemberInfo FindStatic(this Type type, MemberType memberType, string memberName)
			=> type.FindMember(memberType, memberName, false);

		public static MemberInfo FindMember(this Type type, MemberType memberType, string memberName, bool isInstance)
		{
			if (type==null)
				throw new ArgumentNullException(nameof(type));
			Func<BindingFlags, MemberInfo> resolver;
			switch(memberType)
			{
				case MemberType.Method:
					resolver = bf => type.GetMethod(memberName, bf);
					break;
				case MemberType.Property:
					resolver = bf => type.GetProperty(memberName, bf);
					break;
				case MemberType.Field:
					resolver = bf => type.GetField(memberName, bf);
					break;
				default:
					throw new ArgumentException($"Unsupported value {memberType} of enum MemberType");
			}
			var bfi = isInstance ? BindingFlags.Instance : BindingFlags.Static;
			return resolver(bfi | BindingFlags.NonPublic) ?? resolver(bfi | BindingFlags.Public);
		}
	}
}
