namespace Bridge.Domain.Interfaces;

public interface IExpirable
{
    DateTimeOffset ExpiredAt { get; set; }
}