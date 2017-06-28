﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class RequestSizeLimitAttributeTest
    {
        [Fact]
        public void SetsMaxRequestBodySize()
        {
            // Arrange
            var requestSizeLimitAttribute = new RequestSizeLimitAttribute(12345, NullLoggerFactory.Instance);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestSizeLimitAttribute });

            var httpMaxRequestBodySize = new TestHttpMaxRequestBodySizeFeature();
            resourceExecutingContext.HttpContext.Features.Set<IHttpMaxRequestBodySizeFeature>(httpMaxRequestBodySize);

            // Act
            requestSizeLimitAttribute.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Equal(12345, httpMaxRequestBodySize.MaxRequestBodySize);
        }

        [Fact]
        public void SkipsWhenOverridden()
        {
            // Arrange
            var requestSizeLimitAttribute = new RequestSizeLimitAttribute(12345, NullLoggerFactory.Instance);
            var disableRequestSizeLimitAttribute = new DisableRequestSizeLimitAttribute(NullLoggerFactory.Instance);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestSizeLimitAttribute, disableRequestSizeLimitAttribute});

            var httpMaxRequestBodySize = new TestHttpMaxRequestBodySizeFeature();
            resourceExecutingContext.HttpContext.Features.Set<IHttpMaxRequestBodySizeFeature>(httpMaxRequestBodySize);

            // Act
            requestSizeLimitAttribute.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Null(httpMaxRequestBodySize.MaxRequestBodySize);
        }

        [Fact]
        public void LogsFeatureNotFound()
        {
            // Arrange
            var sink = new TestSink();
            var loggerFactory = new TestLoggerFactory(sink, enabled: true);

            var requestSizeLimitAttribute = new RequestSizeLimitAttribute(12345, loggerFactory);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestSizeLimitAttribute });

            // Act
            requestSizeLimitAttribute.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Single(sink.Writes);
            Assert.Equal($"The attribute {nameof(RequestSizeLimitAttribute)} could not find the feature {nameof(IHttpMaxRequestBodySizeFeature)}.", 
                sink.Writes[0].State.ToString());
        }

        [Fact]
        public void LogsFeatureIsReadOnly()
        {
            // Arrange
            var sink = new TestSink();
            var loggerFactory = new TestLoggerFactory(sink, enabled: true);

            var requestSizeLimitAttribute = new RequestSizeLimitAttribute(12345, loggerFactory);        
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestSizeLimitAttribute});

            var httpMaxRequestBodySize = new TestHttpMaxRequestBodySizeFeature();
            httpMaxRequestBodySize.IsReadOnly = true;
            resourceExecutingContext.HttpContext.Features.Set<IHttpMaxRequestBodySizeFeature>(httpMaxRequestBodySize);

            // Act
            requestSizeLimitAttribute.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Single(sink.Writes);
            Assert.Equal($"The implementation of {nameof(IHttpMaxRequestBodySizeFeature)} is read-only. The attribute cannot be applied.", sink.Writes[0].State.ToString());
        }

        [Fact]
        public void RequestSizeLimitAttribute_LogsMaxRequestBodySizeSet()
        {
            // Arrange
            var sink = new TestSink();
            var loggerFactory = new TestLoggerFactory(sink, enabled: true);

            var requestSizeLimitAttribute = new RequestSizeLimitAttribute(12345, loggerFactory);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestSizeLimitAttribute });

            var httpMaxRequestBodySize = new TestHttpMaxRequestBodySizeFeature();
            resourceExecutingContext.HttpContext.Features.Set<IHttpMaxRequestBodySizeFeature>(httpMaxRequestBodySize);

            // Act
            requestSizeLimitAttribute.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Single(sink.Writes);
            Assert.Equal($"The maximum request body size has been set to 12345.", sink.Writes[0].State.ToString());
        }

        [Fact]
        public void DisableRequestSizeLimitAttribute_LogsMaxRequestBodySizeSetToNull()
        {
            // Arrange
            var sink = new TestSink();
            var loggerFactory = new TestLoggerFactory(sink, enabled: true);

            var disableRequestSizeLimitAttribute = new DisableRequestSizeLimitAttribute(loggerFactory);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { disableRequestSizeLimitAttribute });

            var httpMaxRequestBodySize = new TestHttpMaxRequestBodySizeFeature();
            resourceExecutingContext.HttpContext.Features.Set<IHttpMaxRequestBodySizeFeature>(httpMaxRequestBodySize);

            // Act
            disableRequestSizeLimitAttribute.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Single(sink.Writes);
            Assert.Equal($"The maximum request body size has been set to null.", sink.Writes[0].State.ToString());
        }

        private static ResourceExecutingContext CreateResourceExecutingContext(IFilterMetadata[] filters)
        {
            return new ResourceExecutingContext(
                CreateActionContext(),
                filters,
                new List<IValueProviderFactory>());
        }

        private static ActionContext CreateActionContext()
        {
            return new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        }

        private class TestHttpMaxRequestBodySizeFeature : IHttpMaxRequestBodySizeFeature
        {
            public bool IsReadOnly { get; set; }

            public long? MaxRequestBodySize { get; set; }
        }
    }
}
