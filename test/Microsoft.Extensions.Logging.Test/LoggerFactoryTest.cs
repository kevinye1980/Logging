// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Logging.Test
{
    public class LoggerFactoryTest
    {
        [Fact]
        public void AddProvider_ThrowsAfterDisposed()
        {
            var factory = new LoggerFactory();
            factory.Dispose();
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.Throws<ObjectDisposedException>(() => ((ILoggerFactory) factory).AddProvider(CreateProvider()));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Fact]
        public void CreateLogger_ThrowsAfterDisposed()
        {
            var factory = new LoggerFactory();
            factory.Dispose();
            Assert.Throws<ObjectDisposedException>(() => factory.CreateLogger("d"));
        }

        private class TestLoggerFactory : LoggerFactory
        {
            public bool Disposed => CheckDisposed();
        }

        [Fact]
        public void Dispose_MultipleCallsNoop()
        {
            var factory = new TestLoggerFactory();
            factory.Dispose();
            Assert.True(factory.Disposed);
            factory.Dispose();
        }

        [Fact]
        public void Dispose_ProvidersAreDisposed()
        {
            // Arrange
            var factory = new LoggerFactory();
            var disposableProvider1 = CreateProvider();
            var disposableProvider2 = CreateProvider();

#pragma warning disable CS0618 // Type or member is obsolete
            ((ILoggerFactory) factory).AddProvider(disposableProvider1);
            ((ILoggerFactory) factory).AddProvider(disposableProvider2);
#pragma warning restore CS0618 // Type or member is obsolete

            // Act
            factory.Dispose();

            // Assert
            Mock.Get<IDisposable>(disposableProvider1)
                    .Verify(p => p.Dispose(), Times.Once());
            Mock.Get<IDisposable>(disposableProvider2)
                     .Verify(p => p.Dispose(), Times.Once());
        }

        private static ILoggerProvider CreateProvider()
        {
            var disposableProvider = new Mock<ILoggerProvider>();
            disposableProvider.As<IDisposable>()
                  .Setup(p => p.Dispose());
            return disposableProvider.Object;
        }

        [Fact]
        public void Dispose_ThrowException_SwallowsException()
        {
            // Arrange
            var factory = new LoggerFactory();
            var throwingProvider = new Mock<ILoggerProvider>();
            throwingProvider.As<IDisposable>()
                .Setup(p => p.Dispose())
                .Throws<Exception>();
#pragma warning disable CS0618 // Type or member is obsolete
            ((ILoggerFactory) factory).AddProvider(throwingProvider.Object);
#pragma warning restore CS0618 // Type or member is obsolete

            // Act
            factory.Dispose();

            // Assert
            throwingProvider.As<IDisposable>()
                .Verify(p => p.Dispose(), Times.Once());
        }
    }
}
