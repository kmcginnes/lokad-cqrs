﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lokad.Cqrs.Core
{
	/// <include file='Container.xdoc' path='docs/doc[@for="Container"]/*'/>
	public sealed partial class Container : IDisposable
	{
	    readonly Dictionary<ServiceKey, ServiceEntry> _services = new Dictionary<ServiceKey, ServiceEntry>();
		// Disposable components include factory-scoped instances that we don't keep 
		// a strong reference to. 
	    readonly Stack<WeakReference> _disposables = new Stack<WeakReference>();
		// We always hold a strong reference to child containers.
	    readonly Stack<Container> _childContainers = new Stack<Container>();
		Container _parent;

		/// <include file='Container.xdoc' path='docs/doc[@for="Container.ctor"]/*'/>
		public Container()
		{
			_services[new ServiceKey(typeof(Func<Container, Container>), null)] =
				new ServiceEntry<Container, Func<Container, Container>>((Func<Container, Container>)(c => c))
				{
					Container = this,
					Instance = this,
					Owner = Owner.External,
					Reuse = ReuseScope.Container,
				};
		}

		/// <include file='Container.xdoc' path='docs/doc[@for="Container.DefaultOwner"]/*'/>
		public Owner DefaultOwner { get; set; }

		/// <include file='Container.xdoc' path='docs/doc[@for="Container.DefaultReuse"]/*'/>
		public ReuseScope DefaultReuse { get; set; }

		/// <include file='Container.xdoc' path='docs/doc[@for="Container.Dispose"]/*'/>
		public void Dispose()
		{
			while (_disposables.Count > 0)
			{
				var wr = _disposables.Pop();
				var disposable = (IDisposable)wr.Target;
				if (wr.IsAlive)
					disposable.Dispose();
			}
			while (_childContainers.Count > 0)
			{
				_childContainers.Pop().Dispose();
			}
		}

		/// <include file='Container.xdoc' path='docs/doc[@for="Container.Register(instance)"]/*'/>
		public void Register<TService>(TService instance)
		{
			Register(null, instance);
		}

		/// <include file='Container.xdoc' path='docs/doc[@for="Container.Register(name,instance)"]/*'/>
		public void Register<TService>(string name, TService instance)
		{
			var entry = RegisterImpl<TService, Func<Container, TService>>(name, null);
			
			// Set sensible defaults for instance registration.
			entry.ReusedWithin(ReuseScope.Hierarchy).OwnedBy(Owner.External);
			entry.InitializeInstance(instance);
		}


		private ServiceEntry<TService, TFunc> RegisterImpl<TService, TFunc>(string name, TFunc factory)
		{
			if (typeof(TService) == typeof(Container))
				throw new ArgumentException("Container service is built-in and read-only.");

			var entry = new ServiceEntry<TService, TFunc>(factory)
			{
				Container = this,
				Reuse = DefaultReuse,
				Owner = DefaultOwner
			};
			var key = new ServiceKey(typeof(TFunc), name);

			_services[key] = entry;

			return entry;
		}

		#region ResolveImpl

		/* All ResolveImpl are essentially equal, except for the type of the factory 
		 * which is "hardcoded" in each implementation. This slight repetition of 
		 * code gives us a bit more of perf. gain by avoiding an intermediate 
		 * func/lambda to call in a generic way as we did before.
		 */

		private TService ResolveImpl<TService>(string name, bool throwIfMissing)
		{
			// Would throw if missing as appropriate.
			var entry = GetEntry<TService, Func<Container, TService>>(name, throwIfMissing);
			// Return default if not registered and didn't throw above.
			if (entry == null)
				return default(TService);

			TService instance = entry.Instance;
			if (instance == null)
			{
				instance = entry.Factory(entry.Container);
				entry.InitializeInstance(instance);
			}

			return instance;
		}

		private TService ResolveImpl<TService, TArg>(string name, bool throwIfMissing, TArg arg)
		{
			// Would throw if missing as appropriate.
			var entry = GetEntry<TService, Func<Container, TArg, TService>>(name, throwIfMissing);
			// Return default if not registered and didn't throw above.
			if (entry == null)
				return default(TService);

			TService instance = entry.Instance;
			if (instance == null)
			{
				instance = entry.Factory(entry.Container, arg);
				entry.InitializeInstance(instance);
			}

			return instance;
		}

		private TService ResolveImpl<TService, TArg1, TArg2>(string name, bool throwIfMissing, TArg1 arg1, TArg2 arg2)
		{
			// Would throw if missing as appropriate.
			var entry = GetEntry<TService, Func<Container, TArg1, TArg2, TService>>(name, throwIfMissing);
			// Return default if not registered and didn't throw above.
			if (entry == null)
				return default(TService);

			TService instance = entry.Instance;
			if (instance == null)
			{
				instance = entry.Factory(entry.Container, arg1, arg2);
				entry.InitializeInstance(instance);
			}

			return instance;
		}

		private TService ResolveImpl<TService, TArg1, TArg2, TArg3>(string name, bool throwIfMissing, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			// Would throw if missing as appropriate.
			var entry = GetEntry<TService, Func<Container, TArg1, TArg2, TArg3, TService>>(name, throwIfMissing);
			// Return default if not registered and didn't throw above.
			if (entry == null)
				return default(TService);

			TService instance = entry.Instance;
			if (instance == null)
			{
				instance = entry.Factory(entry.Container, arg1, arg2, arg3);
				entry.InitializeInstance(instance);
			}

			return instance;
		}

		private TService ResolveImpl<TService, TArg1, TArg2, TArg3, TArg4>(string name, bool throwIfMissing, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
		{
			// Would throw if missing as appropriate.
			var entry = GetEntry<TService, Func<Container, TArg1, TArg2, TArg3, TArg4, TService>>(name, throwIfMissing);
			// Return default if not registered and didn't throw above.
			if (entry == null)
				return default(TService);

			TService instance = entry.Instance;
			if (instance == null)
			{
				instance = entry.Factory(entry.Container, arg1, arg2, arg3, arg4);
				entry.InitializeInstance(instance);
			}

			return instance;
		}

		private TService ResolveImpl<TService, TArg1, TArg2, TArg3, TArg4, TArg5>(string name, bool throwIfMissing, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
		{
			// Would throw if missing as appropriate.
			var entry = GetEntry<TService, Func<Container, TArg1, TArg2, TArg3, TArg4, TArg5, TService>>(name, throwIfMissing);
			// Return default if not registered and didn't throw above.
			if (entry == null)
				return default(TService);

			TService instance = entry.Instance;
			if (instance == null)
			{
				instance = entry.Factory(entry.Container, arg1, arg2, arg3, arg4, arg5);
				entry.InitializeInstance(instance);
			}

			return instance;
		}

		private TService ResolveImpl<TService, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string name, bool throwIfMissing, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6)
		{
			// Would throw if missing as appropriate.
			var entry = GetEntry<TService, Func<Container, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TService>>(name, throwIfMissing);
			// Return default if not registered and didn't throw above.
			if (entry == null)
				return default(TService);

			TService instance = entry.Instance;
			if (instance == null)
			{
				instance = entry.Factory(entry.Container, arg1, arg2, arg3, arg4, arg5, arg6);
				entry.InitializeInstance(instance);
			}

			return instance;
		}

		#endregion

		internal void TrackDisposable(object instance)
		{
			_disposables.Push(new WeakReference(instance));
		}

		private ServiceEntry<TService, TFunc> GetEntry<TService, TFunc>(string serviceName, bool throwIfMissing)
		{
			var key = new ServiceKey(typeof(TFunc), serviceName);
			ServiceEntry entry = null;
			Container container = this;

			// Go up the hierarchy always for registrations.
			while (!container._services.TryGetValue(key, out entry) && container._parent != null)
			{
				container = container._parent;
			}

			if (entry != null)
			{
				if (entry.Reuse == ReuseScope.Container && entry.Container != this)
				{
					entry = ((ServiceEntry<TService, TFunc>)entry).CloneFor(this);
					_services[key] = entry;
				}
			}
			else
			{
				//i.e. if called Resolve<> for Constructor injection
				if (throwIfMissing) 
				{
				    foreach (var source in Sources)
				    {
				        if (source.Supports(typeof(TService)))
				        {
				            var provider = source.GetProvider(typeof(TService));
				            var serviceEntry = new ServiceEntry<TService, TFunc>((TFunc)(object)(Func<Container, TService>)(c => (TService)provider(c)));
				            return serviceEntry.CloneFor(this);
				        }
				    }

					
					ThrowMissing<TService>(serviceName);
				}
				else
				{
                    foreach (var source in Sources)
                    {
                        if (source.Supports(typeof(TService)))
                        {
                            var provider = source.GetProvider(typeof(TService));
                            var serviceEntry = new ServiceEntry<TService, TFunc>((TFunc)(object)(Func<Container, TService>)(c => (TService)provider(c)));
                            return serviceEntry.CloneFor(this);
                        }
                    }
				}
			} 

			return (ServiceEntry<TService, TFunc>)entry;
		}
        [DebuggerNonUserCode]
		private static void ThrowMissing<TService>(string serviceName)
        {
            if (serviceName == null)
				throw new ResolutionException(typeof(TService));
            throw new ResolutionException(typeof(TService), serviceName);
        }

	    private void ThrowIfNotRegistered<TService, TFunc>(string name)
		{
			GetEntry<TService, TFunc>(name, true);
		}
	}
}
