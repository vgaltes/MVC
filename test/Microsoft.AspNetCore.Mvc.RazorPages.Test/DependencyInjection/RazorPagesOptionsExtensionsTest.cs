﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class RazorPagesOptionsExtensionsTest
    {
        [Fact]
        public void AddFilter_AddsFiltersToAllPages()
        {
            // Arrange
            var filter = Mock.Of<IFilterMetadata>();
            var options = new RazorPagesOptions();
            var models = new[]
            {
                CreateApplicationModel("/Pages/Index.cshtml", "/Index.cshtml"),
                CreateApplicationModel("/Pages/Users/Account.cshtml", "/Users/Account.cshtml"),
                CreateApplicationModel("/Pages/Users/Contact.cshtml", "/Users/Contact.cshtml"),
            };

            // Act
            options.ConfigureFilter(filter);
            ApplyConventions(options, models);

            // Assert
            Assert.Collection(models,
                model => Assert.Same(filter, Assert.Single(model.Filters)),
                model => Assert.Same(filter, Assert.Single(model.Filters)),
                model => Assert.Same(filter, Assert.Single(model.Filters)));
        }

        [Fact]
        public void AuthorizePage_AddsAllowAnonymousFilterToSpecificPage()
        {
            // Arrange
            var options = new RazorPagesOptions();
            var models = new[]
            {
                CreateApplicationModel("/Pages/Index.cshtml", "/Index.cshtml"),
                CreateApplicationModel("/Pages/Users/Account.cshtml", "/Users/Account.cshtml"),
                CreateApplicationModel("/Pages/Users/Contact.cshtml", "/Users/Contact.cshtml"),
            };

            // Act
            options.AuthorizeFolder("/Users");
            options.AllowAnonymousToPage("/Users/Contact.cshtml");
            ApplyConventions(options, models);

            // Assert
            Assert.Collection(models,
                model => Assert.Empty(model.Filters),
                model =>
                {
                    Assert.Equal("/Users/Account.cshtml", model.ViewEnginePath);
                    Assert.IsType<AuthorizeFilter>(Assert.Single(model.Filters));
                },
                model =>
                {
                    Assert.Equal("/Users/Contact.cshtml", model.ViewEnginePath);
                    Assert.IsType<AuthorizeFilter>(model.Filters[0]);
                    Assert.IsType<AllowAnonymousFilter>(model.Filters[1]);
                });
        }

        [Theory]
        [InlineData("/Users")]
        [InlineData("/Users/")]
        public void AuthorizePage_AddsAllowAnonymousFilterToPagesUnderFolder(string folderName)
        {
            // Arrange
            var options = new RazorPagesOptions();
            var models = new[]
            {
                CreateApplicationModel("/Pages/Index.cshtml", "/Index.cshtml"),
                CreateApplicationModel("/Pages/Users/Account.cshtml", "/Users/Account.cshtml"),
                CreateApplicationModel("/Pages/Users/Contact.cshtml", "/Users/Contact.cshtml"),
            };

            // Act
            options.AuthorizeFolder("/");
            options.AllowAnonymousToFolder("/Users");
            ApplyConventions(options, models);

            // Assert
            Assert.Collection(models,
                model =>
                {
                    Assert.Equal("/Index.cshtml", model.ViewEnginePath);
                    Assert.IsType<AuthorizeFilter>(Assert.Single(model.Filters));
                },
                model =>
                {
                    Assert.Equal("/Users/Account.cshtml", model.ViewEnginePath);
                    Assert.IsType<AuthorizeFilter>(model.Filters[0]);
                    Assert.IsType<AllowAnonymousFilter>(model.Filters[1]);
                },
                model =>
                {
                    Assert.Equal("/Users/Contact.cshtml", model.ViewEnginePath);
                    Assert.IsType<AuthorizeFilter>(model.Filters[0]);
                    Assert.IsType<AllowAnonymousFilter>(model.Filters[1]);
                });
        }

        [Fact]
        public void AuthorizePage_AddsAuthorizeFilterWithPolicyToSpecificPage()
        {
            // Arrange
            var options = new RazorPagesOptions();
            var models = new[]
            {
                CreateApplicationModel("/Pages/Index.cshtml", "/Index.cshtml"),
                CreateApplicationModel("/Pages/Users/Account.cshtml", "/Users/Account.cshtml"),
                CreateApplicationModel("/Pages/Users/Contact.cshtml", "/Users/Contact.cshtml"),
            };

            // Act
            options.AuthorizePage("/Users/Account.cshtml", "Manage-Accounts");
            ApplyConventions(options, models);

            // Assert
            Assert.Collection(models,
                model => Assert.Empty(model.Filters),
                model =>
                {
                    Assert.Equal("/Users/Account.cshtml", model.ViewEnginePath);
                    var authorizeFilter = Assert.IsType<AuthorizeFilter>(Assert.Single(model.Filters));
                    var authorizeData = Assert.IsType<AuthorizeAttribute>(Assert.Single(authorizeFilter.AuthorizeData));
                    Assert.Equal("Manage-Accounts", authorizeData.Policy);
                },
                model => Assert.Empty(model.Filters));
        }

        [Fact]
        public void AuthorizePage_AddsAuthorizeFilterWithoutPolicyToSpecificPage()
        {
            // Arrange
            var options = new RazorPagesOptions();
            var models = new[]
            {
                CreateApplicationModel("/Pages/Index.cshtml", "/Index.cshtml"),
                CreateApplicationModel("/Pages/Users/Account.cshtml", "/Users/Account.cshtml"),
                CreateApplicationModel("/Pages/Users/Contact.cshtml", "/Users/Contact.cshtml"),
            };

            // Act
            options.AuthorizePage("/Users/Account.cshtml");
            ApplyConventions(options, models);

            // Assert
            Assert.Collection(models,
                model => Assert.Empty(model.Filters),
                model =>
                {
                    Assert.Equal("/Users/Account.cshtml", model.ViewEnginePath);
                    var authorizeFilter = Assert.IsType<AuthorizeFilter>(Assert.Single(model.Filters));
                    var authorizeData = Assert.IsType<AuthorizeAttribute>(Assert.Single(authorizeFilter.AuthorizeData));
                    Assert.Equal(string.Empty, authorizeData.Policy);
                },
                model => Assert.Empty(model.Filters));
        }

        [Theory]
        [InlineData("/Users")]
        [InlineData("/Users/")]
        public void AuthorizePage_AddsAuthorizeFilterWithPolicyToPagesUnderFolder(string folderName)
        {
            // Arrange
            var options = new RazorPagesOptions();
            var models = new[]
            {
                CreateApplicationModel("/Pages/Index.cshtml", "/Index.cshtml"),
                CreateApplicationModel("/Pages/Users/Account.cshtml", "/Users/Account.cshtml"),
                CreateApplicationModel("/Pages/Users/Contact.cshtml", "/Users/Contact.cshtml"),
            };

            // Act
            options.AuthorizeFolder(folderName, "Manage-Accounts");
            ApplyConventions(options, models);

            // Assert
            Assert.Collection(models,
                model => Assert.Empty(model.Filters),
                model =>
                {
                    Assert.Equal("/Users/Account.cshtml", model.ViewEnginePath);
                    var authorizeFilter = Assert.IsType<AuthorizeFilter>(Assert.Single(model.Filters));
                    var authorizeData = Assert.IsType<AuthorizeAttribute>(Assert.Single(authorizeFilter.AuthorizeData));
                    Assert.Equal("Manage-Accounts", authorizeData.Policy);
                },
                model =>
                {
                    Assert.Equal("/Users/Contact.cshtml", model.ViewEnginePath);
                    var authorizeFilter = Assert.IsType<AuthorizeFilter>(Assert.Single(model.Filters));
                    var authorizeData = Assert.IsType<AuthorizeAttribute>(Assert.Single(authorizeFilter.AuthorizeData));
                    Assert.Equal("Manage-Accounts", authorizeData.Policy);
                });
        }

        [Theory]
        [InlineData("/Users")]
        [InlineData("/Users/")]
        public void AuthorizePage_AddsAuthorizeFilterWithoutPolicyToPagesUnderFolder(string folderName)
        {
            // Arrange
            var options = new RazorPagesOptions();
            var models = new[]
            {
                CreateApplicationModel("/Pages/Index.cshtml", "/Index.cshtml"),
                CreateApplicationModel("/Pages/Users/Account.cshtml", "/Users/Account.cshtml"),
                CreateApplicationModel("/Pages/Users/Contact.cshtml", "/Users/Contact.cshtml"),
            };

            // Act
            options.AuthorizeFolder(folderName);
            ApplyConventions(options, models);

            // Assert
            Assert.Collection(models,
                model => Assert.Empty(model.Filters),
                model =>
                {
                    Assert.Equal("/Users/Account.cshtml", model.ViewEnginePath);
                    var authorizeFilter = Assert.IsType<AuthorizeFilter>(Assert.Single(model.Filters));
                    var authorizeData = Assert.IsType<AuthorizeAttribute>(Assert.Single(authorizeFilter.AuthorizeData));
                    Assert.Equal(string.Empty, authorizeData.Policy);
                },
                model =>
                {
                    Assert.Equal("/Users/Contact.cshtml", model.ViewEnginePath);
                    var authorizeFilter = Assert.IsType<AuthorizeFilter>(Assert.Single(model.Filters));
                    var authorizeData = Assert.IsType<AuthorizeAttribute>(Assert.Single(authorizeFilter.AuthorizeData));
                    Assert.Equal(string.Empty, authorizeData.Policy);
                });
        }

        [Fact]
        public void AddPageRoute_AddsRouteToSelector()
        {
            // Arrange
            var options = new RazorPagesOptions();
            var models = new[]
            {
                new PageRouteModel("/Pages/Index.cshtml", "/Index.cshtml")
                {
                    Selectors =
                    {
                        CreateSelectorModel("Index", suppressLinkGeneration: true),
                        CreateSelectorModel(""),
                    }
                },
                new PageRouteModel("/Pages/About.cshtml", "/About.cshtml")
                {
                    Selectors =
                    {
                        CreateSelectorModel("About"),
                    }
                }
            };

            // Act
            options.AddPageRoute("/Index.cshtml", "Different-Route");
            ApplyConventions(options, models);

            // Assert
            Assert.Collection(models,
                model =>
                {
                    Assert.Equal("/Index.cshtml", model.ViewEnginePath);
                    Assert.Collection(model.Selectors,
                        selector =>
                        {
                            Assert.Equal("Index", selector.AttributeRouteModel.Template);
                            Assert.True(selector.AttributeRouteModel.SuppressLinkGeneration);
                        },
                        selector =>
                        {
                            Assert.Equal("", selector.AttributeRouteModel.Template);
                            Assert.True(selector.AttributeRouteModel.SuppressLinkGeneration);
                        },
                        selector =>
                        {
                            Assert.Equal("Different-Route", selector.AttributeRouteModel.Template);
                            Assert.False(selector.AttributeRouteModel.SuppressLinkGeneration);
                        });
                },
                model =>
                {
                    Assert.Equal("/About.cshtml", model.ViewEnginePath);
                    Assert.Collection(model.Selectors,
                        selector =>
                        {
                            Assert.Equal("About", selector.AttributeRouteModel.Template);
                            Assert.False(selector.AttributeRouteModel.SuppressLinkGeneration);
                        });
                });
        }

        private static SelectorModel CreateSelectorModel(string template, bool suppressLinkGeneration = false)
        {
            return new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel
                {
                    Template = template,
                    SuppressLinkGeneration = suppressLinkGeneration
                },
            };
        }

        private static void ApplyConventions(RazorPagesOptions options, PageRouteModel[] models)
        {
            foreach (var convention in options.RouteModelConventions)
            {
                foreach (var model in models)
                {
                    convention.Apply(model);
                }
            }
        }
        private static void ApplyConventions(RazorPagesOptions options, PageApplicationModel[] models)
        {
            foreach (var convention in options.ApplicationModelConventions)
            {
                foreach (var model in models)
                {
                    convention.Apply(model);
                }
            }
        }

        private PageApplicationModel CreateApplicationModel(string relativePath, string viewEnginePath)
        {
            var descriptor = new PageActionDescriptor
            {
                ViewEnginePath = viewEnginePath,
                RelativePath = relativePath,
            };

            return new PageApplicationModel(descriptor, typeof(object).GetTypeInfo(), new object[0]);
        }
    }
}
