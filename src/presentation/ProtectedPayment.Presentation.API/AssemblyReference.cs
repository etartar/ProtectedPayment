using System.Reflection;

namespace ProtectedPayment.Presentation.API;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
