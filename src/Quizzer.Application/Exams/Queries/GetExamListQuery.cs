using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;

namespace Quizzer.Application.Exams.Queries;

public sealed record GetExamListQuery : IRequest<IReadOnlyList<ExamListItemDto>>;

public sealed record ExamListItemDto(Guid ExamId, string Name, int? LatestPublishedVersionNumber, bool HasDraft);

public sealed class GetExamListQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetExamListQuery, IReadOnlyList<ExamListItemDto>>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<IReadOnlyList<ExamListItemDto>> Handle(GetExamListQuery request, CancellationToken ct)
    {
        var exams = await _db.Exams
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        var versions = await _db.ExamVersions
            .AsNoTracking()
            .Where(v => exams.Select(e => e.Id).Contains(v.ExamId))
            .ToListAsync(ct);

        return [.. exams.Select(e =>
        {
            var vs = versions.Where(v => v.ExamId == e.Id).ToList();
            var latestPub = vs.Where(v => v.Status == VersionStatus.Published).OrderByDescending(v => v.VersionNumber).FirstOrDefault();
            var hasDraft = vs.Any(v => v.Status == VersionStatus.Draft);
            return new ExamListItemDto(e.Id, e.Name, latestPub?.VersionNumber, hasDraft);
        })];
    }
}
