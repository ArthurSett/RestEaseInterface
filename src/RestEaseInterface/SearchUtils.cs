using System.Reflection;
using RestEase;

namespace RestEaseInterface; 

public static class SearchUtils {
    public static IEnumerable<(Type, TypeInfo)> CollectRestEaseInterfaceServices(this Assembly[] assemblies) {
        var allTypes = assemblies
            .SelectMany(assembly => assembly.DefinedTypes).ToArray();
        return allTypes
            .SelectMany(type => type.GetInterfaces())
            .Distinct()
            .Where(type => {
                var methodInfos = type.GetMethods();
                return methodInfos.Length != 0 && methodInfos
                    .All(method =>
                        method.GetCustomAttributes(typeof(RequestAttributeBase), true).Any()
                        && (method.ReturnType == typeof(Task) || method.ReturnType.BaseType == typeof(Task)));
            })
            .Select(x => (x, 
                allTypes.FirstOrDefault(x.IsAssignableFrom)))
            .Where(x => x.Item2 != null)!;
    }
}