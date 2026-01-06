
using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Extensions;
using JoyReactor.Accordion.Logic.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class CrawlerTaskHandler(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<BackgroundServiceSettings> settings,
    ILogger<CrawlerTaskHandler> logger)
    : RobustBackgroundService(settings, logger)
{
    protected override bool IsIndefinite => true;

    protected readonly ConcurrentDictionary<Guid, (Task Task, CancellationTokenSource Cts)> TaskWithCancellationTokenSources = new ConcurrentDictionary<Guid, (Task Task, CancellationTokenSource Cts)>();

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
        await using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();

        var crawlerTasks = await sqlDatabaseContext.CrawlerTasks
            .AsNoTracking()
            .Where(crawlerTask => crawlerTask.IsCompleted == false)
            .ToDictionaryAsync(crawlerTask => crawlerTask.Id, crawlerTask => crawlerTask, cancellationToken);

        var completedTasks = TaskWithCancellationTokenSources
            .Where(task => task.Value.Item1.IsCompleted)
            .ToArray();
        foreach (var (id, (task, cts)) in completedTasks)
        {
            if (task.IsFaulted)
                logger.LogError("Crawler task {CrawlerTaskId} failed", id, task.Exception);

            TaskWithCancellationTokenSources.TryRemove(id, out var _);
        }

        var newCrawlerTasks = crawlerTasks
            .Where(crawkerTask => !TaskWithCancellationTokenSources.ContainsKey(crawkerTask.Key))
            .ToArray();
        foreach (var (id, crawlerTask) in newCrawlerTasks)
        {
            logger.LogInformation("Starting {CrawlerTaskId} crawler task", id);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var ct = cts.Token;
            var task = CrawlAsync(crawlerTask.Id, ct);

            TaskWithCancellationTokenSources.TryAdd(id, new(task, cts));
        }

        var oldTasks = TaskWithCancellationTokenSources
            .Where(task => !crawlerTasks.ContainsKey(task.Key))
            .ToArray();
        foreach (var (id, (task, cts)) in oldTasks)
        {
            logger.LogInformation("Stopping {CrawlerTaskId} crawler task", id);
            await cts.CancelAsync();
        }
    }

    protected async Task CrawlAsync(Guid crawlerTaskId, CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
        await using var sqlDatabaseContext = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseContext>();
        var postClient = serviceScope.ServiceProvider.GetRequiredService<IPostClient>();
        var postParser = serviceScope.ServiceProvider.GetRequiredService<IPostParser>();

        var crawlerTask = await sqlDatabaseContext.CrawlerTasks.FirstAsync(c => c.Id == crawlerTaskId, cancellationToken);
        using (logger.BeginScope(new Dictionary<string, object>() { { "CrawlerTaskId", crawlerTask.Id } }))
        {
            crawlerTask.PageFrom ??= 1;
            crawlerTask.PageCurrent ??= crawlerTask.PageFrom;
            crawlerTask.StartedAt = DateTime.UtcNow;
            crawlerTask.UpdatedAt = DateTime.UtcNow;
            sqlDatabaseContext.CrawlerTasks.Update(crawlerTask);
            await sqlDatabaseContext.SaveChangesAsync(cancellationToken);

            var postPager = (PostPager)null;
            var isFullPage = false;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                var tagNumberId = crawlerTask.TagId.ToInt();
                postPager = await postClient.GetByTagAsync(tagNumberId, crawlerTask.PostLineType, crawlerTask.PageCurrent.Value, cancellationToken);
                logger.LogInformation("Found {PostCount} posts using {TagNumberId} tag id on {Page} page", postPager.Posts.Length, tagNumberId, crawlerTask.PageCurrent);

                await postParser.ParseAsync(postPager.Posts, cancellationToken);

                isFullPage = postPager.Posts.Length == 10;
                if (isFullPage)
                    crawlerTask.PageCurrent += 1;
                else
                {
                    crawlerTask.IsCompleted = crawlerTask.Type == CrawlerTaskType.OneTime;
                    crawlerTask.FinishedAt = DateTime.UtcNow;
                }
                crawlerTask.UpdatedAt = DateTime.UtcNow;
                sqlDatabaseContext.CrawlerTasks.Update(crawlerTask);
                await sqlDatabaseContext.SaveChangesAsync(cancellationToken);
            } while (isFullPage && crawlerTask.PageCurrent <= (crawlerTask.PageTo ?? int.MaxValue));

            logger.LogInformation("Crawler task {CrawlerTaskId} finished", crawlerTask.Id);
        }
    }
}