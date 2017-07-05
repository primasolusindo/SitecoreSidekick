﻿using NSubstitute;
using SitecoreSidekick.Shared.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SitecoreSidekick.UnitTests
{
	public class TestBase
	{
		/// <summary>
		/// The IoC container
		/// </summary>
		protected Container Container { get; }

		protected TestBase()
		{
			Container = new Container();
			Bootstrap.SetContainer(Container);
		}

		/// <summary>
		/// Creates an instance of the provided type using default substitutions for anything that might need to be pulled from the IoC container.
		/// </summary>
		/// <typeparam name="T">The type of instance to create</typeparam>
		/// <returns>A new instance of the provided type</returns>
		protected T CreateInstance<T>()
		{
			IEnumerable<FieldInfo> fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.FieldType.IsInterface);
			foreach (FieldInfo field in fields)
			{
				if (Container.ContainsRegistration(field.FieldType)) continue;
				object fieldImplementation = Substitute.For(new[] { field.FieldType }, new object[] { });
				Container.Register(field.FieldType, fieldImplementation);
			}

			return Activator.CreateInstance<T>();
		}

		/// <summary>
		/// Creates a substitute for the provided interface and registers it with the container
		/// </summary>
		/// <typeparam name="T">The type of interface to register with the container</typeparam>
		/// <returns>The substitute that was registered to the container</returns>
		protected T CreateSubstitute<T>() where T : class
		{
			T substitute = Substitute.For<T>();
			Container.Register<T>(substitute);
			return substitute;
		}

		/// <summary>
		/// Fetches the provided interface as a substitute from the container.  If the interface does not exist in the container, it is created.
		/// </summary>
		/// <typeparam name="T">The type of interface to retrieve from the container</typeparam>
		/// <returns>The Substitute representing the provided interface from the container</returns>
		protected T GetSubstitute<T>() where T : class
		{
			return !Container.ContainsRegistration<T>() ? CreateSubstitute<T>() : Container.Resolve<T>();
		}
	}
}
