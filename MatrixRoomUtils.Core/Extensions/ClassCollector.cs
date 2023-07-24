using System.Reflection;

namespace MatrixRoomUtils.Core.Extensions;

public class ClassCollector<T> where T : class {
    static ClassCollector() {
        if (!typeof(T).IsInterface)
            throw new ArgumentException(
                $"ClassCollector<T> must be used with an interface type. Passed type: {typeof(T).Name}");
    }

    public List<Type> ResolveFromAllAccessibleAssemblies() => AppDomain.CurrentDomain.GetAssemblies().SelectMany(ResolveFromAssembly).ToList();

    public List<Type> ResolveFromObjectReference(object obj) => ResolveFromTypeReference(obj.GetType());

    public List<Type> ResolveFromTypeReference(Type t) => Assembly.GetAssembly(t)?.GetReferencedAssemblies().SelectMany(ResolveFromAssemblyName).ToList() ?? new List<Type>();

    public List<Type> ResolveFromAssemblyName(AssemblyName assemblyName) => ResolveFromAssembly(Assembly.Load(assemblyName));

    public List<Type> ResolveFromAssembly(Assembly assembly) => assembly.GetTypes()
        .Where(x => x is { IsClass: true, IsAbstract: false } && x.GetInterfaces().Contains(typeof(T))).ToList();
}
