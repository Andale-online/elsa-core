﻿using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Activities.Email.Options;
using Elsa.Expressions;
using Elsa.Extensions;
using Elsa.Results;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.Extensions.Options;

namespace Elsa.Activities.Email.Activities
{
    public class SendEmail : Activity
    {
        private readonly IWorkflowExpressionEvaluator expressionEvaluator;
        private readonly IOptions<SmtpOptions> options;
        private readonly SmtpClient smtpClient;

        public SendEmail(
            IWorkflowExpressionEvaluator expressionEvaluator, 
            IOptions<SmtpOptions> options,
            SmtpClient smtpClient)
        {
            this.expressionEvaluator = expressionEvaluator;
            this.options = options;
            this.smtpClient = smtpClient;
        }

        public WorkflowExpression<string> From
        {
            get => GetState(() => new WorkflowExpression<string>(LiteralEvaluator.SyntaxName, ""));
            set => SetState(value);
        }

        public WorkflowExpression<string> To
        {
            get => GetState(() => new WorkflowExpression<string>(LiteralEvaluator.SyntaxName, ""));
            set => SetState(value);
        }

        public WorkflowExpression<string> Subject
        {
            get => GetState(() => new WorkflowExpression<string>(LiteralEvaluator.SyntaxName, ""));
            set => SetState(value);
        }

        public WorkflowExpression<string> Body
        {
            get => GetState(() => new WorkflowExpression<string>(LiteralEvaluator.SyntaxName, ""));
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext workflowContext,
            CancellationToken cancellationToken)
        {
            var from = (await expressionEvaluator.EvaluateAsync(From, workflowContext, cancellationToken)) ?? options.Value.DefaultSender;
            var to = await expressionEvaluator.EvaluateAsync(To, workflowContext, cancellationToken);
            var subject = await expressionEvaluator.EvaluateAsync(Subject, workflowContext, cancellationToken);
            var body = await expressionEvaluator.EvaluateAsync(Body, workflowContext, cancellationToken);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(@from),
                Body = body,
                Subject = subject,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);
            await smtpClient.SendMailAsync(mailMessage);

            return Done();
        }
    }
}