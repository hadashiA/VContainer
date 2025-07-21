using System;
using System.Reflection;
using System.Linq.Expressions;

namespace Reflex
{
	internal class MonoReflector : Reflector
	{
		internal override DynamicObjectActivator GenerateGenericActivator(Type type, ConstructorInfo constructor, Type[] parameters)
		{
			var param = Expression.Parameter(typeof(object[]));
			var argumentsExpressions = new Expression[parameters.Length];

			for (int i = 0; i < parameters.Length; i++)
			{
				var index = Expression.Constant(i);
				var parameterType = parameters[i];
				var parameterAccessor = Expression.ArrayIndex(param, index);
				var parameterCast = Expression.Convert(parameterAccessor, parameterType);
				argumentsExpressions[i] = parameterCast;
			}

			var newExpression = Expression.New(constructor, argumentsExpressions);
			var lambda = Expression.Lambda(typeof(DynamicObjectActivator), Expression.Convert(newExpression, typeof(object)), param);
			return (DynamicObjectActivator) lambda.Compile();
		}

		internal override DynamicObjectActivator GenerateDefaultActivator(Type type)
		{
			var body = Expression.Convert(Expression.Default(type), typeof(object));
			var lambda = Expression.Lambda(typeof(DynamicObjectActivator), body, Expression.Parameter(typeof(object[])));
			return (DynamicObjectActivator) lambda.Compile();
		}
	}
}