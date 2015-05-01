using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
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
                Component.For(typeof(IHandle<>)).ImplementedBy(typeof(LoggingHandler<>)),
                Component.For<IHandle<CopyFile>>().ImplementedBy<CopyFileHandler>()
            );

            var handlers = container.ResolveAll<IHandle<CopyFile>>();

            for (int i = 0; i < handlers.Length; i++)
            {
                var handler = handlers[i];
                Console.WriteLine(string.Format("## Handler {0}: {1}", i+1, handler.GetType().Name));
                handler.Handle(new CopyFile());
                Console.WriteLine();
            }

            handlers.Should().HaveCount(1);
        }
    }

public interface IHandle<in T>
{
    void Handle(T message);
}

    public class LoggingHandler<T> : IHandle<T>
    {
        readonly IHandle<T> _decorated;

        public LoggingHandler(IHandle<T> decorated)
        {
            _decorated = decorated;
        }

        public void Handle(T message)
        {
            Console.WriteLine("LoggingHandler.Handle :: Before");
            _decorated.Handle(message);
            Console.WriteLine("LoggingHandler.Handle :: After");
        }
    }

    public class CopyFileHandler : IHandle<CopyFile>
    {
        public void Handle(CopyFile message)
        {
            Console.WriteLine("CopyFileHandler.Handle");
        }
    }

    public class CopyFile { }
}
