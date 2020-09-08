using GraphQLBuilder.Attributes;
using GraphQLBuilder.Exceptions;
using System;
using System.Text.Json;
using Xunit;

namespace GraphQLBuilder.Tests
{
    public class GraphQLQuery_E2E_Tests
    {
        public GraphQLQuery_E2E_Tests()
        {

        }

        [Fact]
        public void Serialize_MapsPropertyNameToFieldName()
        {
            var query = new GraphQLQuery<BlogPost>().Build();

            Assert.Contains("title", query);
        }

        [Fact]
        public void Serialize_DoesNotContainUnMappedProperties()
        {
            var query = new GraphQLQuery<BlogPost>().Build();

            Assert.DoesNotContain("unmappedfield", query.ToLower());
        }

        [Fact]
        public void Serialize_WithOfType_UsesProvidedTypeInQuery()
        {
            var query = new GraphQLQuery<BlogPost>()
                .OfType("some_remote_type")
                .ToString();

            Assert.Contains("some_remote_type", query);
        }

        [Fact]
        public void Serialize_OperationName_AddsSyntax()
        {
            var query = new GraphQLQuery<BlogPost>()
                .AsOperation("GetBlogPosts")
                .ToString();

            Assert.Contains("query GetBlogPosts", query);
        }

        [Fact]
        public void Serialize_ArgumentWithProperty_AddsProperArgumentSyntax()
        {
            var query = new GraphQLQuery<BlogPost>()
                .WithArgument(x => x.Title, "length", 5)
                .ToString();

            Assert.Contains("title(length: 5)", query);
        }

        [Fact]
        public void Serialize_ArgumentWithoutProperty_AddsProperArgumentSyntax()
        {
            var query = new GraphQLQuery<BlogPost>()
                .WithArgument("id", 1)
                .ToString();

            Assert.Contains("blog_post(id: 1)", query);
        }

        [Fact]
        public void Serialize_MultiArgumentWithoutProperty_AddsProperArgumentSyntax()
        {
            var query = new GraphQLQuery<BlogPost>()
                .WithArgument("id", 2)
                .WithArgument("before", "8/31/2020")
                .ToString();

            Assert.Contains("blog_post(id: 2, before: \"8/31/2020\"", query);
        }

        [Fact]
        public void Serialize_EnumArgument_AddsNameNotValue()
        {
            var query = new GraphQLQuery<BlogPost>()
                .WithArgument("category", Category.News)
                .ToString();

            Assert.Contains("blog_post(category: News", query);
        }

        [Fact]
        public void Serialize_OfType_OverridesAttributeTypeName()
        {
            var query = new GraphQLQuery<BlogPost>()
                .OfType("overriden_type")
                .ToString();

            Assert.Contains("overriden_type {", query);
        }

        [Fact]
        public void Serialize_WithJoin_AndAliases_IncludesBothQueries()
        {
            var newsQuery = new GraphQLQuery<BlogPost>()
                .WithAlias("news_articles")
                .WithArgument("category", Category.News);
            var artQuery = new GraphQLQuery<BlogPost>()
                .WithAlias("art_pieces")
                .WithArgument("category", Category.Art);

            var queryString = newsQuery.With(artQuery).ToString();

            Assert.Contains("news_articles: blog_post(category: News)", queryString);
            Assert.Contains("art_pieces: blog_post(category: Art)", queryString);
        }

        [Fact]
        public void Serialize_WithJoin_AndAmbiguousFields_ThrowsException()
        {
            Assert.Throws<AmbiguousFieldsException>(() =>
            {
                var newsQuery = new GraphQLQuery<BlogPost>()
                    .WithArgument("category", Category.News);
                var artQuery = new GraphQLQuery<BlogPost>()
                    .WithArgument("category", Category.Art);

                var queryString = newsQuery.With(artQuery).ToString();
            });
        }

        [Fact]
        public void Serialize_Map_UsesPropertyWithoutAttribute()
        {
            var query = new GraphQLQuery<BlogPost>()
                .WithMapping(x => x.UnMappedField, "unmapped_field")
                .ToString();

            Assert.Contains("unmapped_field", query);
        }

        [Fact]
        public void Serialize_WithoutOfType_AutomaticallyGuessesTypeInQuery()
        {
            var query = new GraphQLQuery<BlogPost>()
                .ToString();

            Assert.Contains("blog_post", query);
        }

        [Fact]
        public void Serialize_WithIgnore_DoesNotContainField()
        {
            var query = new GraphQLQuery<BlogPost>()
                .ShouldIgnore(x => x.Title)
                .ToString();

            Assert.DoesNotContain("full_name", query);
        }
    
        [GraphQLSchemaType("blog_post")]
        private class BlogPost
        {
            [GraphQLProperty("title")]
            public string Title { get; set; }
            
            [GraphQLProperty("category")]
            public Category Category { get; set; }

            [GraphQLProperty("author")]
            public Author Author { get; set; }

            public string UnMappedField { get; set; }
        }

        private class Author
        {
            [GraphQLProperty("display_name")]
            public string DisplayName { get; set; }
        }

        private enum Category
        {
            News = 0,
            Art = 1
        }
    }
}
