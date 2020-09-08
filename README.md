# GraphQLBuilder

A library for generating and consuming GraphQL queries and responses using a builder pattern and **no additional dependencies**. Heavily tested.

This is **not** a *GraphQL client* library - it is recommended you use something like [graphql-client](https://github.com/graphql-dotnet/graphql-client) for that. This library simply serves the purpose of building queries based off of your domain models that you can send directly to an API, and reading in the response so it is mapped back to the domain models.

## How It Works - Data Attributes

There are a set of attributes that you can apply to your models to make the process of serializing and deserializing simple and model-driven:

#### **[GraphQLProperty(name)]** 

Maps the given property to a remote schema field name. Unspecified properties will not be serialized or mapped to when deserializing

#### **[GraphQLSchemaType(name)]** 

Maps a domain model class to a remote schema type. If not specified, assumes the remote schema type to be named in snake case (i.e BlogPost - blog_post)

#### **[GraphQLIgnore]** 

Property will be ignored when serializing

## Query Building Examples

Specifying arguments and naming your operation

```csharp
var query = new GraphQLQuery<BlogPost>()
    .WithArgument("datePublishedBefore", "9/1/2020")
    .AsOperation("GetPostsBeforeSeptember")
    .Build();

query GetPostsBeforeSeptember {
    blogpost(datePublishedBefore: "9/1/2020") {
        title,
        author {
            username
        }
    }
}
```

Retrieving multiple types

```csharp
var query = new GraphQLQuery<User>()
    .WithArgument("role", RoleEnum.Admin)
    .WithAlias("admins")
    .With(new GraphQLQuery<User>()
    	.WithArgumeent("role", RoleEnum.Moderator)
      	.WithAlias("moderators")
   	)
    .Build();

{
    admins: user(role: Admin) {
        username
    }
    moderators: user(role: Moderator) {
        username
    }
}
```

In the event you can't apply the [GraphQLProperty] attribute to something in your model, you can declare it while building your query:

```csharp
var query = new GraphQLQuery<User>()
    .WithMapping(x => x.Name, "username")
    .Build();
```

Likewise, if you don't apply the [GraphQLSchemaType] attribute, you can declare the remote name of the type like so:

```csharp
var query = new GraphQLQuery<User>()
    .OfType("accounts")
    .Build();
```

If you don't want a specific property in the query, you can ignore it:

```csharp
var query = new GraphQLQuery<User>()
    .ShouldIgnore(x => x.SecretPasswordThatShouldNotBeQueryableAnyway)
    .Build();
```

## Query Response Parsing

Naturally, we want to be able to handle responses coming out of a GraphQL API as well. Since we are not relying on any additional dependencies, there is a GraphQLResponse class that will deserialize json to your domain model using the GraphQL attributes we've already implemented.

To read in a single object:

```csharp
var user = new GraphQLResponse<User>(json).Parse();
```

And for array types:

```csharp
var users = new GraphQLResponse<User>(json).ParseArray();
```



## Supported Data Types

string

double

float

short

int

long

enum