# Buddy API

This is a framework built in dotnet core that helps you develop very quickly a back-end application to support CRUD operations and SPA serving.
They way it works:
 - Define you model, the way you want, usign the data annotations you are used to
 - Creates a controller that inherits from a BuddyController (check the database packages for one that suits you) and let it know the model it has to work with
 - Setup your project to use the database as you are used to
 - Add BuddyAPI to you Startup class (use, add)
 - That's it

The Buddy API Controllers will automatically:
 - (if SQL) find the right table for the model
 - (if mongo) find/create a collection specific for that model
 - handle creation
 - handle deletion
 - handle update
 - handle user access
 - handle listing with pagination built-in

You are able to easily modify those behaviours through methods overrides (either in the services or in controllers).

If you want to modify any specific part of the pipeline you can by overriding the methods of your controller that handle that specific part. You can hijack your code in pretty much part

## Documentation

The documentation is generated with DocFX. In oder to run it you must first have it installed in your machine (once docker is added to the project we will have a container for building the documentation files).
DocFx will parse the tripple slash comments `/// comment` from code and generate the documentation with them.

To generate the docs just:
```sh
$ docfx ./docfx_project/docfx.json
```

If you are running linux or macos you might install the mono runtime and run docfx using mono:
```sh
$ mono docfx.exe ./docfx_project/docfx.json
```

The generated static website will be in: `.\docfx_project\_site`.

## Production

This package is being used in production in 2 projects already, you can use in yours too

## Open Source

This project is open source, and PRs are very welcome.
For now I'm using a private solution for testing (not available to public yet) but in the future I'll require tests with every PR.

## Tutorials / Docs

TBC
