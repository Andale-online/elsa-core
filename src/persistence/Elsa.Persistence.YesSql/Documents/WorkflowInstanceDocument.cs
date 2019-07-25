using System.Collections.Generic;
using Elsa.Models;
using NodaTime;

namespace Elsa.Persistence.YesSql.Documents
{
    public class WorkflowInstanceDocument
    {
        public int Id { get; set; }
        public string WorkflowInstanceId { get; set; }
        public string DefinitionId { get; set; }
        public WorkflowStatus Status { get; set; }
        public Instant CreatedAt { get; set; }
        public Instant? StartedAt { get; set; }
        public Instant? HaltedAt { get; set; }
        public Instant? FinishedAt { get; set; }
        public IDictionary<string, ActivityInstance> Activities { get; set; } = new Dictionary<string, ActivityInstance>();
        public Stack<WorkflowExecutionScope> Scopes { get; set; }
        public Variables Input { get; set; }
        public HashSet<BlockingActivity> BlockingActivities { get; set; }
        public ICollection<LogEntry> ExecutionLog { get; set; }
        public WorkflowFault Fault { get; set; }
    }
}