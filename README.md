# ![RealWorld Example App](logo.png)

> ### [Dotnet 8] codebase containing real world examples (CRUD, auth, advanced patterns, etc) that adheres to the [RealWorld](https://github.com/gothinkster/realworld) spec and API.

For a version using **dotnet minimal api's** check [here](https://github.com/Erikvdv/realworldapiminimal)

### [Demo](https://demo.realworld.io/)&nbsp;&nbsp;&nbsp;&nbsp;[RealWorld](https://github.com/gothinkster/realworld)

This codebase was created to demonstrate a fully fledged fullstack application built with **[Dotnet 8]** including CRUD operations, authentication, routing, pagination, and more.

We've gone to great lengths to adhere to the **[Dotnet 8]** community styleguides & best practices.

For more information on how to this works with other frontends/backends, head over to the [RealWorld](https://github.com/gothinkster/realworld) repo.

# How it works

Traditional Clean Architecture setup using Dotnet 6.
Consisting of the following layers:

- api
- core
- data
- infrastructure

Build using the following features:

- the new WebApplication.CreateBuilder(args)
- file scoped namespaces
- Entity Framework Core with Postgres db
- serilog for logging
- logging integrated with application insights, including realtime monitoring
- Hellang.Middleware.ProblemDetails for consistent error output
- sonarlint for code scanning

# Getting started

This solution uses Postgres.

### Install dotnet-ef.
`dotnet tool install --global dotnet-ef`

### Move to src/Data folder
`cd src/Data`

### Run db upgrade:
`dotnet ef database update --startup-project ../Api/Api.csproj`
