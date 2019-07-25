using System.Linq;
using Elsa.Models;
using Elsa.Services.Extensions;
using YesSql;
using YesSql.Indexes;

namespace Elsa.Persistence.YesSql.Indexes
{
    public class WorkflowInstanceIndex : MapIndex
    {
        public string WorkflowInstanceId { get; set; }
        public string WorkflowDefinitionId { get; set; }
        public WorkflowStatus WorkflowStatus { get; set; }
    }

    public class WorkflowInstanceBlockingActivitiesIndex : WorkflowInstanceIndex
    {
        public string ActivityId { get; set; }
        public string ActivityType { get; set; }
    }

    public class WorkflowInstanceIndexProvider : IndexProvider<WorkflowInstance>
    {
        public override void Describe(DescribeContext<WorkflowInstance> context)
        {
            context.For<WorkflowInstanceIndex>()
                .Map(
                    workflowInstance => new WorkflowInstanceIndex
                    {
                        WorkflowDefinitionId = workflowInstance.Id,
                        WorkflowStatus = workflowInstance.Status
                    }
                );

            context.For<WorkflowInstanceBlockingActivitiesIndex>()
                .Map(
                    workflowInstance => workflowInstance.BlockingActivities
                        .Select(
                            activity => new WorkflowInstanceBlockingActivitiesIndex
                            {
                                WorkflowInstanceId = workflowInstance.Id,
                                WorkflowDefinitionId = workflowInstance.Id,
                                ActivityId = activity.ActivityId,
                                ActivityType = activity.ActivityType
                            }
                        )
                );
        }
    }
}