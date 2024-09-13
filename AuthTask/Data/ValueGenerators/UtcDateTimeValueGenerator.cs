using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace AuthTask.Data.ValueGenerators
{
    public class UtcDateTimeValueGenerator : ValueGenerator<DateTimeOffset>
    {
        public override DateTimeOffset Next(EntityEntry entry) => DateTimeOffset.UtcNow;

        public override ValueTask<DateTimeOffset> NextAsync(EntityEntry entry,
            CancellationToken cancellationToken = new CancellationToken()) =>
            ValueTask.FromResult(DateTimeOffset.UtcNow);

        public override bool GeneratesTemporaryValues => false;
    }
}
