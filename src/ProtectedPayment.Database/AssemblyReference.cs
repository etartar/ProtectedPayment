using System.Reflection;

namespace ProtectedPayment.Database;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
