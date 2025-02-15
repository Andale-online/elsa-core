using System.Collections.Generic;
using Elsa.Models;
using Elsa.Persistence.EntityFrameworkCore.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Elsa.Persistence.EntityFrameworkCore.DbContexts
{
    public class ElsaContext : DbContext
    {
        private readonly JsonSerializerSettings serializerSettings;

        public ElsaContext(DbContextOptions<ElsaContext> options) : base(options)
        {
            serializerSettings = new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public DbSet<WorkflowDefinitionVersionEntity> WorkflowDefinitionVersions { get; set; }
        public DbSet<WorkflowInstanceEntity> WorkflowInstances { get; set; }
        public DbSet<ActivityDefinitionEntity> ActivityDefinitions { get; set; }
        public DbSet<ConnectionDefinitionEntity> ConnectionDefinitions { get; set; }
        public DbSet<ActivityInstanceEntity> ActivityInstances { get; set; }
        public DbSet<BlockingActivityEntity> BlockingActivities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureWorkflowDefinitionVersion(modelBuilder);
            ConfigureWorkflowInstance(modelBuilder);
            ConfigureActivityDefinition(modelBuilder);
            ConfigureActivityInstance(modelBuilder);
            ConfigureBlockingActivity(modelBuilder);
        }
        
        private void ConfigureWorkflowDefinitionVersion(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<WorkflowDefinitionVersionEntity>();

            entity.HasKey(x => x.Id);
            entity.Property(x => x.DefinitionId);
            entity.Property(x => x.Variables).HasConversion(x => Serialize(x), x => Deserialize<Variables>(x));
            entity.HasMany(x => x.Activities).WithOne(x => x.WorkflowDefinitionVersion);
            entity.HasMany(x => x.Connections).WithOne(x => x.WorkflowDefinitionVersion);
        }

        private void ConfigureWorkflowInstance(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<WorkflowInstanceEntity>();

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>();
            
            entity
                .Property(x => x.Scopes)
                .HasConversion(
                    x => Serialize(x),
                    x => Deserialize<Stack<WorkflowExecutionScope>>(x)
                );
            
            entity
                .Property(x => x.ExecutionLog)
                .HasConversion(
                    x => Serialize(x),
                    x => Deserialize<ICollection<LogEntry>>(x)
                );

            entity
                .Property(x => x.Fault)
                .HasConversion(
                    x => Serialize(x),
                    x => Deserialize<WorkflowFault>(x)
                );
            
            entity
                .Property(x => x.Input)
                .HasConversion(
                    x => Serialize(x),
                    x => Deserialize<Variables>(x)
                );

            entity
                .HasMany(x => x.Activities)
                .WithOne(x => x.WorkflowInstance);
            
            entity
                .HasMany(x => x.BlockingActivities)
                .WithOne(x => x.WorkflowInstance);
        }
        
        private void ConfigureActivityDefinition(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<ActivityDefinitionEntity>();

            entity.HasKey(x => x.Id);

            entity
                .Property(x => x.State)
                .HasConversion(x => Serialize(x), x => Deserialize<JObject>(x));
        }
        
        private void ConfigureConnectionDefinition(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<ConnectionDefinitionEntity>();

            entity.HasKey(x => x.Id);
        }
        
        private void ConfigureActivityInstance(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<ActivityInstanceEntity>();

            entity.HasKey(x => x.Id);
            
            entity
                .Property(x => x.State)
                .HasConversion(x => Serialize(x), x => Deserialize<JObject>(x));
            
            entity
                .Property(x => x.Output)
                .HasConversion(x => Serialize(x), x => Deserialize<JObject>(x));
        }
        
        private void ConfigureBlockingActivity(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<BlockingActivityEntity>();

            entity.HasKey(x => x.Id);
        }

        private string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, serializerSettings);
        }

        private T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }
    }
}