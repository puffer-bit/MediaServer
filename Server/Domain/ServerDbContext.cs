using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Domain.Entities;
using Server.MainServer.Main.Server;
using Server.MainServer.Main.Server.Coordinator;
using System;

namespace Server.Domain;

public class ServerDbContext : DbContext
{
    public DbSet<ServerInstance> ServerInstance { get; set; }
    public DbSet<CoordinatorInstance> CoordinatorInstances { get; set; }
    public DbSet<UserEntity> UserEntities { get; set; }
    public DbSet<VideoSessionEntity> VideoSessions { get; set; }

    public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options) { }

}