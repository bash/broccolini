using FsCheck.Xunit;

namespace Broccolini.Test;

public sealed class BroccoliniPropertyAttribute : PropertyAttribute
{
    public BroccoliniPropertyAttribute()
    {
        Arbitrary = [typeof(BroccoliniArbMap)];
    }
}
