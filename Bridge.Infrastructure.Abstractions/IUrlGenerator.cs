namespace Bridge.Infrastructure.Abstractions;

public interface IUrlGenerator
{
    string GetFrontendRoomUrl(Guid roomId);
}