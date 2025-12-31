using System.Reflection;

namespace SocialSim.Core.Neo4j.Cypher;

internal static class Neo4jCypherNaming
{
    public static string GetLabel(Type nodeType)
    {
        var attr = nodeType.GetCustomAttribute<Neo4jLabelAttribute>();
        return attr?.Label ?? nodeType.Name;
    }

    public static string GetRelationshipType(Type relationshipType)
    {
        var attr = relationshipType.GetCustomAttribute<Neo4jRelationshipTypeAttribute>();
        return attr?.Type ?? ToUpperSnakeCase(relationshipType.Name);
    }

    private static string ToUpperSnakeCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        var sb = new System.Text.StringBuilder(value.Length + 8);
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsUpper(c) && i > 0)
            {
                var prev = value[i - 1];
                var nextIsLower = i + 1 < value.Length && char.IsLower(value[i + 1]);
                if (char.IsLower(prev) || nextIsLower)
                {
                    sb.Append('_');
                }
            }

            sb.Append(char.ToUpperInvariant(c));
        }

        return sb.ToString();
    }
}
