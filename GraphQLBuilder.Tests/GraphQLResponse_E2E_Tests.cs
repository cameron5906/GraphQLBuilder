using GraphQLBuilder.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace GraphQLBuilder.Tests
{
    public class GraphQLResponse_E2E_Tests
    {
        [Fact]
        public void Deserialize_DeserializesModel()
        {
            var response = "{\"blogpost\":{\"title\": \"test\", \"author\":{\"display_name\":\"John Doe\"}}}";
            var blogPost = new GraphQLResponse<BlogPost>(response).Parse();
            Assert.Equal("test", blogPost.Title);
            Assert.Equal("John Doe", blogPost.Author.DisplayName);
        }

        [Fact]
        public void DeserializeArray_DeserializesModelArray()
        {
            var response = "{\"blogpost\":[{\"title\": \"test\", \"author\":{\"display_name\":\"John Doe\"}}, {\"title\": \"test\", \"author\":{\"display_name\":\"John Doe\"}}]}";
            var blogPost = new GraphQLResponse<BlogPost>(response).ParseArray();
            Assert.Equal(2, blogPost.Length);
        }

        [Fact]
        public void Deserializes_DeserializesEnum()
        {
            var response = "{\"blogpost\":{\"title\": \"test\", \"category\": 0, \"author\":{\"display_name\":\"John Doe\"}}}";
            var blogPost = new GraphQLResponse<BlogPost>(response).Parse();
            Assert.Equal(Category.News, blogPost.Category);
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
