var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BasicAuthentication>("basicauthentication");
builder.AddProject<Projects.JwtAuthentication>("jwtauthentication");

builder.Build().Run();
