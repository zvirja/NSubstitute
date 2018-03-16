using System;
using System.Collections.Generic;
using NSubstitute.Core.Arguments;
using NSubstitute.Proxies;
using NSubstitute.Proxies.CastleDynamicProxy;
using NSubstitute.Proxies.DelegateProxy;
using NSubstitute.Routing;

namespace NSubstitute.Core
{
    public class SubstitutionContext : ISubstitutionContext
    {
        public static ISubstitutionContext Current { get; set; }

        private readonly ICallRouterResolver _callRouterResolver;
        public ISubstituteFactory SubstituteFactory { get; }
        public IRouteFactory RouteFactory { get; }
        [Obsolete("This property is obsolete and will be removed in a future version of the product.")]
        public SequenceNumberGenerator SequenceNumberGenerator { get; }
        public IThreadLocalContext ThreadContext { get; }

        static SubstitutionContext()
        {
            Current = new SubstitutionContext();
        }

        private SubstitutionContext()
        {
            ThreadContext = new ThreadLocalContext();
            var sequenceNumberGenerator = new SequenceNumberGenerator();
            _callRouterResolver = new CallRouterResolver();
            RouteFactory = new RouteFactory(ThreadContext);

            var callRouterFactory = new CallRouterFactory(sequenceNumberGenerator, RouteFactory);
            var argSpecificationQueue = new ArgumentSpecificationDequeue(ThreadContext.DequeueAllArgumentSpecifications);
            var dynamicProxyFactory = new CastleDynamicProxyFactory(argSpecificationQueue);
            var delegateFactory = new DelegateProxyFactory(dynamicProxyFactory);
            var proxyFactory = new ProxyFactory(delegateFactory, dynamicProxyFactory);
            SubstituteFactory = new SubstituteFactory(ThreadContext, callRouterFactory, proxyFactory);
#pragma warning disable 618 // Obsolete
            SequenceNumberGenerator = sequenceNumberGenerator;
#pragma warning restore 618 // Obsolete
        }

        public SubstitutionContext(ISubstituteFactory substituteFactory,
            IRouteFactory routeFactory,
            IThreadLocalContext threadLocalContext,
            ICallRouterResolver callRouterResolver)
        {
            SubstituteFactory = substituteFactory ?? throw new ArgumentNullException(nameof(substituteFactory));
            RouteFactory = routeFactory ?? throw new ArgumentNullException(nameof(routeFactory));
            ThreadContext = threadLocalContext ?? throw new ArgumentNullException(nameof(threadLocalContext));
            _callRouterResolver = callRouterResolver ?? throw new ArgumentNullException(nameof(callRouterResolver));

#pragma warning disable 618 // Obsolete
            SequenceNumberGenerator = new SequenceNumberGenerator();
#pragma warning restore 618 // Obsolete
        }

        public ICallRouter GetCallRouterFor(object substitute) =>
            _callRouterResolver.ResolveFor(substitute);


        // ***********************************************************
        // ********************** OBSOLETE API **********************
        // API below is obsolete and present for the binary compatibility with the previous versions.
        // All implementations are relaying to the non-obsolete members.

        [Obsolete("This property is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.IsQuerying) + " property instead.")]
        public bool IsQuerying => ThreadContext.IsQuerying;

        [Obsolete("This property is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.PendingSpecification) + " property instead.")]
        public PendingSpecificationInfo PendingSpecificationInfo
        {
            get
            {
                if (!ThreadContext.PendingSpecification.HasPendingCallSpecInfo())
                    return null;

                // This removes the pending specification, so we need to restore it back.
                var consumedSpecInfo = ThreadContext.PendingSpecification.UseCallSpecInfo();
                PendingSpecificationInfo = consumedSpecInfo;

                return consumedSpecInfo;
            }
            set
            {
                if (value == null)
                {
                    ThreadContext.PendingSpecification.Clear();
                    return;
                }

                // Emulate the old API. A bit clumsy, however it's here for the backward compatibility only
                // and is not expected to be used frequently.
                var unwrappedValue = value.Handle(
                    spec => Tuple.Create(spec, (ICall) null),
                    call => Tuple.Create((ICallSpecification) null, call));

                if (unwrappedValue.Item1 != null)
                {
                    ThreadContext.PendingSpecification.SetCallSpecification(unwrappedValue.Item1);
                }
                else
                {
                    ThreadContext.PendingSpecification.SetLastCall(unwrappedValue.Item2);
                }
            }
        }

        [Obsolete("This method is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.LastCallShouldReturn) + "() method instead.")]
        public ConfiguredCall LastCallShouldReturn(IReturn value, MatchArgs matchArgs) =>
            ThreadContext.LastCallShouldReturn(value, matchArgs);

        [Obsolete("This method is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.ClearLastCallRouter) + "() method instead.")]
        public void ClearLastCallRouter() =>
            ThreadContext.ClearLastCallRouter();

        [Obsolete("This method is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(RouteFactory) + " property instead.")]
        public IRouteFactory GetRouteFactory() =>
            RouteFactory;

        [Obsolete("This method is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.SetLastCallRouter) + "() method instead.")]
        public void LastCallRouter(ICallRouter callRouter) =>
            ThreadContext.SetLastCallRouter(callRouter);

        [Obsolete("This method is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.EnqueueArgumentSpecification) + "() method instead.")]
        public void EnqueueArgumentSpecification(IArgumentSpecification spec) =>
            ThreadContext.EnqueueArgumentSpecification(spec);

        [Obsolete("This method is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.DequeueAllArgumentSpecifications) + "() method instead.")]
        public IList<IArgumentSpecification> DequeueAllArgumentSpecifications() =>
            ThreadContext.DequeueAllArgumentSpecifications();

        [Obsolete("This method is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.SetPendingRasingEventArgumentsFactory) + "() method instead.")]
        public void RaiseEventForNextCall(Func<ICall, object[]> getArguments) =>
            ThreadContext.SetPendingRasingEventArgumentsFactory(getArguments);

        [Obsolete("This method is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.UsePendingRaisingEventArgumentsFactory) + "() method instead.")]
        public Func<ICall, object[]> DequeuePendingRaisingEventArguments() =>
            ThreadContext.UsePendingRaisingEventArgumentsFactory();

        [Obsolete("This method is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.AddToQuery) + "() method instead.")]
        public void AddToQuery(object target, ICallSpecification callSpecification) =>
            ThreadContext.AddToQuery(target, callSpecification);

        [Obsolete("This method is obsolete and will be removed in a future version of the product. " +
                  "Use the " + nameof(ThreadContext) + "." + nameof(IThreadLocalContext.RunQuery) + "() method instead.")]
        public IQueryResults RunQuery(Action calls) =>
            ThreadContext.RunQuery(calls);
    }
}