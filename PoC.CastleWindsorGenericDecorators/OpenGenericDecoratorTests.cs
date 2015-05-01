using System;
using System.Linq;
using System.Runtime.Remoting.Channels;
using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Diagnostics.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace PoC.CastleWindsorGenericDecorators
{
    [TestFixture]
    public class OpenGenericDecoratorTests
    {
        [Test]
        public void ShouldDecorateConcreteWithOpenGeneric()
        {
            var container = new WindsorContainer();
            container.Register(
                Component.For<IInterceptor>().ImplementedBy<LoggingInterceptor>(),
                //Component.For(typeof(IHandle<>)).ImplementedBy(typeof(LoggingHandler<>)),
                Component.For<IHandle<CopyFile>>().ImplementedBy<CopyFileHandler>().Interceptors<LoggingInterceptor>()
            );
            //container.Kernel.AddHandlerSelector(new OpenGenericHandlerSelector());
            //container.Kernel.Resolver.AddSubResolver(new OpenGenericHandlerSubDependencyResolver(container.Kernel));
            //container.Kernel.AddHandlersFilter(new OpenGenericHandlerFilter());

            var handlers = container.ResolveAll<IHandle<CopyFile>>();

            for (int i = 0; i < handlers.Length; i++)
            {
                var handler = handlers[i];
                this.Log(string.Format("## Handler {0}: {1}", i + 1, handler.GetType().Name));
                handler.Handle(new CopyFile());
                Console.WriteLine();
            }

            handlers.Should().HaveCount(1);
            Assert.Fail();
        }
    }

    public class LoggingInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            this.Log("Intercept: {0}\n", invocation.TargetType.GetTypeName());
            invocation.Proceed();
        }
    }

    public class OpenGenericHandlerSelector : IHandlerSelector
    {
        public bool HasOpinionAbout(string key, Type service)
        {
            this.Log("Asking for opinion on {0}", service.GetTypeName());
            return true;
            //if (!service.IsGenericType)
            //{
            //    Console.WriteLine("{0} is not interested.\n", this.GetType().Name);
            //    return false;
            //}
            //return service.IsGenericType && typeof(IHandle<>).MakeGenericType(service.GetGenericArguments()[0]).IsAssignableFrom(service);
        }

        public IHandler SelectHandler(string key, Type service, IHandler[] handlers)
        {
            //this.Log("Select from: {0}", String.Join(", ", handlers.Select(x => x.GetComponentName())));
            var handler = handlers.Last();
            this.Log("Chosen: {0}\n", handler.GetComponentName());
            return handler;
            //var decorator = typeof (IHandleDecorator<>).MakeGenericType(service.GetGenericArguments()[0]);
            //var handler = handlers.Last();
            //this.Log("Chosen: {0}", handler.GetComponentName());
            //this.Log();
            //return handler;
        }
    }

    public class OpenGenericHandlerFilter : IHandlersFilter
    {
        public bool HasOpinionAbout(Type service)
        {
            this.Log("Asking for opinion on {0}", service.GetTypeName());
            return true;
        }

        public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
        {
            //this.Log("Filter from: {0}", String.Join(", ", handlers.Select(x => x.GetComponentName())));
            var handler = handlers.Last();
            this.Log("Chosen: {0}\n", handler.GetComponentName());
            return new []{ handler };
        }
    }

    public class OpenGenericHandlerSubDependencyResolver : ISubDependencyResolver
    {
        readonly IKernel _kernel;

        public OpenGenericHandlerSubDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
            DependencyModel dependency)
        {
            this.Log("CanResolve target {0} with {1}\n", dependency.TargetType.GetTypeName(), model.Implementation.GetTypeName());
            return true;
        }

        public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
            DependencyModel dependency)
        {
            this.Log("Resolve target {0} with {1}\n", dependency.TargetType.GetTypeName(), model.Implementation.GetTypeName());
            return _kernel.Resolve(dependency.DependencyKey, dependency.TargetType);
        }
    }

    public interface IHandle<in T>
    {
        void Handle(T message);
    }

    public interface IHandleDecorator<in T> : IHandle<T> { }

    public class LoggingHandler<T> : IHandleDecorator<T>
    {
        readonly IHandle<T> _decorated;

        public LoggingHandler(IHandle<T> decorated)
        {
            _decorated = decorated;
        }

        public void Handle(T message)
        {
            this.Log("LoggingHandler.Handle :: Before");
            _decorated.Handle(message);
            this.Log("LoggingHandler.Handle :: After");
        }
    }

    public class CopyFileHandler : IHandle<CopyFile>
    {
        public void Handle(CopyFile message)
        {
            this.Log("Handle");
        }
    }

    public class CopyFile { }

    public static class LoggingExtentions
    {
        [JetBrains.Annotations.StringFormatMethod("format")]
        public static void Log(this object source, string format, params string[] args)
        {
            var typeName = source.GetType().GetTypeName();
            var prependedMessage = String.Format("[{0}] {1}", typeName, format);
            Console.WriteLine(prependedMessage, args);
        }
    }

    public static class TypeExtensions
    {
        public static string GetTypeName(this Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var typeNameBase = type.Name.Split('`').First();
            var arguments = String.Join(", ", type.GetGenericArguments().Select(x => x.Name));
            return String.Format("{0}<{1}>", typeNameBase, arguments);
        }
    }
}
