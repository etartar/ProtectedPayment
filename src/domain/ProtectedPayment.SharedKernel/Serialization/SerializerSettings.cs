using Newtonsoft.Json;

namespace ProtectedPayment.SharedKernel.Serialization;

public static class SerializerSettings
{
    public static readonly JsonSerializerSettings Instance = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
    };
}
