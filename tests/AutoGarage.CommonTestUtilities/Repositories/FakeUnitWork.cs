using AutoGarage.Domain.Repositories;

namespace AutoGarage.CommonTestUtilities.Repositories;

public class FakeUnitWork : IUnitWork
{
    public int CommitCount { get; private set; }

    public Task Commit()
    {
        CommitCount++;

        return Task.CompletedTask;
    }
}
