using AutoGarage.Domain.Repositories;

namespace AutoGarage.CommonTestUtilities.Repositories;

public class FakeUnitWork : IUnitWork
{
    public int CommitCount { get; private set; }

    public Exception? ThrowOnCommit { get; set; }

    public Task Commit()
    {
        if (ThrowOnCommit is not null)
        {
            throw ThrowOnCommit;
        }

        CommitCount++;

        return Task.CompletedTask;
    }
}
