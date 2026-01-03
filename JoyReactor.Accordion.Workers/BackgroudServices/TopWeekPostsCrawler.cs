using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace JoyReactor.Accordion.Workers.BackgroudServices;

public class TopWeekPostsCrawler(
    IPostClient postClient,
    IPostParser postParser,
    ILogger<TopWeekPostsCrawler> logger)
    : ScopedBackgroudService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var startYear = 2009;
        var startWeek = 12;

        var endYear = DateTime.UtcNow.Date.Year;
        var endYearWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.UtcNow.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday) - 1;

        var nsfw = false;

        for (var year = startYear; year <= endYear; year++)
        {
            var yearLastWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(new DateTime(year, 12, 31), CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            for (var week = (year == startYear ? startWeek : 0); week <= (year == endYear ? endYearWeek : yearLastWeek); week++)
            {
                logger.LogInformation("Crawling {Week} week of {Year} year top {PostType}", week, year, nsfw ? "nsfw posts" : "posts");
                await CrawlAsync(year, week, nsfw, cancellationToken);
            }
        }
    }

    internal async Task CrawlAsync(int year, int week, bool nsfw, CancellationToken cancellationToken)
    {
        var posts = await postClient.GetWeekTopPostsAsync(year, week, nsfw, cancellationToken);
        logger.LogInformation("Found {PostCount} top {Week} week of {Year} year {PostType}", posts.Length, week, year, nsfw ? "nsfw posts" : "posts");

        await postParser.ParseAsync(posts, cancellationToken);
    }
}