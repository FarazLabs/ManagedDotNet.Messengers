using System.Reflection;

namespace ManagedDotNet.Messengers.Core.Telegram.Mapping;

/// <summary>
/// Discovers <see cref="TelegramMapper{TUpdate}"/> implementations in this assembly
/// and resolves them by Telegram update payload type.
/// </summary>
internal static class TelegramMapperRegistry
{
    private static readonly Dictionary<Type, ITelegramMapper> s_mappers;

    static TelegramMapperRegistry()
    {
        s_mappers = new Dictionary<Type, ITelegramMapper>();
        var assembly = typeof(TelegramMapperRegistry).Assembly;

        foreach (Type type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            Type? baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType &&
                    baseType.GetGenericTypeDefinition() == typeof(TelegramMapper<>))
                {
                    Type? genericArgument = baseType.GetGenericArguments().FirstOrDefault();
                    if (genericArgument is null)
                        break;

                    object? created = Activator.CreateInstance(type);
                    if (created is not ITelegramMapper mapper)
                    {
                        throw new InvalidOperationException(
                            $"Type {type} must implement ITelegramMapper and have a public parameterless constructor.");
                    }

                    s_mappers[genericArgument] = mapper;
                    break;
                }

                baseType = baseType.BaseType;
            }
        }
    }

    public static ITelegramMapper? FetchMapper(Type type)
    {
        s_mappers.TryGetValue(type, out ITelegramMapper? mapper);
        return mapper;
    }
}
