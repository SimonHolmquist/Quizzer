using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;

namespace Quizzer.Application.Exams.Queries;

public sealed record GetLatestPublishedVersionQuery(Guid ExamId) : IRequest<PublishedVersionDto?>;

public sealed record PublishedVersionDto(Guid VersionId, int VersionNumber, DateTimeOffset PublishedAt);

public sealed class GetLatestPublishedVersionQueryHandler(IQuizzerDbContext db)
    : IRequestHandler<GetLatestPublishedVersionQuery, PublishedVersionDto?>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<PublishedVersionDto?> Handle(GetLatestPublishedVersionQuery request, CancellationToken ct)
    {
        var version = await _db.ExamVersions
            .AsNoTracking()
            .Where(v => v.ExamId == request.ExamId && v.Status == VersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct);

        return version is null
            ? null
            : new PublishedVersionDto(version.Id, version.VersionNumber, version.PublishedAt ?? version.CreatedAt);
    }
}
