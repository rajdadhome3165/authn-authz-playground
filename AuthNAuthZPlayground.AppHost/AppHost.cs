var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BasicAuthentication>("basicauthentication");

builder.Build().Run();
