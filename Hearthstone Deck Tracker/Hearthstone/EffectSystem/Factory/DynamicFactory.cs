using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Factory;

public abstract class DynamicFactory<T>
{
	protected static readonly Dictionary<string, Constructor> Constructors = new();
	public const BindingFlags ConstField = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

	static DynamicFactory()
	{
		var subClasses = Assembly.GetAssembly(typeof(T)).GetTypes().Where(t => t.IsClass && t.IsSubclassOf(typeof(T)));
		foreach(var entity in subClasses)
		{
			var instance = Activator.CreateInstance(entity, new object[] { 0, true }); // Assuming a default constructor with entityId = 0
			var cardProperties = entity.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var cardId = cardProperties.FirstOrDefault(x => x.Name == "CardId")?.GetValue(instance) as string;
			if(cardId != null)
			{
				var constructor = entity.GetConstructors().First();
				Constructors[cardId] = GetConstructor(constructor);
			}
		}
	}

	public delegate T Constructor(params object?[] args);

	/// <summary>
	/// Uses precompiled LINQ Expression Lambdas to instatiate a specific minion implementation.
	/// This approach is considerably faster than Activator.CreateInstance.
	/// </summary>
	public static Constructor GetConstructor(ConstructorInfo ctor)
	{
		var param = Expression.Parameter(typeof(object[]), "args");
		var ctorArgs = ctor.GetParameters().Select((x, i) =>
		{
			return Expression.Convert(Expression.ArrayIndex(param, Expression.Constant(i)), x.ParameterType);
		});
		return Expression.Lambda<Constructor>(Expression.New(ctor, ctorArgs), param).Compile();
	}
}
