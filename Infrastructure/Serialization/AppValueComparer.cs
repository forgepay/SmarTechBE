using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MicPic.Infrastructure.Serialization;

public class AppValueComparer<T>() : ValueComparer<T>(
    equalsExpression: (v1, v2) => string.Equals(AppJsonSerializer.SerializeOrNull(v1), AppJsonSerializer.SerializeOrNull(v2), StringComparison.Ordinal),
    hashCodeExpression: v => AppJsonSerializer.Serialize(v).GetHashCode(StringComparison.Ordinal),
    snapshotExpression: v => AppJsonSerializer.DeepCopy(v)
) where T : new()
{
    
}