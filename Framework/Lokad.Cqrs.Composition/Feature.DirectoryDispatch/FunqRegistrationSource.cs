using System;
using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Container = Lokad.Cqrs.Core.Container;

namespace Lokad.Cqrs.Feature.DirectoryDispatch
{
    public sealed class FunqRegistrationSource : Autofac.Core.IRegistrationSource
    {
        readonly Container _container;
        public FunqRegistrationSource(Container container)
        {
            _container = container;
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var type = service as TypedService;

            if (type == null)
            {
                yield break;
            }
            //type.ServiceType

            // we'll need to slow down

            var result = typeof(Container)
                .GetMethod("TryResolve", new Type[0])
                .MakeGenericMethod(type.ServiceType)
                .Invoke(_container, null);

            if (null == result)
                yield break;

            yield return new ComponentRegistration(Guid.NewGuid(), new ProvidedInstanceActivator(result),new RootScopeLifetime(), InstanceSharing.Shared, InstanceOwnership.ExternallyOwned, new[] {service}, new Dictionary<string,object>());
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }
    }
}