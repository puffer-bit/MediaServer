using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Domain.Entities;
using Server.MainServer.Main.Server.Coordinator;
using Server.MainServer.Main.Server.Orchestrator;
using System;
using System.Reflection;

namespace Server.Domain;

public class ServerDbContext : DbContext
{
    public DbSet<OrchestratorInstanceEntity> OrchestratorInstance { get; set; }
    public DbSet<CoordinatorInstanceEntity> CoordinatorInstances { get; set; }
    public DbSet<UserEntity> UserEntities { get; set; }
    public DbSet<VideoSessionEntity> VideoSessionsEntities { get; set; }

    public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options) { }

}