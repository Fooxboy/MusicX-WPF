namespace MusicX.Core.Models.Abstractions;

public interface IBlockEntity<out T>
{
    T Id { get; }
}