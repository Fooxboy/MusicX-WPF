namespace MusicX.Core.Models.Abstractions;

public interface IBlockEntityWithOwner<out TId, out TOwnerId> : IBlockEntity<TId>
{
    TOwnerId OwnerId { get; }
}