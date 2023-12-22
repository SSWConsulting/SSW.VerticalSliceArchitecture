[![VerticalSliceArchitecture Template Banner](https://raw.githubusercontent.com/Hona/VerticalSliceArchitecture/main/docs/banner.png)](https://github.com/Hona/VerticalSliceArchitecture)

# 🤔 What is it?

A small but opinionated Vertical Slice Architecture solution template for .NET 8

## Learn

[![](https://img.shields.io/badge/watch%20the%20video-FF0000?style=for-the-badge&logo=youtube)](https://www.youtube.com/watch?v=T-EwN9UqRwE) [![](https://img.shields.io/badge/Read%20the%20Blog-06D6A0?style=for-the-badge&logo=rss&logoColor=fff)](http://lukeparker.dev/blog/vertical-slice-architecture-quick-start)

[![Vertical Slice Architecture: How Does it Compare to Clean Architecture | .NET Conf 2023](https://i3.ytimg.com/vi/T-EwN9UqRwE/maxresdefault.jpg)
](https://www.youtube.com/watch?v=T-EwN9UqRwE)

# 🎉 Getting Started

To install the template from NuGet.org run the following command:

```bash
dotnet new install Hona.VerticalSliceArchitecture.Template
```

Then create a new solution:

```bash
mkdir Sprout
cd Sprout

dotnet new hona-vsa
```

Finally, to update the template to the latest version run:

```bash
dotnet new update
```

# 📚 Faster Development

To speed up development there is a `dotnet new` template to create a full Vertical Slice.

```bash
dotnet new hona-vsa-slice -f Student
```
`-f` or `--feature` where the feature name is the **singular** name of the feature.

Of course, there are always exceptions where appending an 's' is not enough. For example, `Person` becomes `People` and `Child` becomes `Children`.

For this, use the optional parameter:

```bash
dotnet new hona-vsa-slice -f Person -fp People
```

optional: `-fp` or `--feature-plural` where the feature name is the **plural** name of the feature.

This creates everything you need to get started with a new feature.

- Full CRUD endpoints
- CQRS 
    - missing `EventHandlers/` folder as this is more uncommon
    - provides `Events/` as a folder to demonstrate how to trigger side effects
- Basic REPR pattern (i.e., Request, an Endpoint, and a Response)
- Adds a new Entity
- Adds a DbSet to the DbContext
- Adds EF Core Entity Type Configuration
- Adds a Repository

# ✨ Features

- C# 12
- .NET 8
- ASP.NET Core
- Minimal APIs
- EF Core
- Swagger UI
