using JoyReactor.Accordion.Logic.ApiClient;
using JoyReactor.Accordion.Logic.Database.Sql;
using JoyReactor.Accordion.Logic.Extensions;
using JoyReactor.Accordion.Logic.Parsers;
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class ParsedPostAttributePictureImageTypeFixer(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<BackgroundServiceSettings> settings,
    ILogger<ParsedPostAttributePictureImageTypeFixer> logger)
    : RobustBackgroundService(settings, logger)
{
    protected override bool IsIndefinite => true;

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        var postIds = (HashSet<Guid>)null;
        do
        {
            await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
            using var sqlDatabaseContext = serviceScope.ServiceProvider.GetService<SqlDatabaseContext>();
            var postClient = serviceScope.ServiceProvider.GetService<IPostClient>();
            var postParser = serviceScope.ServiceProvider.GetService<IPostParser>();

            postIds = await sqlDatabaseContext.ParsedPostAttributePictures
                .AsNoTracking()
                .Where(postAttribute => (int)postAttribute.ImageType > 6)
                .OrderBy(postAttribute => postAttribute.Id)
                .Take(100)
                .Select(postAttribute => postAttribute.PostId)
                .ToHashSetAsync(cancellationToken);
            if (postIds.Count == 0)
            {
                logger.LogInformation("No parsed post attribute pictures with broken image type found.");
                return;
            }
            logger.LogInformation("Found {PostCount} parsed post with broken attribute pictures image type.", postIds.Count);

            foreach (var postId in postIds)
            {
                var postNumberId = postId.ToInt();
                var post = await postClient.GetAsync(postNumberId, cancellationToken);
                await postParser.ParseAsync(post, cancellationToken);
                logger.LogInformation("Fixed broken attribute pictures image type in post {PostNumberId}.", postNumberId);
            }
        } while (postIds.Count != 0);
    }
}